using System;
using System.Collections.Generic;
using System.Text;
using guiTest;
using System.Windows.Forms;

namespace emulatorTest
{
    class Threads
    {
        public Form1 form;

        public bool dziala = false;
        public void GUI()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            dziala = true;
            Application.Run(form);
        }
    }
}
