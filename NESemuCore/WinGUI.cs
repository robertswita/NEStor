using NESemuGUI;
using System.Windows.Forms;

namespace NESemuCore
{
    class WinGUI
    {
        public FormMain form;

        private bool _ready = false;

        public void GUI()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new FormMain();
            _ready = true;
            Application.Run(form);
        }

        public bool isReady()
        {
            return _ready;
        }
    }
}
