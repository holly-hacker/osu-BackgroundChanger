using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using dnlib.DotNet.Resources;

namespace osu_BackgroundChanger
{
	internal partial class MainForm : Form
	{
		public OsuSeasonal Seasonal;
		public Dictionary<string, Image> Images;

		public MainForm()
		{
			InitializeComponent();
		}

		#region Actions
		private async Task OpenNewDllAsync()
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

				//enable center controls again
				UpdateFile();

				//loop through resources
				foreach (var element in Seasonal.ResourceSet.ResourceElements) {
					if (!(element.ResourceData is BinaryResourceData rd)) continue;

					var bm = await Helpers.DeserializeBitmapAsync(rd.Data); //takes about 50ms

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

		private void ReplaceImage()
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

		private void UpdateImagePreview()
		{
			//set imageview to selected image
			pictureBox1.Image = listView1.SelectedItems.Count != 0
				? Images[listView1.SelectedItems[0].ImageKey]
				: null;
		}

		private void UpdateFile()
		{
			bool enable = Seasonal != null;

			splitContainer1.Enabled = enable;
			saveToolStripMenuItem.Enabled = enable;

			if (!enable) {
				listView1.Clear();
				pictureBox1.Image = null;
			}
		}

		private void UpdateEdit()
		{
			bool enable = listView1.SelectedItems.Count != 0;
			bool multi = listView1.SelectedItems.Count > 1;

			replaceToolStripMenuItem.Enabled = enable;
		}
		#endregion

		#region Event Handlers
		private async void openToolStripMenuItem_Click(object sender, EventArgs e) => await OpenNewDllAsync();
		private void replaceToolStripMenuItem_Click(object sender, EventArgs e) => ReplaceImage();

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateImagePreview();
			UpdateEdit();
		}
		#endregion
	}
}
