namespace OrtzIRC
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class OptionsDialog : Form
    {
        private OptionsPage activePage;

        public OptionsDialog()
        {
            InitializeComponent();

            Pages = new List<OptionsPage>();
        }

        public List<OptionsPage> Pages { get; private set; }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            Bitmap defaultImage = Resources.Icons.Gear;
            imageList.Images.Add(defaultImage);

            Size maxPageSize = pagesPanel.Size;

            foreach (OptionsPage page in Pages)
            {
                pagesPanel.Controls.Add(page);

                AddListItemForPage(page);

                if (page.Width > maxPageSize.Width)
                    maxPageSize.Width = page.Width;
                if (page.Height > maxPageSize.Height)
                    maxPageSize.Height = page.Height;

                page.Dock = DockStyle.Fill;
                page.Visible = false;
            }

            var newSize = new Size
                {
                    Width = (maxPageSize.Width + (Width - pagesPanel.Width)),
                    Height = (maxPageSize.Height + (Height - pagesPanel.Height))
                };

            Size = newSize;
            CenterToParent();

            if (listView.Items.Count != 0)
                listView.Items[0].Selected = true;
        }

        void AddListItemForPage(OptionsPage page)
        {
            int imageIndex = 0;

            Image image = page.Image;
            if (image != null)
            {
                imageList.Images.Add(image);
                imageIndex = imageList.Images.Count - 1;
            }

            var item = new ListViewItem(page.Text, imageIndex) { Tag = page };
            listView.Items.Add(item);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            foreach (OptionsPage page in Pages)
            {
                page.OnApply();
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (activePage != null)
                activePage.Visible = false;

            if (listView.SelectedItems.Count != 0)
            {
                ListViewItem selectedItem = listView.SelectedItems[0];
                var page = (OptionsPage)selectedItem.Tag;
                activePage = page;
            }

            if (activePage != null)
            {
                activePage.Visible = true;
                activePage.OnSetActive();
            }
        }
    }
}
