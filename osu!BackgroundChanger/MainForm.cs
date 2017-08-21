using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            DebugEnable();
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

        private async Task SaveFileAsync()
        {
            //get save location before we do any heavy work
            var sfd = new SaveFileDialog();
            sfd.Filter = "DLL File|*.dll|All Files|*";
            sfd.FileName = "osu!seasonal.dll";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            //serialize all bitmaps
            foreach (var pair in Images) {
                byte[] bytes = await Helpers.SerializeBitmapAsync(new Bitmap(pair.Value));

                //and add them to the resourceset too, while we're at it
                //note that this will overwrite if the name matches an old element
                Seasonal.ResourceSet.Add(new ResourceElement {
                    Name = pair.Key,
                    ResourceData = new BinaryResourceData(new UserResourceType(typeof(Bitmap).AssemblyQualifiedName, ResourceTypeCode.UserTypes),
                        bytes)
                });
            }

            //save module
            Seasonal.Save(sfd.FileName);
        }

        private void SelectAll()
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = true;
        }

        private void ExportImages()
        {
            Debug.Assert(listView1.SelectedItems.Count > 0);

            bool multi = listView1.SelectedItems.Count > 1;

            if (!multi) {
                //only single item selected
                var selectedImage = Images[listView1.SelectedItems[0].ImageKey];

                var sfd = new SaveFileDialog();
                sfd.Filter = "JPEG files|*.jpg;*.jpeg|All Files|*";
                sfd.Title = "Export";
                sfd.FileName = listView1.SelectedItems[0].ImageKey + ".jpg";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var path = sfd.FileName;
                selectedImage.Save(path);
            }
            else {
                //multiple images selected
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() != DialogResult.OK) return;

                var imageKeys = listView1.SelectedItems.Cast<ListViewItem>().Select(i => i.ImageKey);
                Parallel.ForEach(imageKeys, key => Images[key].Save(fbd.SelectedPath + "/" + key + ".jpg"));
            }
        }

        private void ReplaceImage()
        {
            if (listView1.SelectedItems.Count == 0) return;

            var ofd = new OpenFileDialog();
            ofd.Filter = "Jpeg files|*.jpg;*.jpeg";
            ofd.Title = "Replace";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var img = Image.FromFile(ofd.FileName);

            foreach (ListViewItem i in listView1.SelectedItems) {
                Images[i.ImageKey] = img;
            }

            //force update
            listView1_SelectedIndexChanged(this, null);
        }

        private void ClearImage()
        {
            var imageKeys = listView1.SelectedItems.Cast<ListViewItem>().Select(i => i.ImageKey);
            foreach (string key in imageKeys)
                Images[key] = new Bitmap(1, 1);

            UpdateImagePreview();
        }

        private void ClearAllImages()
        {
            foreach (var key in Images.Keys.ToArray())
                Images[key] = new Bitmap(1, 1);

            UpdateImagePreview();
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
            selectAllToolStripMenuItem.Enabled = enable;
            clearAllToolStripMenuItem.Enabled = enable;

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
            exportToolStripMenuItem.Enabled = enable;
            clearToolStripMenuItem.Enabled = enable;
        }

        private void ShowAbout() => new AboutBox().ShowDialog();

        private void ShowHelp() => MessageBox.Show("This program allows you to use your own images as \"seasonal\" images. \"Seasonal\" images "
                                                   + "are those you suddenly see in the main menu after an osu! update.\n\n"
                                                   + "Go ahead and load an osu!seasonal.dll file and see what it contains!",
            "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        #endregion

        #region Event Handlers
        private async void openToolStripMenuItem_Click(object sender, EventArgs e) => await OpenNewDllAsync();
        private async void saveToolStripMenuItem_Click(object sender, EventArgs e) => await SaveFileAsync();
        private void exportToolStripMenuItem_Click(object sender, EventArgs e) => ExportImages();

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) => SelectAll();
        private void replaceToolStripMenuItem_Click(object sender, EventArgs e) => ReplaceImage();
        private void clearToolStripMenuItem_Click(object sender, EventArgs e) => ClearImage();
        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e) => ClearAllImages();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) => ShowAbout();
        private void whatIsThisToolStripMenuItem_Click(object sender, EventArgs e) => ShowHelp();

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e) => ReplaceImage();

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateImagePreview();
            UpdateEdit();
        }
        #endregion

        #region debug
        [Conditional("DEBUG")]
        private void DebugEnable()
        {
            debugMenu.Visible = true;
        }

        private void testSerializedeserializeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "osu!seasonal.dll|osu!seasonal.dll";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var s = new OsuSeasonal(ofd.FileName);

            foreach (var element in s.ResourceSet.ResourceElements) {
                if (!(element.ResourceData is BinaryResourceData rd)) continue;

                var deserialized = Helpers.DeserializeBitmap(rd.Data);
                var serialised = Helpers.SerializeBitmap(deserialized);

                MessageBox.Show("Original: " + rd.Data.Length + "\n" +
                                "Serialized: " + serialised.Length + "\n" +
                                "Difference in size: " + (rd.Data.Length - serialised.Length));
            }
        }
        #endregion
    }
}
