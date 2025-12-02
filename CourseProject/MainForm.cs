using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private ListBox lstPlaylist;
    private TrackBar trackVolume;
    private TrackBar trackPosition;
    private TrackBar eqLow;
    private TrackBar eqMid;
    private TrackBar eqHigh;
    private Label lblTime;
    private Label lblVolume;
    private Label lblLow;
    private Label lblMid;
    private Label lblHigh;
    private Label Eq;
    private Button btnAdd;
    private Button btnRemove;
    private Button btnSave;
    private Button btnLoad;
    private Button btnPlay;
    private Button btnPause;
    private Button btnStop;

    private Timer timer;
    private List<PlaylistItem> playlist = new List<PlaylistItem>();
    private AudioPlayer player = new AudioPlayer();

    public MainForm()
    {
        Text = "Плеер аудиофайлов с плейлистами и базовым эквалайзером";
        Width = 800;
        Height = 420;

        InitializeComponent();

        timer = new Timer { Interval = 200 };
        timer.Tick += Timer_Tick;
        timer.Start();

        player.PlaybackStopped += OnTrackFinished;
    }

    private void InitializeComponent()
    {
        // Плейлист
        lstPlaylist = new ListBox();
        lstPlaylist.Location = new System.Drawing.Point(10, 10);
        lstPlaylist.Size = new System.Drawing.Size(380, 300);
        lstPlaylist.DoubleClick += (s, e) => PlaySelected();

        // Кнопки управления плейлистом
        btnAdd = new Button();
        btnAdd.Text = "Добавить";
        btnAdd.Location = new System.Drawing.Point(10, 320);
        btnAdd.Click += BtnAdd_Click;

        btnRemove = new Button();
        btnRemove.Text = "Удалить";
        btnRemove.Location = new System.Drawing.Point(100, 320);
        btnRemove.Click += (s, e) =>
        {
            if (lstPlaylist.SelectedIndex >= 0)
            {
                playlist.RemoveAt(lstPlaylist.SelectedIndex);
                RefreshPlaylist();
            }
        };

        btnSave = new Button();
        btnSave.Text = "Сохранить";
        btnSave.Location = new System.Drawing.Point(190, 320);
        btnSave.Click += (s, e) =>
        {
            var dlg = new SaveFileDialog { Filter = "JSON|*.json" };
            if (dlg.ShowDialog() == DialogResult.OK)
                PlaylistManager.Save(dlg.FileName, playlist);
        };

        btnLoad = new Button();
        btnLoad.Text = "Загрузить";
        btnLoad.Location = new System.Drawing.Point(280, 320);
        btnLoad.Click += (s, e) =>
        {
            var dlg = new OpenFileDialog { Filter = "JSON|*.json" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                playlist = PlaylistManager.Load(dlg.FileName);
                RefreshPlaylist();
            }
        };

        // Кнопки управления воспроизведением
        btnPlay = new Button();
        btnPlay.Text = "Играть";
        btnPlay.Location = new System.Drawing.Point(400, 10);
        btnPlay.Click += (s, e) => PlaySelected();

        btnPause = new Button();
        btnPause.Text = "Пауза";
        btnPause.Location = new System.Drawing.Point(500, 10);
        btnPause.Click += (s, e) => player.Pause();

        btnStop = new Button();
        btnStop.Text = "Стоп";
        btnStop.Location = new System.Drawing.Point(600, 10);
        btnStop.Click += (s, e) =>
        {
            player.Stop();
            trackPosition.Value = 0;
            lblTime.Text = "00:00/00:00";
        };

        // Метка и регулятор громкости
        lblVolume = new Label();
        lblVolume.Text = "Звук:";
        lblVolume.Location = new System.Drawing.Point(400, 50);
        lblVolume.Size = new System.Drawing.Size(50, 20);

        trackVolume = new TrackBar();
        trackVolume.Location = new System.Drawing.Point(450, 50);
        trackVolume.Size = new System.Drawing.Size(250, 45);
        trackVolume.Minimum = 0;
        trackVolume.Maximum = 100;
        trackVolume.Value = 80;
        trackVolume.Scroll += (s, e) => player.SetVolume(trackVolume.Value / 100f);

        // Прогресс трека
        trackPosition = new TrackBar();
        trackPosition.Location = new System.Drawing.Point(400, 95);
        trackPosition.Size = new System.Drawing.Size(350, 45);
        trackPosition.Minimum = 0;
        trackPosition.Maximum = 1000;
        trackPosition.MouseUp += (s, e) =>
        {
            player.Seek(trackPosition.Value / 1000.0);
        };

        // Время
        lblTime = new Label();
        lblTime.Location = new System.Drawing.Point(400, 140);
        lblTime.Size = new System.Drawing.Size(200, 20);
        lblTime.Text = "00:00/00:00";

        // Заголовок эквалайзера
        Eq = new Label();
        Eq.Text = "Эквалайзер";
        Eq.Location = new System.Drawing.Point(560, 165);
        Eq.Size = new System.Drawing.Size(70, 20);

        // Low
        lblLow = new Label();
        lblLow.Text = "Low:";
        lblLow.Location = new System.Drawing.Point(400, 190);
        lblLow.Size = new System.Drawing.Size(40, 20);

        eqLow = new TrackBar();
        eqLow.Location = new System.Drawing.Point(450, 190);
        eqLow.Size = new System.Drawing.Size(300, 45);
        eqLow.Minimum = -12;
        eqLow.Maximum = 12;
        eqLow.Scroll += (s, e) => UpdateEQ();

        // Mid
        lblMid = new Label();
        lblMid.Text = "Mid:";
        lblMid.Location = new System.Drawing.Point(400, 235);
        lblMid.Size = new System.Drawing.Size(40, 20);

        eqMid = new TrackBar();
        eqMid.Location = new System.Drawing.Point(450, 235);
        eqMid.Size = new System.Drawing.Size(300, 45);
        eqMid.Minimum = -12;
        eqMid.Maximum = 12;
        eqMid.Scroll += (s, e) => UpdateEQ();

        // High
        lblHigh = new Label();
        lblHigh.Text = "High:";
        lblHigh.Location = new System.Drawing.Point(400, 280);
        lblHigh.Size = new System.Drawing.Size(40, 20);

        eqHigh = new TrackBar();
        eqHigh.Location = new System.Drawing.Point(450, 280);
        eqHigh.Size = new System.Drawing.Size(300, 45);
        eqHigh.Minimum = -12;
        eqHigh.Maximum = 12;
        eqHigh.Scroll += (s, e) => UpdateEQ();

        Controls.AddRange(new Control[] {
            lstPlaylist, btnAdd, btnRemove, btnSave, btnLoad,
            btnPlay, btnPause, btnStop,
            lblVolume, trackVolume,
            trackPosition, lblTime, Eq,
            lblLow, eqLow, lblMid, eqMid, lblHigh, eqHigh
        });
    }

    void BtnAdd_Click(object sender, EventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Audio|*.mp3;*.wav;*.flac;*.aiff;*.m4a;*.wma"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            foreach (var f in dlg.FileNames)
                playlist.Add(new PlaylistItem { FilePath = f, DisplayName = Path.GetFileName(f) });

            RefreshPlaylist();
        }
    }

    void RefreshPlaylist()
    {
        lstPlaylist.Items.Clear();
        foreach (var p in playlist)
            lstPlaylist.Items.Add(p.DisplayName);
    }

    void PlaySelected()
    {
        if (lstPlaylist.SelectedIndex < 0) return;

        var item = playlist[lstPlaylist.SelectedIndex];
        player.Play(item.FilePath, trackVolume.Value / 100f, GetBands());
    }

    EqualizerBand[] GetBands() => new[]
    {
        new EqualizerBand { Frequency = 100f, Bandwidth = 0.8f, Gain = eqLow.Value },
        new EqualizerBand { Frequency = 1000f, Bandwidth = 1.0f, Gain = eqMid.Value },
        new EqualizerBand { Frequency = 8000f, Bandwidth = 0.8f, Gain = eqHigh.Value }
    };

    void UpdateEQ()
    {
        if (player.Equalizer == null) return;

        player.Equalizer.UpdateBand(0, eqLow.Value);
        player.Equalizer.UpdateBand(1, eqMid.Value);
        player.Equalizer.UpdateBand(2, eqHigh.Value);
    }

    void Timer_Tick(object sender, EventArgs e)
    {
        if (player.Reader == null) return;

        var cur = player.Reader.CurrentTime;
        var tot = player.Reader.TotalTime;

        lblTime.Text = $"{cur:mm\\:ss}/{tot:mm\\:ss}";
        trackPosition.Value = (int)(cur.TotalSeconds / tot.TotalSeconds * 1000);
    }

    void OnTrackFinished()
    {
        int next = lstPlaylist.SelectedIndex + 1;

        if (next < playlist.Count)
        {
            lstPlaylist.SelectedIndex = next;
            PlaySelected();
        }
    }
}