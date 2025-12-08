using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;

namespace CourseProject
{
    public class EqualizerSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;
        private readonly List<EqualizerBand> bands;
        private BiQuadFilter[][] filters;

        public WaveFormat WaveFormat => source.WaveFormat;

        public EqualizerSampleProvider(ISampleProvider source, EqualizerBand[] bands)
        {
            this.source = source;
            this.bands = new List<EqualizerBand>(bands);
            channels = source.WaveFormat.Channels;

            CreateFilters();
        }

        private void CreateFilters()
        {
            filters = new BiQuadFilter[bands.Count][];

            for (int i = 0; i < bands.Count; i++)
            {
                filters[i] = new BiQuadFilter[channels];

                for (int ch = 0; ch < channels; ch++)
                {
                    filters[i][ch] = BiQuadFilter.PeakingEQ(
                        WaveFormat.SampleRate,
                        bands[i].Frequency,
                        bands[i].Bandwidth,
                        bands[i].Gain
                    );
                }
            }
        }

        public void UpdateBand(int index, float gain)
        {
            if (index < 0 || index >= bands.Count) return;

            bands[index].Gain = gain;

            for (int ch = 0; ch < channels; ch++)
            {
                filters[index][ch] = BiQuadFilter.PeakingEQ(
                    WaveFormat.SampleRate,
                    bands[index].Frequency,
                    bands[index].Bandwidth,
                    bands[index].Gain
                );
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);

            for (int i = 0; i < read; i += channels)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int idx = offset + i + ch;
                    float sample = buffer[idx];

                    for (int b = 0; b < filters.Length; b++)
                        sample = filters[b][ch].Transform(sample);

                    buffer[idx] = sample;
                }
            }

            return read;
        }
    }
}
