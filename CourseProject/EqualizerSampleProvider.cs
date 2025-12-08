using CourseProject;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace CourseProject
{
    public class EqualizerSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;
        private readonly List<EqualizerBand> bands;
        private BiQuadFilter[][] filters;
        private WaveFormat waveFormat;

        public EqualizerSampleProvider(ISampleProvider src, EqualizerBand[] bands)
        {
            this.source = src;
            this.bands = new List<EqualizerBand>(bands);
            this.waveFormat = src.WaveFormat;
            this.channels = waveFormat.Channels;
            CreateFilters();
        }

        void CreateFilters()
        {
            filters = new BiQuadFilter[bands.Count][];
            for (int i = 0; i < bands.Count; i++)
            {
                filters[i] = new BiQuadFilter[channels];
                for (int ch = 0; ch < channels; ch++)
                {
                    filters[i][ch] = BiQuadFilter.PeakingEQ(waveFormat.SampleRate, bands[i].Frequency, bands[i].Bandwidth, bands[i].Gain);
                }
            }
        }

        public void UpdateBand(int index, float gain)
        {
            if (index < 0 || index >= bands.Count) return;
            bands[index].Gain = gain;
            for (int ch = 0; ch < channels; ch++)
                filters[index][ch] = BiQuadFilter.PeakingEQ(waveFormat.SampleRate, bands[index].Frequency, bands[index].Bandwidth, bands[index].Gain);
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);
            for (int n = 0; n < samplesRead; n += channels)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int idx = offset + n + ch;
                    float sample = buffer[idx];
                    for (int b = 0; b < filters.Length; b++)
                        sample = filters[b][ch].Transform(sample);
                    buffer[idx] = sample;
                }
            }
            return samplesRead;
        }
    }
}
