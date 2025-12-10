using CourseProject.Playlist;
using System;
using System.IO;
using System.Windows.Forms;

namespace CourseProject
{
    public class PlaylistController
    {
        private readonly PlaylistManager manager;
        private readonly ListBox listBox;
        private readonly Action refreshAction;

        public PlaylistController(PlaylistManager manager, ListBox listBox, Action refreshAction)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.listBox = listBox ?? throw new ArgumentNullException(nameof(listBox));
            this.refreshAction = refreshAction ?? throw new ArgumentNullException(nameof(refreshAction));
        }

        public void AddTracks()
        {
            string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string audioFilesDir = Path.Combine(projectDir, "Audiofiles");

            if (!Directory.Exists(audioFilesDir))
            {
                Directory.CreateDirectory(audioFilesDir);
            }

            using (var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Аудиофайлы|*.mp3;*.wav;*.flac;*.m4a;*.ogg|Все файлы|*.*",
                Title = "Выберите треки",
                InitialDirectory = audioFilesDir
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filePath in dlg.FileNames)
                    {
                        manager.Add(new PlaylistItem
                        {
                            FilePath = filePath,
                            DisplayName = Path.GetFileName(filePath)
                        });
                    }
                    refreshAction?.Invoke();
                }
            }
        }

        public void RemoveSelected()
        {
            if (listBox.SelectedIndex < 0) return;
            manager.RemoveAt(listBox.SelectedIndex);
            refreshAction();
        }

        public void SavePlaylist()
        {
            string playlistsDir = Path.Combine(PathHelper.GetProjectRoot(), "Playlists");
            Directory.CreateDirectory(playlistsDir);

            using (var dlg = new SaveFileDialog
            {
                Filter = "Плейлист (*.json)|*.json",
                InitialDirectory = playlistsDir,
                Title = "Сохранить плейлист"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    manager.Save(dlg.FileName);
                }
            }
        }

        public void LoadPlaylist()
        {
            string playlistsDir = Path.Combine(PathHelper.GetProjectRoot(), "Playlists");
            Directory.CreateDirectory(playlistsDir);

            using (var dlg = new OpenFileDialog
            {
                Filter = "Плейлист (*.json)|*.json",
                InitialDirectory = playlistsDir,
                Title = "Загрузить плейлист"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    manager.Load(dlg.FileName);
                    refreshAction?.Invoke();
                }
            }
        }

        public PlaylistItem GetSelected()
        {
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= manager.Playlist.Count) return null;
            return manager.Get(listBox.SelectedIndex);
        }
    }
}
