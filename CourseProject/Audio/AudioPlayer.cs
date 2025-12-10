using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Windows.Forms;

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
            if (currentFilePath != null &&
                string.Equals(currentFilePath, filePath, StringComparison.OrdinalIgnoreCase) &&
                output != null &&
                (output.PlaybackState == PlaybackState.Playing || output.PlaybackState == PlaybackState.Paused))
            {
                return;
            }

            isManualStop = true;

            try
            {
                StopInternal();

                currentFilePath = filePath;
                isManualStop = false;

                var sampleProvider = decoder.Load(filePath);
                Reader = decoder.Reader;

                Equalizer = new EqualizerSampleProvider(sampleProvider, bands ?? new EqualizerBand[0]);
                VolumeProvider = new VolumeSampleProvider(Equalizer) { Volume = volume };

                output = new WaveOutEvent();
                output.Init(VolumeProvider.ToWaveProvider16());
                output.PlaybackStopped += OnPlaybackStopped;
                output.Play();
            }
            catch
            {
                isManualStop = false;
                throw;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!isManualStop)
            {
                PlaybackCompleted?.Invoke(this, EventArgs.Empty);
            }

            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            try
            {
                isManualStop = false;
                output?.Pause();
            }
            catch { }
        }

        public void Resume()
        {
            try
            {
                isManualStop = false;
                output?.Play();
            }
            catch { }
        }

        public void TogglePlayPause()
        {
            if (output == null)
            {
                return;
            }

            if (output.PlaybackState == PlaybackState.Playing) Pause(); else Resume();
        }

        public void Stop()
        {
            isManualStop = true;
            StopInternal();
        }

        private void StopInternal()
        {
            output?.Stop();
            if (output != null)
            {
                output.PlaybackStopped -= OnPlaybackStopped;
                output.Dispose();
                output = null;
            }
            currentFilePath = null;
            Reader?.Dispose();
            Reader = null;
            VolumeProvider = null;
            Equalizer = null;
        }

        public void Seek(double percent)
        {
            if (Reader == null) return;
            percent = Math.Max(0, Math.Min(1, percent));
            var sec = Reader.TotalTime.TotalSeconds * percent;
            Reader.CurrentTime = TimeSpan.FromSeconds(sec);
        }

        public void SetVolume(float v)
        {
            if (VolumeProvider != null)
                VolumeProvider.Volume = v;
        }
    }
}