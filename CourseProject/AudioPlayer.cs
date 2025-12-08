using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace CourseProject
{
    public class AudioPlayer
    {
        private WaveOutEvent output;
        private string currentFilePath;
        private bool isManualStop = false;

        public WaveOutEvent OutputDevice => output;
        public AudioFileReader Reader { get; private set; }
        public VolumeSampleProvider VolumeProvider { get; private set; }
        public EqualizerSampleProvider Equalizer { get; private set; }

        public event EventHandler PlaybackStopped;
        public event EventHandler PlaybackCompleted;

        public AudioPlayer() { }

        public void Play(string filePath, float volume, EqualizerBand[] bands, IAudioDecoder decoder)
        {
            if (currentFilePath == filePath && output?.PlaybackState == PlaybackState.Playing)
            {
                Seek(0);
                return;
            }

            Stop();
            currentFilePath = filePath;
            isManualStop = false;

            var sampleProvider = decoder.Load(filePath);
            Reader = decoder.Reader;

            Equalizer = new EqualizerSampleProvider(sampleProvider, bands);

            VolumeProvider = new VolumeSampleProvider(Equalizer)
            {
                Volume = volume
            };

            output = new WaveOutEvent();
            output.Init(VolumeProvider);
            output.PlaybackStopped += OnPlaybackStopped;
            output.Play();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
            }
            else if (!isManualStop)
            {
                PlaybackCompleted?.Invoke(this, EventArgs.Empty);
            }

            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            isManualStop = false;
            output?.Pause();
        }

        public void Resume()
        {
            isManualStop = false;
            output?.Play();
        }

        public void TogglePlayPause()
        {
            if (output == null) return;

            if (output.PlaybackState == PlaybackState.Playing)
                Pause();
            else
                Resume();
        }

        public void Stop()
        {
            isManualStop = true;

            try { output?.Stop(); } catch { }

            output?.Dispose();
            output = null;
            currentFilePath = null;

            Reader?.Dispose();
            Reader = null;

            VolumeProvider = null;
            Equalizer = null;
        }

        public void Seek(double percent)
        {
            if (Reader == null) return;

            double seconds = Reader.TotalTime.TotalSeconds * percent;
            Reader.CurrentTime = TimeSpan.FromSeconds(seconds);
        }

        public void SetVolume(float volume)
        {
            if (VolumeProvider != null)
                VolumeProvider.Volume = volume;
        }
    }
}