using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CourseProject
{
    public interface IAudioDecoder
    {
        bool CanOpen(string path);       
        AudioFileReader Reader { get; }  
        ISampleProvider Load(string path);
    }
}
