using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NESemuGUI
{
    public partial class FormINESHeader : Form
    {
        public Byte[] BinRomHeader = new byte[16];
        public FormINESHeader()
        {
            InitializeComponent();
        }

        private void FormINESHeader_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true; // this cancels the close event.
            UpdateView();
        }

        private void UpdateView()
        {
            string binHeader = "";
            for (int i = 0; i < BinRomHeader.Length; i++)
            {
                binHeader += BinRomHeader[i].ToString("X2") + " ";
            }

            textBoxBinINESHeader.Text = binHeader;

            // PRG ROM size
            int prgRomSize = BinRomHeader[4] * 16;
            textBoxPRGROM.Text = prgRomSize.ToString();

            // CHR ROM size
            int chrRomSize = BinRomHeader[4] * 8;
            textBoxCHRROM.Text = chrRomSize.ToString();

            //mirroring
            //four screens mapping
            string mirroring = "";
            bool fourScreens = ((BinRomHeader[6] & 0x08) == 0x08) ? true : false;
            if (fourScreens)
            {
                mirroring = "Four Screens";
            }
            else
            {
                mirroring = ((BinRomHeader[6] & 0x01) == 0x01) ? "Vertical" : "Horizontal";
            }
            textBoxMirroringType.Text = mirroring;

            //has battery-backed PRG RAM
            bool battery = ((BinRomHeader[6] & 0x02) == 0x02) ? true : false;
            checkBoxBattery.Checked = battery;


            //rom contains trainer
            bool trainer = ((BinRomHeader[6] & 0x04) == 0x04) ? true : false;
            checkBoxTrainer.Checked = trainer;

            //mapper
            int mapper = ((BinRomHeader[7] >> 4) << 4) | (BinRomHeader[6] >> 4);
            textBoxMapper.Text = mapper.ToString();

            //system
            string system = "";
            switch (BinRomHeader[7] & 0x03)
            {
                case 0x00:
                    system = "NES / Famicom / Dendy"; break;
                case 0x01:
                    system = "VS System"; break;
                case 0x02:
                    system = "Playchoice-10"; break;
            }
            textBoxSystem.Text = system;

            // ---- NES 2.0 check -----
            if ((BinRomHeader[7] & 0x0C) == 0x08)
            {
                // --- NES 2.0 Format
                //submapper
                //int submapper = BinRomHeader[8] >> 4;
            }
            else
            {
                // --- iNES Format

                // PRG RAM size
                int prgRamSize = BinRomHeader[8] * 8;
                textBoxPRGRAM.Text = prgRamSize.ToString();

                //TV System - Frame Timing
                string frameTiming = ((BinRomHeader[8] & 0x01) == 0x00) ? "NTSC" : "PAL";
                textBoxFrameTiming.Text = frameTiming;
            }
        }

        private void FormINESHeader_Shown(object sender, EventArgs e)
        {
            UpdateView();
        }
    }
}
