using System;
using System.Collections.Generic;
using DxLibDLL;

namespace RALLYDX {

    public abstract class GameBase {

        protected DxGraph DXGRAPH = null;
        protected DxFonts DXFONTS = null;
        protected DxSound DXSOUND = null;
        protected DxInput DXINPUT = null;

        protected StageMap STAGEMAP = null;

        protected MyCar MYCAR = null;
        protected SmokeScreen SMOKE = null;
        protected CheckPoint CHECKPOINT = null;
        protected Rocks ROCKS = null;
        protected List<RedCar> REDCAR = null;

        protected State STATE = State.GameStart;
        protected enum State {
            GameStart,
            Gingle,
            Play,
            Lucky,
            Crush,
            StageClear,
            Continue,
            Bonus,
            NextRound
        }

        protected int STAGE_START_MUSIC = 0;
        protected int STAGE_IN_MUSIC    = 0;
        protected int STAGE_CAR_CRUSH   = 0;

		/// <summary>
		/// GameBaseクラスのコンストラクタ。
        /// クラス全体で一度初期化すればよいものを設定する。
		/// </summary>
        public GameBase() {

            DXGRAPH = DxGraph.GetInstance();
            DXFONTS = DxFonts.GetInstance();
            DXSOUND = DxSound.GetInstance();
            DXINPUT = DxInput.GetInstance();

            STAGEMAP = StageMap.GetInstance();
        }

        /// <summary>
        /// ゲームを初期化する。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public virtual void Init() {
        }

        /// <summary>
        /// コンティニューのための初期化をする。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public virtual void Continue() {
        }

        /// <summary>
        /// 次ステージのための初期化をする。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public virtual void ReInit() {
        }

        /// <summary>
        /// スクリーンへ描画する。
        /// </summary>
        /// <param name="gamestate"></param>
        /// <returns>最新のGameStateを返却する。</returns>
        public virtual GameState DrawScreen(GameState gamestate) {

            return gamestate;
        }

        /// <summary>
        /// ステージに各アイテムを描画
        /// </summary>
        protected virtual void DrawContent() {
        }

        /// <summary>
        /// スコアパネルの描画
        /// </summary>
        protected virtual void DrawScorePanel() {

            // スコアパネル
            DX.DrawBox(480, 0, 641, 481, Settings.Black, DX.TRUE);

            // ハイスコア、スコア、ラウンドの描画
            string sc0 = "HI-SCORE";
            string sc1 = STAGEMAP.HISCORE.ToString().PadLeft(8, ' ');
            string sc2 = "1UP     ";
            string sc3 = STAGEMAP.SCORE.ToString().PadLeft(8, ' ');
            string sc4 = "ROUND " + STAGEMAP.ROUND.ToString();
            DrawString(32, 01, sc0, Settings.White);
            DrawString(32, 02, sc1, Settings.Red);
            DrawString(32, 03, sc2, Settings.White);
            DrawString(32, 04, sc3, Settings.SkyBlue);
            if (STAGEMAP.ROUND > 0)
                DrawString(32, 29, sc4, Settings.White);

            if (MYCAR == null)
                return;

            // 燃料計
            DrawImage(32, 08, DXGRAPH.FUEL[0]);
            int w = 122 - (STAGEMAP.FUEL_LEFT / 60) + 1;
            uint base_color = Settings.Yellow;
            if (w >= 122) {
                Settings.MYCAR_MOVE = 1;
            } else if (w >= 98) {
                base_color = Settings.Red;
                Settings.MYCAR_MOVE = 2;
            }
            DX.DrawBox(496, 132, 618, 147, base_color, DX.TRUE);
            DX.DrawBox(496, 132, (496 + w), 147, Settings.Black, DX.TRUE);

            // レーダーエリア
            const int map_x = 496;
            const int map_y = 172;
            const int map_lx = map_x + 128;
            const int map_ly = map_y + 224;

            DX.DrawBox(map_x, map_y, map_lx, map_ly, Settings.Navy, DX.TRUE);

            // マイカー
            int mx = MYCAR.MX - Settings.MAP_OFFSET;
            int my = MYCAR.MY - Settings.MAP_OFFSET;
            int mycar_mx = map_x + (mx * 4);
            int mycar_my = map_y + (my * 4);
            DX.DrawBox(mycar_mx, mycar_my, mycar_mx + 5, mycar_my + 5, Settings.White, DX.TRUE);
            
            // チェックポイント
            foreach (CheckPointInfo cp in CHECKPOINT.FLAG) {
                int cp_mx = map_x + ((cp.MX - Settings.MAP_OFFSET) * 4);
                int cp_my = map_y + ((cp.MY - Settings.MAP_OFFSET) * 4);
                if (!cp.Cleared) {
                    uint c = Settings.Yellow;
                    if (cp.IsSpecial && STAGEMAP.SYSTIME % 30 > 15)
                        c = Settings.Black;      
                    DX.DrawBox(cp_mx, cp_my, cp_mx + 5, cp_my + 5, c, DX.TRUE);
                }
                    
            }
            
            // レッドカー
            foreach (RedCar r in REDCAR) {
                mx = r.MX - Settings.MAP_OFFSET;
                my = r.MY - Settings.MAP_OFFSET;
                int redcar_mx = map_x + (mx * 4);
                int redcar_my = map_y + (my * 4);
                DX.DrawBox(redcar_mx, redcar_my, redcar_mx + 5, redcar_my + 5, Settings.Red, DX.TRUE);
            }

            // マイカー残機
            for (int i = 0; i < STAGEMAP.MYCAR_LEFT; i++)
                DrawImage(32 + (i * 2), 26, DXGRAPH.MYLEFT[0]);
        }

        /// <summary>
        /// 文字列を描画する。
        /// </summary>
        /// <param name="x">X軸カラム位置を指定する。</param>
        /// <param name="y">Y軸カラム位置を指定する。</param>
        /// <param name="s">描画する文字列を指定する。</param>
        /// <param name="c">文字色を指定する。</param>
        protected virtual void DrawString(int x, int y, string s, uint c, int font = 0) {

            int nx = (x - 1) * 16;
            int ny = (y - 1) * 16 + 2;
            DX.DrawStringToHandle(nx, ny, s, c, DXFONTS.FONT[font]);
		}

        /// <summary>
        /// ビットマップを描画する。起点は左上。
        /// </summary>
        /// <param name="x">X軸カラム位置を指定する。</param>
        /// <param name="y">Y軸カラム位置を指定する。</param>
        /// <param name="b">DxLibへ登録したビットマップ番号を指定する。</param>
		protected virtual void DrawImage(int x, int y, int b) {

            int nx = (x - 1) * 16;
            int ny = (y - 1) * 16;
			DX.DrawGraph(nx, ny, b, DX.TRUE);
		}

        /// <summary>
        /// ビットマップを描画する。起点は左上。
        /// </summary>
        /// <param name="x">X軸ピクセル位置を指定する。</param>
        /// <param name="y">Y軸ピクセル位置を指定する。</param>
        /// <param name="b">DxLibへ登録したビットマップ番号を指定する。</param>
        protected virtual void DrawPixelImage(int x, int y, int b) {
        
            DX.DrawGraph(x, y, b, DX.TRUE);
        }

        /// <summary>
        /// サウンドを演奏する。
        /// </summary>
        /// <param name="sound">サウンド番号</param>
        /// <param name="PlayType"></param>
        protected virtual void PlaySound(int sound, int PlayType) {

            DX.PlaySoundMem(DXSOUND.SOUND[sound], PlayType, DX.TRUE);
        }

        /// <summary>
        /// 演奏中のサウンドを停止する。
        /// </summary>
        /// <param name="sound">サウンド番号</param>
        protected virtual void StopSound(int sound) {

            DX.StopSoundMem(DXSOUND.SOUND[sound]);
        }

        /// <summary>
        /// 演奏中かどうかを判定する。
        /// </summary>
        /// <param name="sound">サウンド番号</param>
        /// <returns>演奏中はtrueを返却する。停止中はfalseを返却する。</returns>
        protected virtual bool CheckPlaySound(int sound) {

            return Convert.ToBoolean(DX.CheckSoundMem(DXSOUND.SOUND[sound]));
        }
    }
}