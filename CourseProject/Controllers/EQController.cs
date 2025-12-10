using System.Windows.Forms;

namespace CourseProject
{
    public class EQController
    {
        private readonly AudioPlayer player;
        private readonly TrackBar low, mid, high;

        public EQController(AudioPlayer player, TrackBar low, TrackBar mid, TrackBar high)
        {
            this.player = player;
            this.low = low;
            this.mid = mid;
            this.high = high;
        }

        public void UpdateEQ()
        {
            if (player.Equalizer == null) return;
            player.Equalizer.UpdateBand(0, low.Value);
            player.Equalizer.UpdateBand(1, mid.Value);
            player.Equalizer.UpdateBand(2, high.Value);
        }

        public EqualizerBand[] CreateBands()
        {
            return new[]
            {
                new EqualizerBand { Frequency = 100, Bandwidth = 0.8f, Gain = low?.Value ?? 0 },
                new EqualizerBand { Frequency = 1000, Bandwidth = 1.0f, Gain = mid?.Value ?? 0 },
                new EqualizerBand { Frequency = 8000, Bandwidth = 0.8f, Gain = high?.Value ?? 0 }
            };
        }
    }
}
