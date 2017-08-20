using System;
using System.Windows.Forms;

namespace osu_BackgroundChanger
{
    internal partial class Form1 : Form
    {
        public OsuSeasonal Seasonal;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "osu!seasonal.dll|osu!seasonal.dll";

            if (ofd.ShowDialog() == DialogResult.OK) {
                try {
                    //read file and save it in a variable
                    Seasonal = new OsuSeasonal(ofd.FileName);

                    //for now, show the amount of images found
                    MessageBox.Show("Amount of detected resource elements: " + Seasonal.ResourceSet.Count);
                }
                catch (Exception ex) {
                    MessageBox.Show("An exception occured while trying to load the assembly:\n\n" + ex);
                }
            }
        }
    }
}
