using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace CourseProject
{
    public class WavDecoder : IAudioDecoder
    {
        public AudioFileReader Reader { get; private set; }

        public bool CanOpen(string path) =>
            Path.GetExtension(path).ToLower() == ".wav";  

        public ISampleProvider Load(string path)
        {
            Reader = new AudioFileReader(path);
            return Reader;
        }
    }
}
