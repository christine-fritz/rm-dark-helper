using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RMDarkHelper
{
    internal sealed class ControllerForm : Form
    {
        private const int HOTKEY_ID = 0x524D44;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const int WM_HOTKEY = 0x0312;

        private readonly System.Windows.Forms.Timer _timer;
        private readonly NotifyIcon _tray;
        private readonly ToolStripMenuItem _softItem;
        private readonly ToolStripMenuItem _invertItem;
        private readonly ToolStripMenuItem _enabledItem;
        private readonly ToolStripMenuItem _startWithWindowsItem;

        private readonly Dictionary<IntPtr, OverlayForm> _overlays =
            new Dictionary<IntPtr, OverlayForm>();

        private bool _enabled = true;
        private DarkMode _mode = DarkMode.SoftDark;

        public ControllerForm()
        {
            Text = "RM Dark Helper Controller";
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-32000, -32000);
            Size = new Size(1, 1);
            Opacity = 0;

            ContextMenuStrip menu = new ContextMenuStrip();

            _enabledItem = new ToolStripMenuItem("Dark mode enabled");
            _enabledItem.Checked = true;
            _enabledItem.Click += delegate { ToggleEnabled(); };

            _softItem = new ToolStripMenuItem("Soft Dark");
            _softItem.Checked = true;
            _softItem.Click += delegate
            {
                _mode = DarkMode.SoftDark;
                UpdateModeChecks();
                ApplyModeToAll();
            };

            _invertItem = new ToolStripMenuItem("Invert");
            _invertItem.Click += delegate
            {
                _mode = DarkMode.Invert;
                UpdateModeChecks();
                ApplyModeToAll();
            };

            _startWithWindowsItem = new ToolStripMenuItem("Start with Windows");
            _startWithWindowsItem.Checked = IsStartWithWindowsEnabled();
            _startWithWindowsItem.Click += delegate
            {
                SetStartWithWindows(!_startWithWindowsItem.Checked);
            };

            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += delegate
            {
                MessageBox.Show(
                    "RM Dark Helper 1.0.0\n\n" +
                    "Unofficial dark-mode overlay for RootsMagic 11 on Windows.\n\n" +
                    "Independent project. Not affiliated with or endorsed by RootsMagic, Inc.\n" +
                    "RootsMagic is a trademark of RootsMagic, Inc.",
                    "About RM Dark Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += delegate { Close(); };

            menu.Items.Add(_enabledItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_softItem);
            menu.Items.Add(_invertItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_startWithWindowsItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Hotkey: Ctrl+Alt+D") { Enabled = false });
            menu.Items.Add(aboutItem);
            menu.Items.Add(exitItem);

            _tray = new NotifyIcon();
            _tray.Icon = SystemIcons.Application;
            _tray.Text = "RM Dark Helper";
            _tray.Visible = true;
            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += delegate { ToggleEnabled(); };

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 100;
            _timer.Tick += TimerTick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Hide();

            NativeMethods.RegisterHotKey(
                Handle,
                HOTKEY_ID,
                MOD_CONTROL | MOD_ALT,
                (uint)Keys.D);

            _timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            NativeMethods.UnregisterHotKey(Handle, HOTKEY_ID);

            foreach (OverlayForm overlay in _overlays.Values)
            {
                try
                {
                    overlay.Close();
                    overlay.Dispose();
                }
                catch { }
            }
            _overlays.Clear();

            _tray.Visible = false;
            _tray.Dispose();

            base.OnFormClosed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleEnabled();
                return;
            }

            base.WndProc(ref m);
        }

        private static string GetStartupShortcutPath()
        {
            string startupFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.Startup);

            return Path.Combine(startupFolder, "RM Dark Helper.lnk");
        }

        private static bool IsStartWithWindowsEnabled()
        {
            string shortcutPath = GetStartupShortcutPath();

            if (!File.Exists(shortcutPath))
                return false;

            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");

                if (shellType == null)
                    return false;

                object shellObject = Activator.CreateInstance(shellType);
                dynamic shell = shellObject;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                string targetPath = shortcut.TargetPath as string;

                try { Marshal.FinalReleaseComObject(shortcut); } catch { }
                try { Marshal.FinalReleaseComObject(shellObject); } catch { }

                return string.Equals(
                    Path.GetFullPath(targetPath ?? ""),
                    Path.GetFullPath(Application.ExecutablePath),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void SetStartWithWindows(bool enabled)
        {
            string shortcutPath = GetStartupShortcutPath();

            try
            {
                if (enabled)
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");

                    if (shellType == null)
                        throw new InvalidOperationException(
                            "Windows Script Host is unavailable.");

                    object shellObject = Activator.CreateInstance(shellType);
                    dynamic shell = shellObject;
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);

                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WorkingDirectory =
                        Path.GetDirectoryName(Application.ExecutablePath);
                    shortcut.Description = "RM Dark Helper";
                    shortcut.IconLocation = Application.ExecutablePath + ",0";
                    shortcut.Save();

                    try { Marshal.FinalReleaseComObject(shortcut); } catch { }
                    try { Marshal.FinalReleaseComObject(shellObject); } catch { }
                }
                else
                {
                    if (File.Exists(shortcutPath))
                        File.Delete(shortcutPath);
                }

                _startWithWindowsItem.Checked =
                    IsStartWithWindowsEnabled();
            }
            catch (Exception ex)
            {
                _startWithWindowsItem.Checked =
                    IsStartWithWindowsEnabled();

                MessageBox.Show(
                    "The Windows startup setting could not be changed.\n\n" +
                    ex.Message,
                    "RM Dark Helper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ToggleEnabled()
        {
            _enabled = !_enabled;
            _enabledItem.Checked = _enabled;

            if (!_enabled)
                HideAll();
        }

        private void UpdateModeChecks()
        {
            _softItem.Checked = _mode == DarkMode.SoftDark;
            _invertItem.Checked = _mode == DarkMode.Invert;
        }

        private void ApplyModeToAll()
        {
            foreach (OverlayForm overlay in _overlays.Values)
                overlay.SetMode(_mode);
        }

        private void HideAll()
        {
            foreach (OverlayForm overlay in _overlays.Values)
            {
                if (overlay.Visible)
                    overlay.Hide();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!_enabled)
            {
                HideAll();
                return;
            }

            List<IntPtr> targets = FindRootsMagicWindows();
            HashSet<IntPtr> targetSet = new HashSet<IntPtr>(targets);

            List<IntPtr> stale = new List<IntPtr>();

            foreach (KeyValuePair<IntPtr, OverlayForm> kv in _overlays)
            {
                if (!targetSet.Contains(kv.Key) || !NativeMethods.IsWindow(kv.Key))
                    stale.Add(kv.Key);
            }

            foreach (IntPtr hwnd in stale)
            {
                OverlayForm oldOverlay = _overlays[hwnd];

                try
                {
                    oldOverlay.Close();
                    oldOverlay.Dispose();
                }
                catch { }

                _overlays.Remove(hwnd);
            }

            foreach (IntPtr target in targets)
            {
                NativeMethods.RECT rect;

                if (!NativeMethods.GetWindowRect(target, out rect))
                    continue;

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                if (width < 80 || height < 60)
                    continue;

                OverlayForm overlay;

                if (!_overlays.TryGetValue(target, out overlay))
                {
                    overlay = new OverlayForm();
                    overlay.SetMode(_mode);
                    _overlays[target] = overlay;
                }

                overlay.AttachToTarget(target);
                overlay.UpdateTarget(rect);

                if (!overlay.Visible)
                    overlay.Show();
            }

            IntPtr[] overlayHandles = new IntPtr[_overlays.Count];
            int i = 0;

            foreach (OverlayForm overlay in _overlays.Values)
                overlayHandles[i++] = overlay.Handle;

            foreach (OverlayForm overlay in _overlays.Values)
                overlay.SetExcludedWindows(overlayHandles);
        }

        private List<IntPtr> FindRootsMagicWindows()
        {
            HashSet<uint> rootsMagicPids = new HashSet<uint>();
            uint ownPid = (uint)Process.GetCurrentProcess().Id;

            try
            {
                Process[] processes = Process.GetProcesses();

                foreach (Process p in processes)
                {
                    try
                    {
                        if ((uint)p.Id == ownPid)
                            continue;

                        string name = p.ProcessName ?? "";

                        if (string.Equals(name, "RootsMagic", StringComparison.OrdinalIgnoreCase))
                            rootsMagicPids.Add((uint)p.Id);
                    }
                    catch { }
                    finally
                    {
                        try { p.Dispose(); } catch { }
                    }
                }
            }
            catch { }

            List<IntPtr> windows = new List<IntPtr>();

            if (rootsMagicPids.Count == 0)
                return windows;

            NativeMethods.EnumWindows(delegate (IntPtr hwnd, IntPtr lParam)
            {
                if (!NativeMethods.IsWindowVisible(hwnd))
                    return true;

                if (NativeMethods.IsIconic(hwnd))
                    return true;

                uint pid;
                NativeMethods.GetWindowThreadProcessId(hwnd, out pid);

                if (pid == ownPid)
                    return true;

                if (!rootsMagicPids.Contains(pid))
                    return true;

                NativeMethods.RECT rect;

                if (!NativeMethods.GetWindowRect(hwnd, out rect))
                    return true;

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                if (width < 80 || height < 60)
                    return true;

                windows.Add(hwnd);
                return true;

            }, IntPtr.Zero);

            return windows;
        }
    }
}
