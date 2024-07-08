using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// GameTitleクラス。
    /// </summary>
	public class SmokeScreen {
		
        // インスタンス
        private static SmokeScreen singleton = null;

        private DxGraph _DXGRAPH = null;
        private DxSound _DXSOUND = null;
 
        private StageMap _STAGEMAP = null;

        public List<Smoke> SMOKESCREEN = null; 
        private Queue<Smoke> SMOKEQUEUE = null; 

        private int _SMOKE_INTERVAL = 0;
        private int _SMOKE_LEFT = 0;
        private const int _SMOKE_INTERVAL_TIME = 12;

        /// <summary>
        /// SmokeScreenのインスタンスを作成
        /// </summary>
        /// <returns>当クラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static SmokeScreen GetInstance() {

            if (singleton == null) {
                singleton = new SmokeScreen();
            }
            return singleton;
        }

		/// <summary>
		/// SmokeScreenクラスのコンストラクタ。
		/// </summary>
        private SmokeScreen() {

            _DXGRAPH = DxGraph.GetInstance();
            _DXSOUND = DxSound.GetInstance();
            
            _STAGEMAP = StageMap.GetInstance();

            _SMOKE_INTERVAL = 0;

            InitGame();
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitGame() {

            SMOKESCREEN = new List<Smoke>();
            SMOKEQUEUE = new Queue<Smoke>();

            _SMOKE_INTERVAL = 0;
            _SMOKE_LEFT = 0;
        }

        /// <summary>
        /// ステージ内コンティニュー
        /// </summary>
        public void ContinueGame() {

            InitGame();
        }

        /// <summary>
        /// スモークスクリーンを発生させる。
        /// </summary>
        /// <param name="x">X軸発生位置</param>
        /// <param name="y">Y軸発生位置</param>
		public void AddSmokeScreen(int x, int y, Direction move) {

            if (SMOKEQUEUE.Count > 0)
                return;
		    if (--_SMOKE_INTERVAL > 0)
                return;
            if (_STAGEMAP.FUEL_LEFT < 120)
                return;

            Add(x, y, move);
            DX.PlaySoundMem(_DXSOUND.SOUND[DxSound.SND_SMOKESCREEN], DX.DX_PLAYTYPE_BACK, DX.TRUE);
            _STAGEMAP.UseFuel(120);

            _SMOKE_LEFT = 2;
            _SMOKE_INTERVAL = _SMOKE_INTERVAL_TIME;
		}

		public void DelaySmokeScreen(int x, int y, Direction move) {

            if (SMOKEQUEUE.Count > 0)
                return;
            if (_SMOKE_LEFT == 0)
                return;

		    if (--_SMOKE_INTERVAL > 0)
                return;

            _SMOKE_LEFT--;
            SMOKESCREEN.Add(new Smoke(_DXGRAPH.SMOKE[0], x, y));
            DX.PlaySoundMem(_DXSOUND.SOUND[DxSound.SND_SMOKESCREEN], DX.DX_PLAYTYPE_BACK, DX.TRUE);

            if (_SMOKE_LEFT > 0)
                _SMOKE_INTERVAL = _SMOKE_INTERVAL_TIME;
		}

        /// <summary>
        /// 当クラスのクリーンアップ
        /// </summary>
        public void CleanUp() {
        
			// スモークスクリーン
            for (int i = 0; i < SMOKESCREEN.Count; i++)
                if (--SMOKESCREEN[i].Live == 0)
                    SMOKESCREEN.RemoveAt(i);
        }

        private void Add(int x, int y, Direction move) {

            if (move == Direction.ToUp)
                y += 40;
            else if (move == Direction.ToDown)
                y += 8;
            else if (move == Direction.ToLeft)
                x += 40;
            else if (move == Direction.ToRight)
                x += 8;

            SMOKESCREEN.Add(new Smoke(_DXGRAPH.SMOKE[0], x, y));
        }
    }

	public class Smoke {

		public Smoke(int gr, int x, int y) {

            GrHandle = gr;
			X = x;
			Y = y;
            MX = x / Settings.CELL_SIZE;
            MY = y / Settings.CELL_SIZE;
            MOD_X = x % Settings.CELL_SIZE;
            MOD_Y = y % Settings.CELL_SIZE;
			Live = 180;
		}
        public int GrHandle { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int MX { get; set; }
		public int MY { get; set; }
		public int MOD_X { get; set; }
		public int MOD_Y { get; set; }
		public int Live { get; set; }
	}
}