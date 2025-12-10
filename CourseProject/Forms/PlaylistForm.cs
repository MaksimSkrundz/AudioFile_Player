using CourseProject.Playlist;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CourseProject
{
    public class PlaylistForm : Form
    {
        private readonly PlaylistManager manager;
        private ListBox lst;
        private Button btnRemove, btnUp, btnDown, btnApply, btnClose;

        public event Action<System.Collections.Generic.List<PlaylistItem>> PlaylistUpdated;

        public PlaylistForm(PlaylistManager manager)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            InitializeComponent();
            RefreshList();
        }

        private void InitializeComponent()
        {
            this.Text = "Редактор плейлиста";
            this.Width = 600;
            this.Height = 420;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lst = new ListBox
            {
                Left = 10,
                Top = 10,
                Width = 560,
                Height = 300
            };
            lst.DoubleClick += (s, e) =>
            {
                if (lst.SelectedIndex >= 0)
                    MessageBox.Show(Path.GetFileName(manager.Playlist[lst.SelectedIndex].FilePath), "Путь к файлу");
            };
            this.Controls.Add(lst);

            btnRemove = new Button { Left = 10, Top = 320, Width = 80, Text = "Удалить" };
            btnRemove.Click += (s, e) =>
            {
                if (lst.SelectedIndex >= 0)
                {
                    manager.Playlist.RemoveAt(lst.SelectedIndex);
                    RefreshList();
                }
            };
            this.Controls.Add(btnRemove);

            btnUp = new Button { Left = 100, Top = 320, Width = 80, Text = "Вверх" };
            btnUp.Click += (s, e) =>
            {
                int idx = lst.SelectedIndex;
                if (idx > 0)
                {
                    var item = manager.Playlist[idx];
                    manager.Playlist.RemoveAt(idx);
                    manager.Playlist.Insert(idx - 1, item);
                    RefreshList();
                    lst.SelectedIndex = idx - 1;
                }
            };
            this.Controls.Add(btnUp);

            btnDown = new Button { Left = 190, Top = 320, Width = 80, Text = "Вниз" };
            btnDown.Click += (s, e) =>
            {
                int idx = lst.SelectedIndex;
                if (idx >= 0 && idx < manager.Playlist.Count - 1)
                {
                    var item = manager.Playlist[idx];
                    manager.Playlist.RemoveAt(idx);
                    manager.Playlist.Insert(idx + 1, item);
                    RefreshList();
                    lst.SelectedIndex = idx + 1;
                }
            };
            this.Controls.Add(btnDown);

            btnApply = new Button { Left = 280, Top = 320, Width = 120, Text = "Применить" };
            btnApply.Click += (s, e) =>
            {
                var updatedList = manager.Playlist
                    .Select(p => new PlaylistItem { FilePath = p.FilePath, DisplayName = p.DisplayName })
                    .ToList();

                PlaylistUpdated?.Invoke(updatedList);

                if (!string.IsNullOrEmpty(manager.CurrentPlaylistPath))
                {
                    try
                    {
                        manager.Save(manager.CurrentPlaylistPath);
                        MessageBox.Show($"Плейлист применён и сохранён:\n{Path.GetFileName(manager.CurrentPlaylistPath)}",
                                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Плейлист применён, но не удалось сохранить:\n{ex.Message}",
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Плейлист применён (новый плейлист — не сохранён автоматически).",
                                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Close();
            };
            this.Controls.Add(btnApply);

            btnClose = new Button { Left = 410, Top = 320, Width = 80, Text = "Закрыть" };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void RefreshList()
        {
            lst.Items.Clear();
            foreach (var item in manager.Playlist)
                lst.Items.Add(item.DisplayName);

            if (lst.Items.Count > 0 && lst.SelectedIndex == -1)
                lst.SelectedIndex = 0;
        }
    }
}