using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Windows.Forms;

using dnlib.DotNet.Resources;

namespace osu_BackgroundChanger
{
	internal partial class Form1 : Form
	{
		public OsuSeasonal Seasonal;
		public Dictionary<string, Image> Images;

		public Form1()
		{
			InitializeComponent();
		}

		private async void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var ofd = new OpenFileDialog();
			ofd.Filter = "osu!seasonal.dll|osu!seasonal.dll";

			if (ofd.ShowDialog() != DialogResult.OK) return; 

#if !DEBUG
            try
#endif
			{
				//prepare
				Images = new Dictionary<string, Image>();

				//read file and save it in a variable
				Seasonal = new OsuSeasonal(ofd.FileName);

				//enable controls again
				splitContainer1.Enabled = true;

				//loop through resources
				foreach (var element in Seasonal.ResourceSet.ResourceElements) {
					if (!(element.ResourceData is BinaryResourceData)) continue;

					var rs = (BinaryResourceData)element.ResourceData;
					var bm = await Helpers.DeserializeBitmapAsync(rs.Data); //takes about 50ms

					Images.Add(element.Name, bm);
					listView1.Items.Add(element.Name, element.Name);
				}
			}
#if !DEBUG
            catch (Exception ex) {
                MessageBox.Show("An exception occured while trying to load the assembly:\n\n" + ex);
            }
#endif
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			//set imageview to selected image
			pictureBox1.Image = listView1.SelectedItems.Count != 0
				? Images[listView1.SelectedItems[0].ImageKey]
				: null;
		}

		private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var ofd = new OpenFileDialog();
			ofd.Filter = "Jpeg files|*.jpg;*.jpeg";

			if (ofd.ShowDialog() != DialogResult.OK) return;

			var img = Image.FromFile(ofd.FileName);

			foreach (ListViewItem i in listView1.SelectedItems) {
				Images[i.ImageKey] = img;
			}

			//force update
			listView1_SelectedIndexChanged(this, null);
		}
	}
}
