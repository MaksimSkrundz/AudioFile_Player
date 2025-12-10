using System;
using System.Windows.Forms;

namespace CourseProject
{
    public class HotkeysController : IDisposable
    {
        private readonly Form form;
        private readonly PlaybackController playback;
        private readonly Action addTrack;
        private readonly Action removeTrack;

        private readonly Func<int> getVolume;
        private readonly Action<int> setVolume;

        private bool disposed;

        public HotkeysController(
            Form form,
            PlaybackController playback,
            Func<int> getVolume,
            Action<int> setVolume,
            Action addTrack,
            Action removeTrack)
        {
            this.form = form;
            this.playback = playback;
            this.getVolume = getVolume;
            this.setVolume = setVolume;
            this.addTrack = addTrack;
            this.removeTrack = removeTrack;

            form.KeyPreview = true;

            form.KeyDown += Form_KeyDown;
            form.MouseWheel += Form_MouseWheel;
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            // Play / Resume
            if (e.KeyCode == Keys.P && NoModifiers(e))
            {
                playback.PlaySelected();
                Suppress(e);
                return;
            }

            // Pause — Shift
            if (e.KeyCode == Keys.ShiftKey)
            {
                playback.Pause();
                Suppress(e);
                return;
            }

            // Stop
            if (e.KeyCode == Keys.S && NoModifiers(e))
            {
                playback.Stop();
                Suppress(e);
                return;
            }

            // Предыдущий трек — Q
            if (e.KeyCode == Keys.Q && NoModifiers(e))
            {
                playback.PreviousTrack(true);
                Suppress(e);
                return;
            }

            // Следующий трек — W
            if (e.KeyCode == Keys.W && NoModifiers(e))
            {
                playback.NextTrack(true);
                Suppress(e);
                return;
            }

            // +
            if (e.KeyCode == Keys.Oemplus && NoModifiers(e))
            {
                addTrack?.Invoke();
                Suppress(e);
                return;
            }

            // -
            if (e.KeyCode == Keys.OemMinus && NoModifiers(e))
            {
                removeTrack?.Invoke();
                Suppress(e);
                return;
            }
        }

        private void Form_MouseWheel(object sender, MouseEventArgs e)
        {
            int step = 5;
            int current = getVolume();
            int newVol = e.Delta > 0 ? current + step : current - step;
            newVol = Math.Max(0, Math.Min(100, newVol));
            setVolume(newVol);

            if (e is HandledMouseEventArgs h)
                h.Handled = true;
        }

        private bool NoModifiers(KeyEventArgs e) =>
            (e.Modifiers & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None;

        private void Suppress(KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            form.KeyDown -= Form_KeyDown;
            form.MouseWheel -= Form_MouseWheel;
        }
    }
}