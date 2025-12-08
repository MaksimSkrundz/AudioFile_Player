using System;
using System.Windows.Forms;

namespace CourseProject
{
    /// <summary>
    /// Полностью автономный обработчик горячих клавиш и колёсика мыши
    /// </summary>
    public static class Hotkeys
    {
        public static void Setup(
            Form form,
            Action playOrResume,
            Action pause,
            Action stop,
            Action nextTrack,
            Action previousTrack,
            Func<int> getVolume,        
            Action<int> setVolume)      
        {
            form.KeyPreview = true;

            form.KeyDown += (sender, e) =>
            {
                // P — Play / Resume
                if (e.KeyCode == Keys.P && Control.ModifierKeys == Keys.None)
                {
                    playOrResume?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // Левый Shift — только пауза
                if (e.KeyCode == Keys.ShiftKey && Control.ModifierKeys == Keys.Shift)
                {
                    pause?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // S — Stop
                if (e.KeyCode == Keys.S && Control.ModifierKeys == Keys.None)
                {
                    stop?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // Ctrl + → — следующий трек
                if (e.Control && e.KeyCode == Keys.Right && !e.Alt && !e.Shift)
                {
                    nextTrack?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // Ctrl + ← — предыдущий трек
                if (e.Control && e.KeyCode == Keys.Left && !e.Alt && !e.Shift)
                {
                    previousTrack?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            };

            // Колёсико мыши — изменение громкости
            form.MouseWheel += (sender, e) =>
            {
                int step = 5;
                int current = getVolume();
                int newVolume = e.Delta > 0 ? current + step : current - step;
                newVolume = Math.Max(0, Math.Min(100, newVolume));

                setVolume(newVolume);

                string originalTitle = form.Text;
                form.Text = $"Аудиоплеер — Громкость: {newVolume}%";

                var timer = new Timer { Interval = 1000 };
                timer.Tick += (s, args) =>
                {
                    form.Text = originalTitle;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();

                ((HandledMouseEventArgs)e).Handled = true;
            };
        }
    }
}