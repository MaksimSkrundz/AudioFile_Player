using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseProject
{
    public class AudioPlayer
    {
        private WaveOutEvent output;                            
        public WaveOutEvent OutputDevice => output;            

        public AudioFileReader Reader { get; private set; }
        public VolumeSampleProvider VolumeProvider { get; private set; }
        public EqualizerSampleProvider Equalizer { get; private set; }

        private readonly List<IAudioDecoder> decoders;

        public event EventHandler PlaybackStopped;

        public AudioPlayer()
        {
            decoders = new List<IAudioDecoder>
            {
                new Mp3Decoder(),
                new WavDecoder()
            };
        }

        public void Play(string path, float volume, EqualizerBand[] bands, IAudioDecoder decoder)
        {
            if (decoder == null)
            {
                decoder = decoders.FirstOrDefault(d => d.CanOpen(path));
                if (decoder == null) throw new NotSupportedException("Не найден декодер для файла: " + path);
            }

            if (output != null && output.PlaybackState == PlaybackState.Paused && Reader != null && Reader.FileName == path)
            {
                Resume();
                return;
            }

            Stop();

            var sample = decoder.Load(path);
            Reader = decoder.Reader ?? (sample as AudioFileReader); 

            Equalizer = new EqualizerSampleProvider(sample, bands ?? new EqualizerBand[0]);

            VolumeProvider = new VolumeSampleProvider(Equalizer)
            {
                Volume = volume
            };

            output = new WaveOutEvent();
            output.Init(VolumeProvider.ToWaveProvider16());
            output.PlaybackStopped += Output_PlaybackStopped;
            output.Play();
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            try
            {
                output?.Pause();
            }
            catch { }
        }

        public void Resume()
        {
            try
            {
                output?.Play();
            }
            catch { }
        }

        public void TogglePlayPause()
        {
            if (output == null) return;

            if (output.PlaybackState == PlaybackState.Playing) Pause();
            else Resume();
        }

        public void Stop()
        {
            try
            {
                if (output != null)
                {
                    output.Stop();
                    output.PlaybackStopped -= Output_PlaybackStopped;
                    output.Dispose();
                    output = null;
                }
            }
            catch { }

            if (Reader != null)
            {
                try { Reader.CurrentTime = TimeSpan.Zero; }
                catch { }
            }
        }

        public void Seek(double percent)
        {
            if (Reader == null) return;
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;
            var sec = Reader.TotalTime.TotalSeconds * percent;
            Reader.CurrentTime = TimeSpan.FromSeconds(sec);
        }

        public void SetVolume(float volume)
        {
            if (VolumeProvider != null) VolumeProvider.Volume = volume;
        }
    }
}
