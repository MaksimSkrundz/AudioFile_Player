using CourseProject.Playlist;
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
        private PlaybackController playbackController;
        private PlaylistController playlistController;
        private PositionController positionController;
        private MetadataController metadataController;
        private EQController eqController;
        private HotkeysController hotkeysController;

        private ListBox lstPlaylist;
        private Button btnAdd, btnRemove, btnEditPlaylistForm, btnPlay, btnPause, btnStop, btnSave, btnLoad;
        private TrackBar trVolume, trPosition, trEQLow, trEQMid, trEQHigh;
        private Label lblTime, lblTitle, lblArtist;

        public MainForm()
        {
            InitializeComponent();   // интерфейс
            InitializeCustom();      // логика
        }

        private void InitializeCustom()
        {
            //  папка Playlists в корне проекта при запуске
            string playlistsDir = Path.Combine(PathHelper.GetProjectRoot(), "Playlists");
            if (!Directory.Exists(playlistsDir))
                Directory.CreateDirectory(playlistsDir);

            // 1. Основные объекты
            playlistManager = new PlaylistManager();
            audioPlayer = new AudioPlayer();

            // 2. Контроллеры (порядок важен!)
            metadataController = new MetadataController(lblTitle, lblArtist);
            playbackController = new PlaybackController(audioPlayer, playlistManager, lstPlaylist, metadataController);
            playlistController = new PlaylistController(playlistManager, lstPlaylist, RefreshPlaylistListBox);
            positionController = new PositionController(audioPlayer, trPosition, lblTime);
            eqController = new EQController(audioPlayer, trEQLow, trEQMid, trEQHigh);

            // 3. Горячие клавиши
            hotkeysController = new HotkeysController(
                this,
                playbackController,
                () => trVolume.Value,
                vol => { trVolume.Value = vol; audioPlayer.SetVolume(vol / 100f); },
                () => playlistController.AddTracks(),
                () => playlistController.RemoveSelected()
            );

            // 4. Подписки на события
            playbackController.OnStopReset += () => positionController.Reset();

            // 5. Начальные значения
            trVolume.Value = 80;
            audioPlayer.SetVolume(0.8f);
            RefreshPlaylistListBox();
        }

        private void RefreshPlaylistListBox()
        {
            lstPlaylist.Items.Clear();
            foreach (var item in playlistManager.Playlist)
                lstPlaylist.Items.Add(item.DisplayName);

            if (lstPlaylist.Items.Count > 0 && lstPlaylist.SelectedIndex == -1)
                lstPlaylist.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            this.Text = "Аудиоплеер";
            this.Size = new Size(900, 520);
            this.StartPosition = FormStartPosition.CenterScreen;

            // список треков
            lstPlaylist = new ListBox { Left = 10, Top = 10, Width = 400, Height = 430 };
            lstPlaylist.DoubleClick += (s, e) => playbackController.PlaySelected();
            this.Controls.Add(lstPlaylist);

            // кнопки плейлиста
            btnAdd = CreateButton(10, 450, 80, "Добавить", (s, e) => playlistController.AddTracks());
            btnRemove = CreateButton(100, 450, 80, "Удалить", (s, e) => playlistController.RemoveSelected());
            btnSave = CreateButton(190, 450, 90, "СохранитьPL", (s, e) => playlistController.SavePlaylist());
            btnLoad = CreateButton(290, 450, 90, "ЗагрузитьPL", (s, e) => playlistController.LoadPlaylist());
            btnEditPlaylistForm = CreateButton(390, 450, 100, "ИзменитьPL", BtnEditPlaylistForm_Click);

            // кнопки управления воспроизведением
            btnPlay = CreateButton(430, 10, 80, "Играть", (s, e) => playbackController.PlaySelected());
            btnPause = CreateButton(520, 10, 80, "Пауза", (s, e) => playbackController.Pause());
            btnStop = CreateButton(610, 10, 80, "Стоп", (s, e) => playbackController.Stop());

            // громкость
            this.Controls.Add(new Label { Left = 430, Top = 60, Width = 40, Text = "Звук:" });
            trVolume = new TrackBar { Left = 480, Top = 60, Width = 290, Minimum = 0, Maximum = 100, Value = 80, TickFrequency = 10 };
            trVolume.Scroll += (s, e) => audioPlayer.SetVolume(trVolume.Value / 100f);
            this.Controls.Add(trVolume);

            // ползунок позиции
            trPosition = new TrackBar { Left = 430, Top = 120, Width = 400, Minimum = 0, Maximum = 1000, TickFrequency = 100 };
            this.Controls.Add(trPosition);

            // время
            lblTime = new Label { Left = 430, Top = 170, Width = 250, Text = "00:00 / 00:00", Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(lblTime);

            // метаданные
            lblTitle = new Label { Left = 430, Top = 200, Width = 440, Text = "Название: -", Font = new Font("Segoe UI", 10F) };
            lblArtist = new Label { Left = 430, Top = 230, Width = 440, Text = "Исполнитель: -", Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblArtist);

            // эквалайзер
            this.Controls.Add(new Label
            {
                Left = 430,
                Top = 280,
                Width = 440,
                Height = 25,
                Text = "Эквалайзер",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });

            trEQLow = CreateEQTrackBar(310, "Низкие:");
            trEQMid = CreateEQTrackBar(360, "Средние:");
            trEQHigh = CreateEQTrackBar(410, "Высокие:");
        }

        // методы для создания кнопок и ползунков
        private Button CreateButton(int x, int y, int w, string text, EventHandler handler)
        {
            var btn = new Button { Left = x, Top = y, Width = w, Height = 30, Text = text, Font = new Font("Segoe UI", 9F) };
            btn.Click += handler;
            this.Controls.Add(btn);
            return btn;
        }

        private TrackBar CreateEQTrackBar(int top, string label)
        {
            this.Controls.Add(new Label { Left = 430, Top = top, Width = 60, Text = label, Font = new Font("Segoe UI", 9F) });
            var tr = new TrackBar { Left = 500, Top = top, Width = 330, Minimum = -12, Maximum = 12, TickFrequency = 3 };
            tr.Scroll += (s, e) => eqController.UpdateEQ();
            this.Controls.Add(tr);
            return tr;
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