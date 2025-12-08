using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace CourseProject
{
    public class Mp3Decoder : IAudioDecoder
    {
        public AudioFileReader Reader { get; private set; }

        public bool CanOpen(string path) =>
            Path.GetExtension(path).ToLower() == ".mp3";  

        public ISampleProvider Load(string path)
        {
            Reader = new AudioFileReader(path);
            return Reader;
        }
    }
}
