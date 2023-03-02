using System;
using System.Windows.Forms;
using Common;

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
            }
        }

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
