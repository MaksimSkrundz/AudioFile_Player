using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

public class AudioPlayer
{
    public WaveOutEvent OutputDevice { get; private set; }
    public AudioFileReader Reader { get; private set; }
    public VolumeSampleProvider VolumeProvider { get; private set; }
    public EqualizerSampleProvider Equalizer { get; private set; }

    private bool isPaused = false;
    private bool isStopped = true;
    private bool isManualStop = false; // Флаг для ручной остановки

    public event Action PlaybackStopped;

    public void Play(string filePath, float volume, EqualizerBand[] bands)
    {
        if (isPaused && !isStopped && Reader != null)
        {
            Resume();
            return;
        }

        Stop();

        Reader = new AudioFileReader(filePath);

        Equalizer = new EqualizerSampleProvider(Reader.ToSampleProvider(), bands);

        VolumeProvider = new VolumeSampleProvider(Equalizer)
        {
            Volume = volume
        };

        OutputDevice = new WaveOutEvent();
        OutputDevice.Init(VolumeProvider.ToWaveProvider16());
        OutputDevice.PlaybackStopped += (s, e) =>
        {
            if (!isManualStop)
            {
                PlaybackStopped?.Invoke();
            }
            isStopped = true;
            isPaused = false;
            isManualStop = false; 
        };
        OutputDevice.Play();

        isStopped = false;
        isPaused = false;
        isManualStop = false;
    }

    public void Pause()
    {
        if (OutputDevice != null && !isStopped && !isPaused)
        {
            OutputDevice?.Pause();
            isPaused = true;
        }
    }

    public void Resume()
    {
        if (OutputDevice != null && isPaused && !isStopped)
        {
            OutputDevice?.Play();
            isPaused = false;
        }
    }

    public void Stop()
    {
        isManualStop = true; 

        try { OutputDevice?.Stop(); } catch { }

        OutputDevice?.Dispose();
        OutputDevice = null;

        Reader?.Dispose();
        Reader = null;

        Equalizer = null;
        VolumeProvider = null;

        isStopped = true;
        isPaused = false;
    }

    public void SetVolume(float vol)
    {
        if (VolumeProvider != null) VolumeProvider.Volume = vol;
    }

    public void Seek(double percent)
    {
        if (Reader == null || isStopped) return;

        var sec = Reader.TotalTime.TotalSeconds * percent;
        Reader.CurrentTime = TimeSpan.FromSeconds(sec);
    }
}