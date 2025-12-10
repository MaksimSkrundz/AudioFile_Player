using NAudio.Wave;

namespace CourseProject
{
    public interface IAudioDecoder
    {
        bool CanDecode(string path);
        ISampleProvider Load(string path);
        AudioFileReader Reader { get; }
    }
}
