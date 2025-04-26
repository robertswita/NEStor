using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NES
{
    public partial class FormPPUViewer : Form
    {
        public FormPPUViewer()
        {
            InitializeComponent();
        }

        private void FormPPUViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private void checkBoxShowPpuScrollOverlay_CheckedChanged(object sender, EventArgs e)
        {
            //glViewNametable.Context.ShowPPUScrollOverlay = !glViewNametable.Context.ShowPPUScrollOverlay;
        }

        private void checkBoxShowTileGrid_CheckedChanged(object sender, EventArgs e)
        {
            //glViewNametable.Context.ShowTileGrid = !glViewNametable.Context.ShowTileGrid;
        }

        private void checkBoxShowAttributeGrid_CheckedChanged(object sender, EventArgs e)
        {
            //glViewNametable.Context.ShowAttributeGrid = !glViewNametable.Context.ShowAttributeGrid;
        }

        private void PictureBoxPaint(object sender, PaintEventArgs e)
        {
            var pictureBox = sender as PictureBox;
            if (pictureBox.Image != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                e.Graphics.DrawImage(pictureBox.Image, pictureBox.ClientRectangle);
            }
        }
    }
}
