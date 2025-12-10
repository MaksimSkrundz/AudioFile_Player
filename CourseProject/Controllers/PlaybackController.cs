using CourseProject.Playlist;
using NAudio.Wave;
using System;
using System.Windows.Forms;

namespace CourseProject
{
    public class PlaybackController
    {
        private readonly AudioPlayer player;
        private readonly PlaylistManager playlist;
        private readonly ListBox listBox;
        private readonly MetadataController metadataController; 
        public event Action OnStopReset;

        public PlaybackController(AudioPlayer player, PlaylistManager playlist, ListBox listBox, MetadataController metadataController)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
            this.listBox = listBox ?? throw new ArgumentNullException(nameof(listBox));
            this.metadataController = metadataController ?? throw new ArgumentNullException(nameof(metadataController));
        }

        public void PlaySelected()
        {
            if (listBox.SelectedIndex < 0 || listBox.SelectedIndex >= playlist.Playlist.Count)
            {
                MessageBox.Show("Выберите трек.");
                return;
            }

            var item = playlist.Playlist[listBox.SelectedIndex];

            bool isSameTrack = player.OutputDevice != null &&
                               player.Reader != null &&
                               string.Equals(player.Reader.FileName, item.FilePath, StringComparison.OrdinalIgnoreCase);

            if (isSameTrack && player.OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                player.Resume();
                return;
            }

            if (isSameTrack && player.OutputDevice.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            StartPlayback(item.FilePath);
        }

        private void StartPlayback(string filePath)
        {
            var decoder = DecoderFactory.GetDecoderForFile(filePath);
            try
            {
                player.Play(
                    filePath,
                    volume: player.VolumeProvider?.Volume ?? 1.0f,
                    bands: new[]
                    {
                new EqualizerBand { Frequency = 100, Bandwidth = 0.8f, Gain = 0 },
                new EqualizerBand { Frequency = 1000, Bandwidth = 1.0f, Gain = 0 },
                new EqualizerBand { Frequency = 8000, Bandwidth = 0.8f, Gain = 0 }
                    },
                    decoder: decoder
                );

                var item = playlist.Playlist[listBox.SelectedIndex];
                metadataController.Update(item.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка воспроизведения: " + ex.Message);
            }
        }
        public void Pause() => player.Pause();
        public void Resume() => player.Resume();

        public void TogglePlayPause()
        {
            if (player.OutputDevice == null)
            {
                PlaySelected();
                return;
            }
            if (player.OutputDevice.PlaybackState == PlaybackState.Playing) Pause();
            else if (player.OutputDevice.PlaybackState == PlaybackState.Paused) Resume();
            else PlaySelected();
        }

        public void Stop()
        {
            OnStopReset?.Invoke();
            player.Stop();
            metadataController.Clear(); 
        }

        public void NextTrack(bool userInitiated = true)
        {
            if (listBox.Items.Count == 0) return;

            int next = (listBox.SelectedIndex + 1) % listBox.Items.Count;
            listBox.SelectedIndex = next;
            PlaySelected(); 
        }

        public void PreviousTrack(bool userInitiated = true)
        {
            if (listBox.Items.Count == 0) return;

            int prev = listBox.SelectedIndex - 1;
            if (prev < 0) prev = listBox.Items.Count - 1;
            listBox.SelectedIndex = prev;
            PlaySelected();
        }
    }
}