using System;
using System.Collections.Generic;
using NAudio.Wave;
using NAudio.Dsp;

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
        waveFormat = src.WaveFormat;
        channels = waveFormat.Channels;

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
                filters[i][ch] = BiQuadFilter.PeakingEQ(
                    waveFormat.SampleRate,
                    bands[i].Frequency,
                    bands[i].Bandwidth,
                    bands[i].Gain
                );
            }
        }
    }

    public void UpdateBand(int index, float gain)
    {
        bands[index].Gain = gain;
        CreateFilters();
    }

    public WaveFormat WaveFormat => waveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = source.Read(buffer, offset, count);

        for (int n = 0; n < read; n += channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                float s = buffer[offset + n + ch];
                for (int b = 0; b < bands.Count; b++)
                    s = filters[b][ch].Transform(s);

                buffer[offset + n + ch] = s;
            }
        }

        return read;
    }
}
