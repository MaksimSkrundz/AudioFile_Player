using NAudio.Wave;

namespace CourseProject
{
    public class WavDecoder : IAudioDecoder
    {
        public AudioFileReader Reader { get; private set; }

        public bool CanDecode(string path)
        {
            return path.ToLower().EndsWith(".wav");
        }

        public ISampleProvider Load(string path)
        {
            Reader = new AudioFileReader(path);
            return Reader.ToSampleProvider();
        }
    }
}
