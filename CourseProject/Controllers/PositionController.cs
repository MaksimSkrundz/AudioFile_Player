using NAudio.Wave;
using System;
using System.Windows.Forms;

namespace CourseProject
{
    public class PositionController
    {
        private readonly AudioPlayer player;
        private readonly TrackBar positionBar;
        private readonly Label timeLabel;
        private readonly Timer timer;
        private bool isUserDragging = false;

        public PositionController(AudioPlayer player, TrackBar positionBar, Label timeLabel)
        {
            this.player = player;
            this.positionBar = positionBar;
            this.timeLabel = timeLabel;

            timer = new Timer { Interval = 200 };
            timer.Tick += Timer_Tick;
            timer.Start();

            if (positionBar != null)
            {
                positionBar.MouseDown += (s, e) => { isUserDragging = true; };
                positionBar.MouseUp += (s, e) =>
                {
                    isUserDragging = false;
                    SeekFromTrackbar();
                };
                positionBar.Scroll += (s, e) =>
                {
                    if (player?.Reader != null)
                    {
                        double fraction = positionBar.Value / (double)positionBar.Maximum;
                        var cur = TimeSpan.FromSeconds(fraction * player.Reader.TotalTime.TotalSeconds);
                        timeLabel.Text = $"{cur:mm\\:ss} / {player.Reader.TotalTime:mm\\:ss}";
                    }
                };
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (player?.Reader == null) return;

            if (!isUserDragging)
            {
                var cur = player.Reader.CurrentTime;
                var tot = player.Reader.TotalTime;
                timeLabel.Text = $"{cur:mm\\:ss} / {tot:mm\\:ss}";

                if (tot.TotalSeconds > 0)
                {
                    int pos = (int)(cur.TotalSeconds / tot.TotalSeconds * positionBar.Maximum);
                    if (Math.Abs(positionBar.Value - pos) > 1)
                        positionBar.Value = Math.Max(0, Math.Min(positionBar.Maximum, pos));
                }
            }
        }

        public void SeekFromTrackbar()
        {
            if (player?.Reader == null) return;

            double fraction = positionBar.Value / (double)positionBar.Maximum;
            player.Seek(fraction);
        }

        public void Reset()
        {
            if (positionBar != null)
            {
                if (positionBar.InvokeRequired) positionBar.Invoke(new Action(() => positionBar.Value = 0));
                else positionBar.Value = 0;
            }
            if (timeLabel != null)
            {
                if (timeLabel.InvokeRequired) timeLabel.Invoke(new Action(() => timeLabel.Text = "00:00 / 00:00"));
                else timeLabel.Text = "00:00 / 00:00";
            }
        }
    }
}
