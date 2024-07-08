using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using DxLibDLL;

namespace RALLYDX {

	public class GameForm : Form {

        private DxGraph _DXGRAPH = null;
        private DxFonts _DXFONTS = null;
        private DxSound _DXSOUND = null;
        private DxInput _DXINPUT = null;

        private StageMap _STAGEMAP = null;
        private MyCar _MYCAR = null;
        private SmokeScreen _SMOKE = null;
        private CheckPoint _CHECKPOINT = null;
        private Rocks _ROCKS = null;

        private GameTitle _GAME_TITLE = null;
        private GameMain _GAME_MAIN = null;
        private GameExtra _GAME_EXTRA = null;
        private GameChamp _GAME_CHAMP = null;

        private GameState _GAME = new GameState();
        private GameState _GAME_SAVE = new GameState();

		/// <summary>
		/// GameFormクラスのコンストラクタ。
		/// </summary>
		public GameForm() {

			this.Text = Settings.WINDOW_TITLE;
			this.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
			this.ClientSize = new Size(Settings.FORM_WIDTH, Settings.FORM_HEIGHT);
			this.ShowInTaskbar = true;
			this.StartPosition = FormStartPosition.WindowsDefaultLocation;
			this.Opacity = 1.0;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.BackColor = Color.Black;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.AllowDrop = false;
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);

            // Log.txtの生成を設定
            DX.SetOutApplicationLogValidFlag(Settings.DX_LOG);

            // DxLibの親ウインドウをこのフォームウインドウにセット
            DX.SetUserWindow(this.Handle);
            DX.SetWindowSizeChangeEnableFlag(DX.TRUE);
            DX.SetFullScreenResolutionMode(DX.DX_FSRESOLUTIONMODE_DESKTOP);
            DX.SetFullScreenScalingMode(DX.DX_FSSCALINGMODE_BILINEAR);
            DX.ChangeWindowMode(Settings.DX_WINDOW_MODE);
            DX.SetGraphMode(Settings.FORM_WIDTH, Settings.FORM_HEIGHT, Settings.COLOR_DEPTH);

            // DxLibの初期化
            if (DX.DxLib_Init() == -1)
                ExitApplication(Settings.ERR_NO_INIT);
            
            // 初期化後
            _DXGRAPH = DxGraph.GetInstance();
            _DXFONTS = DxFonts.GetInstance();
            _DXSOUND = DxSound.GetInstance();
            _DXINPUT = DxInput.GetInstance();
            
            _STAGEMAP = StageMap.GetInstance();

            _CHECKPOINT = CheckPoint.GetInstance();
            _ROCKS = Rocks.GetInstance();
            //_MYCAR = MyCar.GetInstance();
            _SMOKE = SmokeScreen.GetInstance();

            _GAME_TITLE = new GameTitle();
            _GAME_MAIN = new GameMain();
            _GAME_EXTRA = new GameExtra();
            _GAME_CHAMP = new GameChamp();
		}

        /// <summary>
        /// ゲームのメインループ。
        /// </summary>
        public void MainLoop() {

            // fps調整用
            Stopwatch frameWatch = Stopwatch.StartNew();

            // ダイレクトインプットの状態を更新
            _DXINPUT.Update();

            _STAGEMAP.SYSTIME++;
            if (_STAGEMAP.SYSTIME > 59)
                _STAGEMAP.SYSTIME = 0;

            // ESCキーが押下されるまで繰り返す
            if (_DXINPUT.IsKeyTrigger(DX.KEY_INPUT_ESCAPE)) {
                this.Close();
                return;
            }

            //
            if (_GAME_SAVE.State != _GAME.State) {
                if (_GAME.State == Now.GameTitle)
                    InitGameTitle();
                else if (_GAME.State == Now.NormalStage)
                    InitGameMain();
                else if (_GAME.State == Now.ExtraStage)
                    InitExtraStage();
                else if (_GAME.State == Now.HighScore)
                    InitHighScore();
                else if (_GAME.State == Now.NextStage) {
                    ContinueMain();
                    _GAME.State = Now.NormalStage;
                }
                _GAME_SAVE.State = _GAME.State;
            }

            // 状態に合わせてクラスを選択する
            if (_GAME.State == Now.GameTitle)
                _GAME = _GAME_TITLE.DrawScreen(_GAME);
            else if (_GAME.State == Now.ExtraStage)
                _GAME = _GAME_EXTRA.DrawScreen(_GAME);
            else if (_GAME.State == Now.HighScore)
                _GAME = _GAME_CHAMP.DrawScreen(_GAME);
            else
                _GAME = _GAME_MAIN.DrawScreen(_GAME);

            // 指定時間まで待つ
            frameWatch.Stop();
            long timeTaken = frameWatch.ElapsedMilliseconds;
            int sleepTime = (int)((1000.0 / Settings.DX_FPS) - timeTaken);
            Thread.Sleep(Math.Max(sleepTime, 0));
        }

        /// <summary>
        /// フォームイベント：
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(Object sender, FormClosingEventArgs e) {

            DX.DxLib_End();
        }

        /// <summary>
        /// アプリケーションの強制終了。
        /// </summary>
        /// <param name="s">メッセージボックスに表示する文字列を指定する。</param>
        private void ExitApplication(string s) {

            MessageBox.Show(s);
            Environment.Exit(0x8020);
        }

        /// <summary>
        /// アバンタイトルを初期化する。
        /// </summary>
        private void InitGameTitle() {

            _GAME_TITLE.Init();
        }

        /// <summary>
        /// ゲームを行うために初期化する。
        /// </summary>
        private void InitGameMain() {

            _GAME_MAIN.Init();
        }

        /// <summary>
        /// チャレンジングステージを行うために初期化する。
        /// </summary>
        private void InitExtraStage() {

            _GAME_EXTRA.Init();
        }

        /// <summary>
        /// ハイスコア表示のために初期化する。
        /// </summary>
        private void InitHighScore() {

            _GAME_CHAMP.Init();
        }

        /// <summary>
        /// チャレンジングステージから通常ステージに戻るための特別な初期化。
        /// </summary>
        private void ContinueMain() {

            _GAME_MAIN.NextRound();
        }
	}

    public class GameState {

        public GameState() {

            State = Now.GameTitle;
        }
        public Now State { get; set; }
    }

    public enum Now {
        GameTitle,
        NormalStage,
        ExtraStage,
        NextStage,
        HighScore
    }

}