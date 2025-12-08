using System;
using TagLib;

namespace CourseProject
{
    public static class MetadataReader
    {
        public static TrackMetadata Read(string path)
        {
            try
            {
                var file = TagLib.File.Create(path);

                return new TrackMetadata
                {
                    Title = file.Tag.Title ?? System.IO.Path.GetFileName(path),
                    Artist = file.Tag.FirstPerformer ?? "Unknown",
                    Album = file.Tag.Album ?? "",
                    Duration = file.Properties.Duration
                };
            }
            catch
            {
                return new TrackMetadata
                {
                    Title = System.IO.Path.GetFileName(path),
                    Artist = "Unknown"
                };
            }
        }
    }

    public class TrackMetadata
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
