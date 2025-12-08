using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CourseProject
{
    public class PlaylistForm : Form
    {
        private PlaylistManager manager;
        private ListBox lst;
        private Button btnRemove, btnSave, btnClose;

        public event Action<List<PlaylistItem>> PlaylistUpdated;

        public PlaylistForm(PlaylistManager manager)
        {
            this.manager = manager;
            InitializeComponent();
            RefreshList();
        }

        private void InitializeComponent()
        {
            this.Text = "Edit Playlist";
            this.Width = 600;
            this.Height = 400;

            lst = new ListBox { Left = 10, Top = 10, Width = 560, Height = 300 };
            this.Controls.Add(lst);

            btnRemove = new Button { Left = 10, Top = 320, Width = 80, Text = "Remove" };
            btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(btnRemove);

            btnSave = new Button { Left = 100, Top = 320, Width = 80, Text = "Save" };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnClose = new Button { Left = 190, Top = 320, Width = 80, Text = "Close" };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void RefreshList()
        {
            lst.Items.Clear();
            foreach (var it in manager.Playlist) lst.Items.Add(it.DisplayName);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lst.SelectedIndex >= 0)
            {
                manager.Playlist.RemoveAt(lst.SelectedIndex);
                RefreshList();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            PlaylistUpdated?.Invoke(manager.Playlist.ToList());
            MessageBox.Show("Playlist sent to main form.");
        }
    }
}
