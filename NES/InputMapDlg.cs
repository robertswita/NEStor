using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NES
{
    public partial class InputMapDlg : Form
    {
        public int[] KeyMap;
        public InputMapDlg()
        {
            InitializeComponent();
        }

        private void InputMapDlg_Shown(object sender, EventArgs e)
        {
            var keys = new string[KeyMap.Length];
            for (int i = 0; i < KeyMap.Length; i++)
                keys[i] = ((Keys)KeyMap[i]).ToString();
            RightBox.Text = keys[0];
            LeftBox.Text = keys[1];
            DownBox.Text = keys[2];
            UpBox.Text = keys[3];
            StartBox.Text = keys[4];
            SelectBox.Text = keys[5];
            BBox.Text = keys[6];
            ABox.Text = keys[7];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            KeyMap = new int[] {
                (int)Enum.Parse(typeof(Keys), RightBox.Text),
                (int)Enum.Parse(typeof(Keys), LeftBox.Text),
                (int)Enum.Parse(typeof(Keys), DownBox.Text),
                (int)Enum.Parse(typeof(Keys), UpBox.Text),
                (int)Enum.Parse(typeof(Keys), StartBox.Text),
                (int)Enum.Parse(typeof(Keys), SelectBox.Text),
                (int)Enum.Parse(typeof(Keys), BBox.Text),
                (int)Enum.Parse(typeof(Keys), ABox.Text),
                (int)Keys.Escape };
        }
    }
}
