using CourseProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CourseProject
{
    public class PlaylistManager
    {
        public List<PlaylistItem> Playlist { get; private set; } = new List<PlaylistItem>();

        public void Add(PlaylistItem item) => Playlist.Add(item);

        public void SetPlaylist(List<PlaylistItem> list)
        {
            Playlist = list ?? new List<PlaylistItem>();
        }

        public void Save(string path)
        {
            try
            {
                var json = JsonSerializer.Serialize(Playlist, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save playlist: " + ex.Message, ex);
            }
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            try
            {
                var json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<PlaylistItem>>(json) ?? new List<PlaylistItem>();
                Playlist = list;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load playlist: " + ex.Message, ex);
            }
        }
    }
}
