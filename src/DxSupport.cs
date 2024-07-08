using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// DxGraphクラス。
    /// DxLibで使用するグラフィックをメモリーに読み込み、利用可能にする。
    /// </summary>
	public class DxGraph {
		
        // インスタンス
        private static DxGraph singleton = null;

        // 各種ビットマップ情報
        public int[] AVANT  = new int[2];
        public int[] MAPSET = new int[4];
        public int[] MYCAR  = new int[12];
        public int[] REDCAR = new int[12];
        public int[] ROCK   = new int[1];
        public int[] SMOKE  = new int[1];
        public int[] BANG   = new int[1];
        public int[] FLAG   = new int[3];
        public int[] FUEL   = new int[1];
        public int[] BGIMG  = new int[4];
        public int[] BLOCK  = new int[16];
        public int[] MYLEFT = new int[1];
        public int[] POINTS = new int[1];

        private string[] _GRAPH_F = {
            "assets/rally-dx.png",
            "assets/nyamcot.png",
            "assets/chars/MAP32x56-1.png",
            "assets/chars/MAP32x56-2.png",
            "assets/chars/MAP32x56-3.png",
            "assets/chars/MAP32x56-4.png",
            "assets/chars/MYCAR32-01.png",
            "assets/chars/MYCAR32-02.png",
            "assets/chars/MYCAR32-03.png",
            "assets/chars/MYCAR32-04.png",
            "assets/chars/MYCAR32-05.png",
            "assets/chars/MYCAR32-06.png",
            "assets/chars/MYCAR32-07.png",
            "assets/chars/MYCAR32-08.png",
            "assets/chars/MYCAR32-09.png",
            "assets/chars/MYCAR32-10.png",
            "assets/chars/MYCAR32-11.png",
            "assets/chars/MYCAR32-12.png",
            "assets/chars/REDCAR32-01.png",
            "assets/chars/REDCAR32-02.png",
            "assets/chars/REDCAR32-03.png",
            "assets/chars/REDCAR32-04.png",
            "assets/chars/REDCAR32-05.png",
            "assets/chars/REDCAR32-06.png",
            "assets/chars/REDCAR32-07.png",
            "assets/chars/REDCAR32-08.png",
            "assets/chars/REDCAR32-09.png",
            "assets/chars/REDCAR32-10.png",
            "assets/chars/REDCAR32-11.png",
            "assets/chars/REDCAR32-12.png",
            "assets/chars/ROCK48.png",
            "assets/chars/SMOKE48.png",
            "assets/chars/BANG48.png",
            "assets/chars/FLAG48.png",
            "assets/chars/FLAG48S.png",
            "assets/chars/FLAG48L.png",
            "assets/chars/FUEL.png",
            "assets/chars/BG-01.ico",
            "assets/chars/BG-02.ico",
            "assets/chars/BG-03.ico",
            "assets/chars/BG48-01.png",
            "assets/chars/BLOCK-01.png",
            "assets/chars/BLOCK-02.png",
            "assets/chars/BLOCK-03.png",
            "assets/chars/BLOCK-04.png",
            "assets/chars/BLOCK-05.png",
            "assets/chars/BLOCK-06.png",
            "assets/chars/BLOCK-07.png",
            "assets/chars/BLOCK-08.png",
            "assets/chars/BLOCK-09.png",
            "assets/chars/BLOCK-10.png",
            "assets/chars/BLOCK-11.png",
            "assets/chars/BLOCK-12.png",
            "assets/chars/BLOCK-13.png",
            "assets/chars/BLOCK-14.png",
            "assets/chars/BLOCK-15.png",
            "assets/chars/BLOCK-16.png",
            "assets/chars/MYCAR_LEFT24.png",
            "assets/chars/POINTS.png"
        };

        /// <summary>
        /// DxGraphクラスのインスタンスを取得。
        /// </summary>
        /// <returns>DxGraphクラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static DxGraph GetInstance() {

            if (singleton == null) {
                singleton = new DxGraph();
            }
            return singleton;
        }

        /// <summary>
        /// DxGraphクラスのコンストラクタ。
        /// </summary>
        private DxGraph() {

            // 存在チェック
            foreach (string graph in _GRAPH_F)
                if (!File.Exists(graph))
                    ExitApplication(string.Format(Settings.ERR_NO_GRAPH, graph));

            try {
                int i = 0;
                // AVANT
                AVANT[0] = DX.LoadGraph(_GRAPH_F[i++]);
                AVANT[1] = DX.LoadGraph(_GRAPH_F[i++]);
                // MAP
                MAPSET[0] = DX.LoadSoftImage(_GRAPH_F[i++]);
                MAPSET[1] = DX.LoadSoftImage(_GRAPH_F[i++]);
                MAPSET[2] = DX.LoadSoftImage(_GRAPH_F[i++]);
                MAPSET[3] = DX.LoadSoftImage(_GRAPH_F[i++]);
                // MYCAR
                MYCAR[0] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[1] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[2] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[3] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[4] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[5] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[6] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[7] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[8] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[9] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[10] = DX.LoadGraph(_GRAPH_F[i++]);
                MYCAR[11] = DX.LoadGraph(_GRAPH_F[i++]);
                // REDCAR
                REDCAR[0] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[1] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[2] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[3] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[4] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[5] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[6] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[7] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[8] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[9] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[10] = DX.LoadGraph(_GRAPH_F[i++]);
                REDCAR[11] = DX.LoadGraph(_GRAPH_F[i++]);
                // ROCK
                ROCK[0] = DX.LoadGraph(_GRAPH_F[i++]);
                // SMOKE
                SMOKE[0] = DX.LoadGraph(_GRAPH_F[i++]);
                // BANG
                BANG[0] = DX.LoadGraph(_GRAPH_F[i++]);
                // FLAG
                FLAG[0] = DX.LoadGraph(_GRAPH_F[i++]);
                FLAG[1] = DX.LoadGraph(_GRAPH_F[i++]);
                FLAG[2] = DX.LoadGraph(_GRAPH_F[i++]);
                // FUEL
                FUEL[0] = DX.LoadGraph(_GRAPH_F[i++]);
                // BACKGROUND
                BGIMG[0] = DX.LoadGraph(_GRAPH_F[i++]);
                BGIMG[1] = DX.LoadGraph(_GRAPH_F[i++]);
                BGIMG[2] = DX.LoadGraph(_GRAPH_F[i++]);
                BGIMG[3] = DX.LoadGraph(_GRAPH_F[i++]);
                // BLOCK
                BLOCK[0] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[1] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[2] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[3] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[4] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[5] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[6] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[7] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[8] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[9] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[10] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[11] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[12] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[13] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[14] = DX.LoadGraph(_GRAPH_F[i++]);
                BLOCK[15] = DX.LoadGraph(_GRAPH_F[i++]);
                //
                MYLEFT[0] = DX.LoadGraph(_GRAPH_F[i++]);
                //
                POINTS[0] = DX.LoadGraph(_GRAPH_F[i++]);
            }
            catch (Exception ex) {
                ExitApplication(string.Format(Settings.ERR_NO_READ_GRAPH, ex.Message));
            }
        }

        /// <summary>
        /// アプリケーションの強制終了。
        /// </summary>
        /// <param name="s">メッセージボックスに表示する文字列を指定する。</param>
        private void ExitApplication(string s) {

            MessageBox.Show(s);
            Environment.Exit(0x8020);
        }
    }

    /// <summary>
    /// DxFontsクラス。
    /// DxLibで使用するフォントをメモリーに読み込み、利用可能にする。
    /// </summary>
    public class DxFonts {

        // インスタンス
        private static DxFonts singleton = null;

  		public PrivateFontCollection FONT_COLLECTION = new PrivateFontCollection();

        // 使用するフォントファイルリスト
        private string[] _FONT_F = {
            "assets/font/PressStart2P.ttf"
        };

        // DxLibへ登録したフォント配列
        public int[] FONT = null;

		/// <summary>
		/// DxFontsクラスのインスタンスを取得。
		/// </summary>
        /// <returns>DxFontクラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static DxFonts GetInstance() {

            if (singleton == null) {
                singleton = new DxFonts();
            }
            return singleton;
        }

		/// <summary>
		/// DxFontsクラスのコンストラクタ。
		/// </summary>
		private DxFonts() {

            // 存在チェック
            foreach (string font in _FONT_F)
                if (!File.Exists(font))
                    ExitApplication(string.Format(Settings.ERR_NO_FONT, font));
            
            // ユーザーフォントの登録
            foreach (string font in _FONT_F)
                FONT_COLLECTION.AddFontFile(font);

            FONT = new int[_FONT_F.Length];

            // 14ポイントと28ポイントを登録
            // 使用したいサイズ、
            for (int i = 0; i < FONT_COLLECTION.Families.Length; i++)
                FONT[i] = DX.CreateFontToHandle(FONT_COLLECTION.Families[i].Name, 16, -1, -1);
        }

        /// <summary>
        /// アプリケーションの強制終了。
        /// </summary>
        /// <param name="s">メッセージボックスに表示する文字列を指定する。</param>
        private void ExitApplication(string s) {

            MessageBox.Show(s);
            Environment.Exit(0x8020);
        }
    }

    /// <summary>
    /// DxSoundクラス。
    /// DxLibで使用するサウンドをメモリーに読み込み、利用可能にする。
    /// </summary>
    public class DxSound {

        // インスタンス
        private static DxSound singleton = null;

		public static readonly int SND_GAMESTART_MUSIC               = 0;
		public static readonly int SND_SMOKESCREEN                   = 1;
		public static readonly int SND_FLAGGET                       = 2;
		public static readonly int SND_SPECIAL_FLAGGET               = 3;
		public static readonly int SND_ROUND_CLEAR_MUSIC             = 4;
		public static readonly int SND_IN_GAME_MUSIC                 = 5;
		public static readonly int SND_MY_CARCRASH                   = 6;
		public static readonly int SND_FUEL_BONUS_SOUND              = 7;
		public static readonly int SND_CHALLENGING_STAGE_START_MUSIC = 8;
		public static readonly int SND_CHALLENGING_STAGE_MUSIC       = 9;
        public static readonly int SND_CREDIT_SOUND                  = 10;
        public static readonly int SND_EXTEND_SOUND                  = 11;
        public static readonly int SND_HIGH_SCORE_MUSIC              = 12;
        public static readonly int SND_MY_CARRUN                     = 13;

        // 使用するフォントファイルリスト
        private string[] _SOUND_F = {
            "assets/sound/game-start-music.mp3",
            "assets/sound/carsmokescreen.mp3",
            "assets/sound/flagget.mp3",
            "assets/sound/special-flagget.mp3",
            "assets/sound/round-clear-music.mp3",
            "assets/sound/in-game-music.mp3",
            "assets/sound/my-carcrash.mp3",
            "assets/sound/fuel-bonus-sound.mp3",
            "assets/sound/challenging-stage-start-music.mp3",
            "assets/sound/challenging-stage-music.mp3",
            "assets/sound/credit-sound.mp3",
            "assets/sound/extend-sound.mp3",
            "assets/sound/high-score-music.mp3",
            "assets/sound/my-carrun.mp3"
        };
        
        // DxLibへ登録したサウンド配列
        public int[] SOUND = null;
        //
		public static readonly int PLAYTYPE_NORMAL = DX.DX_PLAYTYPE_NORMAL;
		public static readonly int PLAYTYPE_BACK   = DX.DX_PLAYTYPE_BACK;
		public static readonly int PLAYTYPE_LOOP   = DX.DX_PLAYTYPE_LOOP;

		/// <summary>
		/// DxSoundクラスのインスタンスを取得。
		/// </summary>
        /// <returns>DxSoundクラスのインスタンスを返却する。シングルトン構成です。</returns>      
        public static DxSound GetInstance() {

            if (singleton == null) {
                singleton = new DxSound();
            }
            return singleton;
        }

		/// <summary>
		/// DxSoundクラスのコンストラクタ。
		/// </summary>
        private DxSound() {

            // 存在チェック
            foreach (string sound in _SOUND_F)
                if (!File.Exists(sound))
                    ExitApplication(string.Format(Settings.ERR_NO_SOUND, sound));
            
            // サウンドの登録
            SOUND = new int[_SOUND_F.Length];

            for (int i = 0; i < _SOUND_F.Length; i++)
                SOUND[i] = DX.LoadSoundMem(_SOUND_F[i]);
        }

        /// <summary>
        /// アプリケーションの強制終了。
        /// </summary>
        /// <param name="s">メッセージボックスに表示する文字列を指定する。</param>
        private void ExitApplication(string s) {

            MessageBox.Show(s);
            Environment.Exit(0x8020);
        }
    }

    /// <summary>
    /// DxInputクラス。
    /// </summary>
	public class DxInput {
		
        // インスタンス
        private static DxInput singleton = null;

        //
        private static int[] _KEYSTATE = null;
        private int[] _PADLOG = null;

        /// <summary>
        /// DxInputのインスタンスを作成
        /// </summary>
        /// <returns>当クラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static DxInput GetInstance() {

            if (singleton == null) {
                singleton = new DxInput();
            }
            return singleton;
        }

		/// <summary>
		/// DxInputクラスのコンストラクタ。
        /// クラス全体で一度初期化すればよいものを設定する。
		/// </summary>
        private DxInput() {

            _KEYSTATE = new int[256];
            for (int i = 0; i < _KEYSTATE.Length; i++)
                _KEYSTATE[i] = 0;

            _PADLOG = new int[16];
            for (int i = 0; i < _PADLOG.Length; i++)
                _PADLOG[i] = 0;
        }

        /// <summary>
        /// 入力状態取得
        /// </summary>
        public void Update() {

            // キーの状態の取得
            DX.GetHitKeyStateAllEx(_KEYSTATE);

            // パッドの状態の取得
            for (int i = _PADLOG.Length - 1; i >= 1; i--) {
                _PADLOG[i] = _PADLOG[i - 1];
            }
            _PADLOG[0] = DX.GetJoypadInputState(DX.DX_INPUT_KEY_PAD1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //public int KeyState(int key) {
        //
        //    return _KEYSTATE[key];
        //}

        public bool IsKeyPress(int key) {

            return (_KEYSTATE[key] > 0);
        }

        public bool IsKeyTrigger(int key) {

            return (_KEYSTATE[key] == 1);
        }

        public bool IsKeyRelase(int key) {

            return (_KEYSTATE[key] < 0);
        }

        /// <summary>
        /// ボタンを押している間、ずっと反応
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsPadPress(int button) {
		    
            return Convert.ToBoolean(_PADLOG[0] & button);
	    }

        /// <summary>
        /// ボタンを押した瞬間だけ反応
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsPadTrigger(int button) {
        
            bool isNow = Convert.ToBoolean(_PADLOG[0] & button);	// 現在の状態
            bool isLast = Convert.ToBoolean(_PADLOG[1] & button);	// １フレーム前の状態
            return (isNow && !isLast);
        }

        /// <summary>
        /// ボタンを離した瞬間だけ反応
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        bool IsPadRelase(int button) {

            bool isNow = Convert.ToBoolean(_PADLOG[0] & button);    // 現在の状態
            bool isLast = Convert.ToBoolean(_PADLOG[1] & button);	// １フレーム前の状態
            return (!isNow && isLast);
        }
    }
}