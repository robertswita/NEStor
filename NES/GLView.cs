using System;
using System.Drawing;
using System.Windows.Forms;
using Common;
using NES;
using TGL;

namespace NESemuGUI
{
    public partial class GLView : UserControl
    {
        public GLContext Context = new GLContext();
        TPixmap _FrameBuffer;
        public TPixmap FrameBuffer
        {
            get { return _FrameBuffer; }
            set
            {
                _FrameBuffer = value;
                Invalidate();
                //OnPaint(null);
            }
        }

        public GLView()
        {
            InitializeComponent();
            Context.View = this;
            ResizeRedraw = true;
            SetStyle(ControlStyles.Opaque, true);
        }

        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == 0xF) // WM_PAINT
        //    {
        //        Context.DrawView();
        //        Win32.ValidateRect(Handle, IntPtr.Zero);
        //        //DefWndProc(ref m);
        //        return;
        //    }
        //    base.WndProc(ref m);
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            Context.DrawView();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= (int)Win32.CS_OWNDC;
                return cp;
            }
        }
    }
}
