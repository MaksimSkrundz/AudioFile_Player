using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CourseProject
{
    public class Mp3Decoder : IAudioDecoder
    {
        public AudioFileReader Reader { get; private set; }

        public bool CanDecode(string path)
        {
            return path.ToLower().EndsWith(".mp3");
        }

        public ISampleProvider Load(string path)
        {
            Reader = new AudioFileReader(path);
            return Reader.ToSampleProvider();
        }
    }
}
