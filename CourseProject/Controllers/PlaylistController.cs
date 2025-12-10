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
            using (var dlg = new OpenFileDialog { Multiselect = true, Filter = "Audio|*.mp3;*.wav;*.flac;*.m4a" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                foreach (var f in dlg.FileNames)
                    manager.Add(new PlaylistItem { FilePath = f, DisplayName = Path.GetFileName(f) });
                refreshAction();
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
            string playlistsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
            if (!Directory.Exists(playlistsDir))
            {
                Directory.CreateDirectory(playlistsDir); 
            }

            using (var dlg = new SaveFileDialog { Filter = "Playlist (*.json)|*.json", InitialDirectory = playlistsDir })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                manager.Save(dlg.FileName);
            }
        }

        public void LoadPlaylist()
        {
            string playlistsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
            if (!Directory.Exists(playlistsDir))
            {
                Directory.CreateDirectory(playlistsDir); 
            }

            using (var dlg = new OpenFileDialog { Filter = "Playlist (*.json)|*.json", InitialDirectory = playlistsDir })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                manager.Load(dlg.FileName);
                refreshAction();
            }
        }

        public PlaylistItem GetSelected()
        {
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= manager.Playlist.Count) return null;
            return manager.Get(listBox.SelectedIndex);
        }
    }
}
