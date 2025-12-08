using CourseProject;
using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class MainForm : Form
    {
        private PlaylistManager playlistManager;
        private AudioPlayer audioPlayer;
        private Timer uiTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustom();
        }

        private void RefreshPlaylistListBox()
        {
            lstPlaylist.Items.Clear();
            foreach (var item in playlistManager.Playlist)
                lstPlaylist.Items.Add(item.DisplayName);
        }

        private void InitializeCustom()
        {
            playlistManager = new PlaylistManager();
            audioPlayer = new AudioPlayer();
            // Подписка на событие должна соответствовать EventHandler
            audioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            uiTimer = new Timer { Interval = 200 };
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            RefreshPlaylistListBox();
        }

        #region UI
        private ListBox lstPlaylist;
        private Button btnAdd, btnRemove, btnEditPlaylistForm, btnPlay, btnPause, btnStop, btnSave, btnLoad;
        private TrackBar trVolume, trPosition, trEQLow, trEQMid, trEQHigh;
        private Label lblTime, lblTitle, lblArtist;

        private void InitializeComponent()
        {
            this.Text = "Audio Player";
            this.Width = 900;
            this.Height = 520;

            lstPlaylist = new ListBox { Left = 10, Top = 10, Width = 400, Height = 430 };
            lstPlaylist.DoubleClick += (s, e) => PlaySelected();
            this.Controls.Add(lstPlaylist);

            btnAdd = new Button { Left = 10, Top = 450, Width = 80, Text = "Добавить" };
            btnAdd.Click += BtnAdd_Click;
            this.Controls.Add(btnAdd);

            btnRemove = new Button { Left = 100, Top = 450, Width = 80, Text = "Удалить" };
            btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(btnRemove);

            btnSave = new Button { Left = 190, Top = 450, Width = 85, Text = "СохранитьPL" };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnLoad = new Button { Left = 280, Top = 450, Width = 80, Text = "ЗагрузитьPL" };
            btnLoad.Click += BtnLoad_Click;
            this.Controls.Add(btnLoad);

            btnEditPlaylistForm = new Button { Left = 370, Top = 450, Width = 130, Text = "Изменить плейлист..." };
            btnEditPlaylistForm.Click += BtnEditPlaylistForm_Click;
            this.Controls.Add(btnEditPlaylistForm);

            btnPlay = new Button { Left = 430, Top = 10, Width = 80, Text = "Играть" };
            btnPlay.Click += btnPlay_Click;
            this.Controls.Add(btnPlay);

            btnPause = new Button { Left = 520, Top = 10, Width = 80, Text = "Пауза" };
            btnPause.Click += (s, e) => audioPlayer.Pause();
            this.Controls.Add(btnPause);

            btnStop = new Button { Left = 610, Top = 10, Width = 80, Text = "Стоп" };
            btnStop.Click += btnStop_Click;
            this.Controls.Add(btnStop);

            trVolume = new TrackBar { Left = 460, Top = 60, Width = 260, Minimum = 0, Maximum = 100, Value = 80 };
            trVolume.Scroll += (s, e) => audioPlayer.SetVolume(trVolume.Value / 100f);
            this.Controls.Add(trVolume);

            trPosition = new TrackBar { Left = 430, Top = 120, Width = 400, Minimum = 0, Maximum = 1000, Value = 0 };
            trPosition.MouseDown += (s, e) => uiTimer.Stop();
            trPosition.MouseUp += TrPosition_MouseUp;
            this.Controls.Add(trPosition);

            lblTime = new Label { Left = 430, Top = 170, Width = 250, Text = "00:00 / 00:00" };
            this.Controls.Add(lblTime);

            lblTitle = new Label { Left = 430, Top = 200, Width = 440, Text = "Название: -" };
            this.Controls.Add(lblTitle);

            lblArtist = new Label { Left = 430, Top = 230, Width = 440, Text = "Исполнитель: -" };
            this.Controls.Add(lblArtist);

            trEQLow = new TrackBar { Left = 475, Top = 310, Width = 400, Minimum = -12, Maximum = 12, Value = 0 };
            trEQLow.Scroll += (s, e) => UpdateEQ();
            this.Controls.Add(trEQLow);

            trEQMid = new TrackBar { Left = 475, Top = 360, Width = 400, Minimum = -12, Maximum = 12, Value = 0 };
            trEQMid.Scroll += (s, e) => UpdateEQ();
            this.Controls.Add(trEQMid);

            trEQHigh = new TrackBar { Left = 475, Top = 410, Width = 400, Minimum = -12, Maximum = 12, Value = 0 };
            trEQHigh.Scroll += (s, e) => UpdateEQ();
            this.Controls.Add(trEQHigh);

            // Подпись "Звук"
            var lblVolume = new Label
            {
                Left = 430,
                Top = 60,
                Width = 60,
                Height = 20,
                Text = "Звук:"
            };
            this.Controls.Add(lblVolume);

            // Заголовок "Эквалайзер"
            var lblEQTitle = new Label
            {
                Left = 420,
                Top = 280,
                Width = 520,
                Height = 20,
                Text = "Эквалайзер",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblEQTitle);

            // Подпись для Low
            var lblEQLow = new Label
            {
                Left = 430,
                Top = 310,
                Width = 80,
                Height = 20,
                Text = "EQLow:"
            };
            this.Controls.Add(lblEQLow);

            // Подпись для Mid
            var lblEQMid = new Label
            {
                Left = 430,
                Top = 360,
                Width = 80,
                Height = 20,
                Text = "EQMid:"
            };
            this.Controls.Add(lblEQMid);

            // Подпись для High
            var lblEQHigh = new Label
            {
                Left = 430,
                Top = 410,
                Width = 80,
                Height = 20,
                Text = "EQHigh:"
            };
            this.Controls.Add(lblEQHigh);

        }
        #endregion

        // ------------------ Buttons -----------------------

        private void btnPlay_Click(object sender, EventArgs e)
        {
            // Если сейчас на паузе и output доступен — просто возобновляем
            if (audioPlayer.OutputDevice != null &&
                audioPlayer.OutputDevice.PlaybackState == PlaybackState.Paused &&
                audioPlayer.Reader != null)
            {
                audioPlayer.Resume();
                return;
            }

            PlaySelected();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            audioPlayer.Stop();

            trPosition.Value = 0;
            lblTime.Text = "00:00 / 00:00";
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstPlaylist.SelectedItem == null)
            {
                MessageBox.Show("Выберите трек для удаления.");
                return;
            }

            int index = lstPlaylist.SelectedIndex;

            // Если удаляем текущий проигрываемый трек — остановить
            if (audioPlayer.Reader != null && index >= 0 &&
                index < playlistManager.Playlist.Count &&
                playlistManager.Playlist[index].FilePath == audioPlayer.Reader.FileName)
            {
                audioPlayer.Stop();
            }

            playlistManager.Playlist.RemoveAt(index);
            RefreshPlaylistListBox();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Audio Files|*.mp3;*.wav;*.flac;*.m4a"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var f in dlg.FileNames)
                        playlistManager.Playlist.Add(new PlaylistItem { FilePath = f, DisplayName = Path.GetFileName(f) });

                    RefreshPlaylistListBox();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog { Filter = "Playlist|*.json" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    playlistManager.Save(dlg.FileName);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Playlist|*.json" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    playlistManager.Load(dlg.FileName);
                    RefreshPlaylistListBox();
                }
            }
        }

        private void BtnEditPlaylistForm_Click(object sender, EventArgs e)
        {
            var form = new PlaylistForm(playlistManager);
            form.PlaylistUpdated += (list) =>
            {
                playlistManager.SetPlaylist(list);
                RefreshPlaylistListBox();
            };
            form.Show();
        }

        // ------------------ Playback -----------------------

        private void PlaySelected()
        {
            if (lstPlaylist.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите трек");
                return;
            }

            var item = playlistManager.Playlist[lstPlaylist.SelectedIndex];
            var decoder = DecoderFactory.GetDecoderForFile(item.FilePath);

            try
            {
                audioPlayer.Play(
                    item.FilePath,
                    trVolume.Value / 100f,
                    new EqualizerBand[]
                    {
                        new EqualizerBand{Frequency = 100, Bandwidth = 0.8f, Gain = trEQLow.Value},
                        new EqualizerBand{Frequency = 1000, Bandwidth = 1.0f, Gain = trEQMid.Value},
                        new EqualizerBand{Frequency = 8000, Bandwidth = 0.8f, Gain = trEQHigh.Value},
                    },
                    decoder
                );

                UpdateTrackMetadata(item.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка воспроизведения: " + ex.Message);
            }
        }

        private void UpdateTrackMetadata(string path)
        {
            try
            {
                var tags = MetadataReader.Read(path);
                lblTitle.Text = "Title: " + (tags.Title ?? Path.GetFileName(path));
                lblArtist.Text = "Artist: " + (tags.Artist ?? "Unknown");
            }
            catch
            {
                lblTitle.Text = "Title: " + Path.GetFileName(path);
                lblArtist.Text = "Artist: Unknown";
            }
        }

        // PlaybackStopped теперь имеет сигнатуру EventHandler
        private void AudioPlayer_PlaybackStopped(object sender, EventArgs e)
        {
            // Здесь можно реализовать автоматическое переключение на следующий трек
            // или другие действия по завершении воспроизведения.
            // Пример (не включаем автопереход по Stop): если Reader != null и позиция в конце -> следующий
            try
            {
                if (audioPlayer.Reader != null)
                {
                    var cur = audioPlayer.Reader.CurrentTime;
                    var tot = audioPlayer.Reader.TotalTime;
                    if (tot.TotalSeconds > 0 && cur >= tot)
                    {
                        int next = lstPlaylist.SelectedIndex + 1;
                        if (next < playlistManager.Playlist.Count)
                        {
                            lstPlaylist.SelectedIndex = next;
                            PlaySelected();
                        }
                    }
                }
            }
            catch { /* безопасно игнорируем */ }
        }

        private void TrPosition_MouseUp(object sender, MouseEventArgs e)
        {
            if (audioPlayer.Reader == null) { uiTimer.Start(); return; }

            double pct = trPosition.Value / (double)trPosition.Maximum;
            audioPlayer.Seek(pct);

            uiTimer.Start();
        }

        private void UpdateEQ()
        {
            if (audioPlayer.Equalizer == null) return;

            audioPlayer.Equalizer.UpdateBand(0, trEQLow.Value);
            audioPlayer.Equalizer.UpdateBand(1, trEQMid.Value);
            audioPlayer.Equalizer.UpdateBand(2, trEQHigh.Value);
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (audioPlayer.Reader == null) return;

            var cur = audioPlayer.Reader.CurrentTime;
            var tot = audioPlayer.Reader.TotalTime;

            lblTime.Text = $"{cur:mm\\:ss} / {tot:mm\\:ss}";

            if (tot.TotalSeconds > 0)
                trPosition.Value = (int)(cur.TotalSeconds / tot.TotalSeconds * trPosition.Maximum);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                audioPlayer.TogglePlayPause();
        }
    }
}
