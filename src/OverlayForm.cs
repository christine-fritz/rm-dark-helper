using System;
using System.Drawing;
using System.Windows.Forms;

namespace RMDarkHelper
{
    internal sealed class OverlayForm : Form
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const int GWL_EXSTYLE = -20;
        private const int GWLP_HWNDPARENT = -8;

        private const uint LWA_ALPHA = 0x00000002;

        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private IntPtr _magnifier = IntPtr.Zero;
        private IntPtr _ownerTarget = IntPtr.Zero;
        private DarkMode _mode = DarkMode.SoftDark;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                cp.ExStyle |=
                    WS_EX_TRANSPARENT |
                    WS_EX_TOOLWINDOW |
                    WS_EX_LAYERED |
                    WS_EX_NOACTIVATE;

                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            NativeMethods.SetLayeredWindowAttributes(
                Handle,
                0,
                255,
                LWA_ALPHA);

            _magnifier = NativeMethods.CreateWindowEx(
                0,
                "Magnifier",
                "RMDarkHelperMagnifier",
                WS_CHILD | WS_VISIBLE,
                0,
                0,
                Math.Max(1, ClientSize.Width),
                Math.Max(1, ClientSize.Height),
                Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            if (_magnifier == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Magnifier-Control konnte nicht erstellt werden.");

            IntPtr ex = NativeMethods.GetWindowLongPtr(
                _magnifier,
                GWL_EXSTYLE);

            long exValue = ex.ToInt64();
            exValue |= WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;

            NativeMethods.SetWindowLongPtr(
                _magnifier,
                GWL_EXSTYLE,
                new IntPtr(exValue));

            NativeMethods.MAGTRANSFORM transform =
                NativeMethods.IdentityTransform();

            NativeMethods.MagSetWindowTransform(
                _magnifier,
                ref transform);

            IntPtr[] initialExclude = new IntPtr[] { Handle };

            NativeMethods.MagSetWindowFilterList(
                _magnifier,
                0,
                initialExclude.Length,
                initialExclude);

            ApplyColorEffect();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DetachFromTarget();

            if (_magnifier != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(_magnifier);
                _magnifier = IntPtr.Zero;
            }

            base.OnFormClosed(e);
        }

        public void SetMode(DarkMode mode)
        {
            _mode = mode;

            if (_magnifier != IntPtr.Zero)
                ApplyColorEffect();
        }

        public void SetExcludedWindows(IntPtr[] windows)
        {
            if (_magnifier == IntPtr.Zero ||
                windows == null ||
                windows.Length == 0)
                return;

            NativeMethods.MagSetWindowFilterList(
                _magnifier,
                0,
                windows.Length,
                windows);
        }

        public void AttachToTarget(IntPtr target)
        {
            if (target == IntPtr.Zero || target == _ownerTarget)
                return;

            NativeMethods.SetWindowLongPtr(
                Handle,
                GWLP_HWNDPARENT,
                target);

            _ownerTarget = target;
        }

        public void DetachFromTarget()
        {
            if (_ownerTarget == IntPtr.Zero)
                return;

            NativeMethods.SetWindowLongPtr(
                Handle,
                GWLP_HWNDPARENT,
                IntPtr.Zero);

            _ownerTarget = IntPtr.Zero;
        }

        public void UpdateTarget(NativeMethods.RECT sourceRect)
        {
            int width = sourceRect.Right - sourceRect.Left;
            int height = sourceRect.Bottom - sourceRect.Top;

            NativeMethods.SetWindowPos(
                Handle,
                IntPtr.Zero,
                sourceRect.Left,
                sourceRect.Top,
                width,
                height,
                SWP_NOZORDER |
                SWP_NOACTIVATE |
                SWP_SHOWWINDOW);

            if (_magnifier != IntPtr.Zero)
            {
                NativeMethods.SetWindowPos(
                    _magnifier,
                    IntPtr.Zero,
                    0,
                    0,
                    width,
                    height,
                    SWP_NOZORDER |
                    SWP_NOACTIVATE);

                NativeMethods.MagSetWindowSource(
                    _magnifier,
                    sourceRect);
            }
        }

        private void ApplyColorEffect()
        {
            NativeMethods.MAGCOLOREFFECT effect;

            if (_mode == DarkMode.Invert)
            {
                effect = NativeMethods.CreateColorEffect(
                    -1f,  0f,  0f, 0f, 0f,
                     0f, -1f,  0f, 0f, 0f,
                     0f,  0f, -1f, 0f, 0f,
                     0f,  0f,  0f, 1f, 0f,
                     1f,  1f,  1f, 0f, 1f);
            }
            else
            {
                effect = NativeMethods.CreateColorEffect(
                     0.634328f, -0.365672f, -0.365672f, 0f, 0f,
                    -1.230144f, -0.230144f, -1.230144f, 0f, 0f,
                    -0.124184f, -0.124184f,  0.875816f, 0f, 0f,
                     0f,         0f,         0f,        1f, 0f,
                     0.78f,      0.78f,      0.78f,     0f, 1f);
            }

            NativeMethods.MagSetColorEffect(
                _magnifier,
                ref effect);
        }
    }
}
