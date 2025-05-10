using System;
using System.Drawing;
using System.Windows.Forms;
using Common;
using NES;
using TGL;

namespace NES
{
    public partial class GLView : UserControl
    {
        public GLContext Context = new GLContext();
        public TPixmap FrameBuffer;

        public GLView()
        {
            InitializeComponent();
            Context.View = this;
            ResizeRedraw = true;
            SetStyle(ControlStyles.Opaque, true);
        }

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
