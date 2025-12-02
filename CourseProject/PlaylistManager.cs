using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class PlaylistManager
{
    public static void Save(string file, List<PlaylistItem> list)
    {
        File.WriteAllText(file, JsonSerializer.Serialize(list, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public static List<PlaylistItem> Load(string file)
    {
        return JsonSerializer.Deserialize<List<PlaylistItem>>(File.ReadAllText(file));
    }
}
