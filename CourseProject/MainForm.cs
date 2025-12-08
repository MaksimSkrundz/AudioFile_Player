using CourseProject.Playlist;
using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class MainForm : Form
    {
        private PlaylistManager playlistManager;
        private AudioPlayer audioPlayer;
        private Timer uiTimer;

        private ListBox lstPlaylist;
        private Button btnAdd, btnRemove, btnEditPlaylistForm, btnPlay, btnPause, btnStop, btnSave, btnLoad;
        private TrackBar trVolume, trPosition, trEQLow, trEQMid, trEQHigh;
        private Label lblTime, lblTitle, lblArtist;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustom();
        }

        private void InitializeCustom()
        {
            playlistManager = new PlaylistManager();
            audioPlayer = new AudioPlayer();
            audioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;

            uiTimer = new Timer { Interval = 200 };
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            // === Громкость по умолчанию ===
            trVolume.Value = 80;
            audioPlayer.SetVolume(0.8f);

            // === ПОДКЛЮЧАЕМ ВСЕ ГОРЯЧИЕ КЛАВИШИ И КОЛЁСИКО ОДНИМ ВЫЗОВОМ ===
            Hotkeys.Setup(
                form: this,
                playOrResume: () => PlayOrResume(),
                pause: () => audioPlayer.Pause(),
                stop: () => StopAndReset(),
                nextTrack: () => NextTrack(),
                previousTrack: () => PreviousTrack(),
                getVolume: () => trVolume.Value,
                setVolume: vol =>
                {
                    trVolume.Value = vol;
                    audioPlayer.SetVolume(vol / 100f);
                }
            );

            RefreshPlaylistListBox();
        }

        private void PlayOrResume()
        {
            if (audioPlayer.OutputDevice == null ||
                audioPlayer.OutputDevice.PlaybackState == PlaybackState.Stopped)
            {
                PlaySelected();
            }
            else if (audioPlayer.OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                audioPlayer.Resume();
            }
        }

        private void StopAndReset()
        {
            audioPlayer.Stop();
            trPosition.Value = 0;
            lblTime.Text = "00:00 / 00:00";
        }

        private void NextTrack()
        {
            if (lstPlaylist.Items.Count == 0) return;
            int next = (lstPlaylist.SelectedIndex + 1) % lstPlaylist.Items.Count;
            lstPlaylist.SelectedIndex = next;
            PlaySelected();
        }

        private void PreviousTrack()
        {
            if (lstPlaylist.Items.Count == 0) return;
            int prev = lstPlaylist.SelectedIndex - 1;
            if (prev < 0) prev = lstPlaylist.Items.Count - 1;
            lstPlaylist.SelectedIndex = prev;
            PlaySelected();
        }

        private void RefreshPlaylistListBox()
        {
            lstPlaylist.Items.Clear();
            foreach (var item in playlistManager.Playlist)
                lstPlaylist.Items.Add(item.DisplayName);

            if (lstPlaylist.Items.Count > 0 && lstPlaylist.SelectedIndex == -1)
                lstPlaylist.SelectedIndex = 0;
        }

        #region InitializeComponent — UI (без изменений, но с Anchor)
        private void InitializeComponent()
        {
            this.Text = "Аудиоплеер";
            this.Size = new Size(900, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lstPlaylist = new ListBox
            {
                Left = 10,
                Top = 10,
                Width = 400,
                Height = 430,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };
            lstPlaylist.DoubleClick += (s, e) => { if (lstPlaylist.SelectedIndex >= 0) PlaySelected(); };
            this.Controls.Add(lstPlaylist);

            btnAdd = CreateButton(10, 450, 80, "Добавить", BtnAdd_Click);
            btnRemove = CreateButton(100, 450, 80, "Удалить", BtnRemove_Click);
            btnSave = CreateButton(190, 450, 85, "СохранитьPL", BtnSave_Click);
            btnLoad = CreateButton(280, 450, 80, "ЗагрузитьPL", BtnLoad_Click);
            btnEditPlaylistForm = CreateButton(370, 450, 130, "Изменить плейлист...", BtnEditPlaylistForm_Click);

            btnPlay = CreateButton(430, 10, 80, "Играть", (s, e) => PlayOrResume());
            btnPause = CreateButton(520, 10, 80, "Пауза", (s, e) => audioPlayer.Pause());
            btnStop = CreateButton(610, 10, 80, "Стоп", (s, e) => StopAndReset());

            var lblVolume = new Label { Left = 430, Top = 60, Width = 40, Text = "Звук:" };
            this.Controls.Add(lblVolume);

            trVolume = new TrackBar
            {
                Left = 480,
                Top = 60,
                Width = 290,
                Minimum = 0,
                Maximum = 100,
                Value = 80,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            trVolume.Scroll += (s, e) => audioPlayer.SetVolume(trVolume.Value / 100f);
            this.Controls.Add(trVolume);

            trPosition = new TrackBar
            {
                Left = 430,
                Top = 120,
                Width = 400,
                Minimum = 0,
                Maximum = 1000,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            trPosition.MouseDown += (s, e) => uiTimer.Stop();
            trPosition.MouseUp += (s, e) =>
            {
                if (audioPlayer.Reader != null)
                {
                    double ratio = trPosition.Value / (double)trPosition.Maximum;
                    audioPlayer.Seek(ratio);
                }
                uiTimer.Start();
            };
            this.Controls.Add(trPosition);

            lblTime = new Label { Left = 430, Top = 170, Width = 250, Text = "00:00 / 00:00" };
            lblTitle = new Label { Left = 430, Top = 200, Width = 440, Text = "Название: -" };
            lblArtist = new Label { Left = 430, Top = 230, Width = 440, Text = "Исполнитель: -" };
            this.Controls.Add(lblTime); this.Controls.Add(lblTitle); this.Controls.Add(lblArtist);

            trEQLow = CreateEQTrackBar(310, "EQLow:");
            trEQMid = CreateEQTrackBar(360, "EQMid:");
            trEQHigh = CreateEQTrackBar(410, "EQHigh:");

            var lblEQTitle = new Label
            {
                Left = 440,
                Top = 280,
                Width = 440,
                Height = 25,
                Text = "Эквалайзер",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblEQTitle);
        }

        private Button CreateButton(int l, int t, int w, string text, EventHandler click)
        {
            var b = new Button { Left = l, Top = t, Width = w, Height = 28, Text = text };
            b.Click += click;
            this.Controls.Add(b);
            return b;
        }

        private TrackBar CreateEQTrackBar(int top, string label)
        {
            var l = new Label { Left = 430, Top = top, Width = 50, Text = label };
            var tb = new TrackBar
            {
                Left = 490,
                Top = top,
                Width = 350,
                Minimum = -12,
                Maximum = 12,
                Value = 0,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            tb.Scroll += (s, e) => UpdateEQ();
            this.Controls.Add(l);
            this.Controls.Add(tb);
            return tb;
        }
        #endregion

        private void PlaySelected()
        {
            if (lstPlaylist.SelectedIndex < 0) { MessageBox.Show("Выберите трек"); return; }
            var item = playlistManager.Playlist[lstPlaylist.SelectedIndex];
            var decoder = DecoderFactory.GetDecoderForFile(item.FilePath);

            try
            {
                audioPlayer.Play(
                    item.FilePath,
                    trVolume.Value / 100f,
                    new EqualizerBand[]
                    {
                        new EqualizerBand { Frequency = 100, Bandwidth = 0.8f, Gain = trEQLow.Value },
                        new EqualizerBand { Frequency = 1000, Bandwidth = 1.0f, Gain = trEQMid.Value },
                        new EqualizerBand { Frequency = 8000, Bandwidth = 0.8f, Gain = trEQHigh.Value }
                    },
                    decoder
                );
                UpdateTrackMetadata(item.FilePath);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void UpdateTrackMetadata(string path)
        {
            try
            {
                var tags = MetadataReader.Read(path);
                lblTitle.Text = "Название: " + (tags.Title ?? Path.GetFileName(path));
                lblArtist.Text = "Исполнитель: " + (tags.Artist ?? "Неизвестно");
            }
            catch
            {
                lblTitle.Text = "Название: " + Path.GetFileName(path);
                lblArtist.Text = "Исполнитель: Неизвестно";
            }
        }

        private void UpdateEQ()
        {
            if (audioPlayer.Equalizer != null)
            {
                audioPlayer.Equalizer.UpdateBand(0, trEQLow.Value);
                audioPlayer.Equalizer.UpdateBand(1, trEQMid.Value);
                audioPlayer.Equalizer.UpdateBand(2, trEQHigh.Value);
            }
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (audioPlayer.Reader == null) return;
            var cur = audioPlayer.Reader.CurrentTime;
            var tot = audioPlayer.Reader.TotalTime;
            lblTime.Text = $"{cur:mm\\:ss} / {tot:mm\\:ss}";
            if (tot.TotalSeconds > 0)
            {
                int pos = (int)(cur.TotalSeconds / tot.TotalSeconds * trPosition.Maximum);
                if (Math.Abs(trPosition.Value - pos) > 5)
                    trPosition.Value = pos;
            }
        }

        private void AudioPlayer_PlaybackStopped(object sender, EventArgs e)
        {
            if (audioPlayer.Reader != null && audioPlayer.Reader.CurrentTime >= audioPlayer.Reader.TotalTime - TimeSpan.FromSeconds(1))
            {
                this.BeginInvoke((MethodInvoker)NextTrack);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstPlaylist.SelectedIndex < 0) { MessageBox.Show("Выберите трек для удаления."); return; }

            int index = lstPlaylist.SelectedIndex;

            if (audioPlayer.Reader != null &&
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
    }
}