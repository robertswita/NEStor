using NESemuGUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace emulatorTest
{
    class Threads
    {
        public FormMain form;

        public bool dziala = false;
        public void GUI()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new FormMain();
            dziala = true;
            Application.Run(form);
        }
    }
}
