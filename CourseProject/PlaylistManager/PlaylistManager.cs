using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace CourseProject.Playlist
{
    public class PlaylistManager
    {
        public List<PlaylistItem> Playlist { get; private set; } = new List<PlaylistItem>();

        public string CurrentPlaylistPath { get; private set; }

        public void Add(PlaylistItem item) => Playlist.Add(item);
        public void RemoveAt(int idx) { if (idx >= 0 && idx < Playlist.Count) Playlist.RemoveAt(idx); }
        public PlaylistItem Get(int idx) => (idx >= 0 && idx < Playlist.Count) ? Playlist[idx] : null;

        public void SetPlaylist(List<PlaylistItem> list)
        {
            Playlist = list ?? new List<PlaylistItem>();
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(Playlist, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            CurrentPlaylistPath = path;
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            Playlist = JsonSerializer.Deserialize<List<PlaylistItem>>(json) ?? new List<PlaylistItem>();

            CurrentPlaylistPath = path; 
        }

        public bool SaveCurrent()
        {
            if (string.IsNullOrEmpty(CurrentPlaylistPath)) return false;
            try
            {
                Save(CurrentPlaylistPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}