using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NESemuGUI
{
    public partial class FormPPUViewer : Form
    {
        public FormPPUViewer()
        {
            InitializeComponent();
        }

        private void FormPPUViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true; // this cancels the close event.
        }

        public void UpdateMirroringType(bool type)
        {
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    if (type)
                    {
                        labelMirroringType.Text = "Vertical";
                    }
                    else
                    {
                        labelMirroringType.Text = "Horizontal";
                    }
                }));
            }
            
        }

        private void checkBoxShowPpuScrollOverlay_CheckedChanged(object sender, EventArgs e)
        {
            glViewNametable.Context.ShowPPUScrollOverlay = !glViewNametable.Context.ShowPPUScrollOverlay;
        }

        private void checkBoxShowTileGrid_CheckedChanged(object sender, EventArgs e)
        {
            glViewNametable.Context.ShowTileGrid = !glViewNametable.Context.ShowTileGrid;
        }

        private void checkBoxShowAttributeGrid_CheckedChanged(object sender, EventArgs e)
        {
            glViewNametable.Context.ShowAttributeGrid = !glViewNametable.Context.ShowAttributeGrid;
        }
    }
}
