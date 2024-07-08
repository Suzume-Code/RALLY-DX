using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    public class StageMap {

        // インスタンス
        private static StageMap singleton = null;

        private DxGraph _DXGRAPH = null;
        private DxSound _DXSOUND = null;

        public int[,] MAP = null;
        public int MAP_WIDTH = 0;
        public int MAP_HEIGHT = 0;
        public List<Block> BLOCKS = null;
        public List<PositionState> INIT_MYCAR = null;
        public List<Position> INIT_REDCAR = null;
        public List<CheckPointInfo> INIT_FLAG = null;
        public List<Position> INIT_ROCK = null;

        public int HISCORE = 0;
        public int SCORE = 0;
        public int TODAYS_HISCORE = 0;
        public int ROUND = 0;
        public int EXTRA_ROUND = 0;
        public int MYCAR_LEFT = 0;
        public int FUEL_LEFT = 0;
        public bool IS_GAME_OVER = false;

        private int _SAVE_FUEL_LEFT = 0;

        private int[] _MAPSET_NUMBER    = { 0, 0, 0, 1, 1, 1, 1, 2, 2, 2,  2, 3, 3, 3,  3 };
        private int[] _ROUND_OF_REDCARS = { 1, 2, 7, 3, 3, 4, 7, 4, 5, 5,  7, 6, 6, 6,  7 };
        private int[] _ROUND_OF_ROCKS   = { 1, 2, 5, 3, 4, 5, 7, 5, 5, 5, 10, 5, 5, 5, 10 };        
        private int[] _POSITION_OF_REDCARS = { 4, 3, 5, 2, 6, 0, 1 };

        public int SYSTIME = 0;

        /// <summary>
        /// StageMapクラスのインスタンスを取得。
        /// </summary>
        /// <returns>StageMapクラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static StageMap GetInstance() {

            if (singleton == null) {
                singleton = new StageMap();
            }
            return singleton;
        }

		/// <summary>
		/// StageMapクラスのコンストラクタ。
		/// </summary>
        private StageMap() {

            _DXGRAPH = DxGraph.GetInstance();
            _DXSOUND = DxSound.GetInstance();

            HISCORE = Settings.FIRST_EXTEND;
            SCORE = 0;
            TODAYS_HISCORE = HISCORE;
            ROUND = 0;
            EXTRA_ROUND = 0;
            MYCAR_LEFT = Settings.INIT_MYCAR;
            FUEL_LEFT = 0;
            IS_GAME_OVER = false;
        }

        /// <summary>
        /// ゲームを初期化する。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public void InitGame() {

            SCORE = 0;
            TODAYS_HISCORE = HISCORE;
            ROUND = 0;
            MYCAR_LEFT = Settings.INIT_MYCAR;
            ResetFuel(); 

            InitMap();
        }

        public void NextRound() {

            ResetFuel(); 

            InitMap();
        }

        public void ResetFuel() {

            FUEL_LEFT = ((122 + 1) * Settings.DX_FPS) - 1;
        }

        /// <summary>
        /// マップを初期化する。
        /// </summary>
        public void InitMap() {

            MAP = null;
            if (BLOCKS != null)
                BLOCKS.Clear();
            BLOCKS = new List<Block>();
            if (INIT_MYCAR != null)
                INIT_MYCAR.Clear();
            INIT_MYCAR = new List<PositionState>();
            if (INIT_REDCAR != null)
                INIT_REDCAR.Clear();
            INIT_REDCAR = new List<Position>();
            if (INIT_FLAG != null)
                INIT_FLAG.Clear();
            INIT_FLAG = new List<CheckPointInfo>();
            if (INIT_ROCK != null)
                INIT_ROCK.Clear();
            INIT_ROCK = new List<Position>();

            // ラウンド数カウントアップ
            ROUND++;
            if (IsChallengingStage())
                EXTRA_ROUND++;

            // マップセット番号を取得
            int mapset = MapsetNumberOfRound();

            // マップデータの読み込み
            int img_width, img_height;
	        DX.GetSoftImageSize(_DXGRAPH.MAPSET[mapset], out img_width, out img_height);

            // イメージ＋パディング（６マス）をマップサイズとする
            int map_width = img_width + Settings.MAP_OFFSET + Settings.MAP_OFFSET;
            int map_height = img_height + Settings.MAP_OFFSET + Settings.MAP_OFFSET;

            MAP = new int[map_height, map_width];
            MAP_WIDTH = map_width;
            MAP_HEIGHT = map_height;

            // パディングエリアをイメージで埋める
            for (int y = 0; y < map_height; y++) {
                for (int x = 0; x < map_width; x++) {
                    if (x >= Settings.MAP_OFFSET && x <= (map_width - Settings.MAP_OFFSET - 1))
                        if (y >= Settings.MAP_OFFSET && y <= (map_height - Settings.MAP_OFFSET - 1))
                            continue;
                    MAP[y, x] = _DXGRAPH.BGIMG[3];
                }
            }

            // マップイメージから情報を取得
            int r, g, b, a;
            for (int y = 0; y < img_height; y++) {
                for (int x = 0; x < img_width; x++) {

                    // マップイメージからピクセル情報を取得
                    DX.GetPixelSoftImage(_DXGRAPH.MAPSET[mapset], x, y, out r, out g, out b, out a);

                    int map_x = x + Settings.MAP_OFFSET;
                    int map_y = y + Settings.MAP_OFFSET;

                    // 透明
                    if (a == 0) {
                        MAP[map_y, map_x] = _DXGRAPH.BLOCK[0];
                        BLOCKS.Add(new Block(map_x, map_y));
                        continue;
                    }

                    Color color = Color.FromArgb(r, g, b);
                    // MYCAR (Blue)
                    if (r == 0 && g == 0 && b == 255) {
                        INIT_MYCAR.Add(new PositionState(_DXGRAPH.MYCAR[0], map_x, map_y));
                        continue;
                    }
                    // REDCAR (Red)
                    if (r == 255 && g == 0 && b == 0) {
                        INIT_REDCAR.Add(new Position(map_x, map_y));
                        continue;
                    }
                    // Yellow
                    if (r == 255 && g == 255 && b == 0) {
                        INIT_FLAG.Add(new CheckPointInfo(_DXGRAPH.FLAG[0], map_x, map_y));
                        continue;
                    }
                    // Pink
                    if (r == 255 && g == 0 && b == 255) {
                        INIT_ROCK.Add(new Position(map_x, map_y));
                        continue;
                    }
                }
            }

            // ブロック
            bool[] blk01 = { true, true, true, true };      // 全部空き
            bool[] blk02 = { false, true, true, true };     // 上空き
            bool[] blk03 = { false, false, true, true };    // 上左空き
            bool[] blk04 = { false, false, false, true };   // 上左下空き
            bool[] blk05 = { false, false, false, false };  // 上左下右空き
            bool[] blk06 = { true, false, true, true };     // 左空き
            bool[] blk07 = { true, false, false, true };    // 左下空き
            bool[] blk08 = { true, false, true, false };    // 左右空き
            bool[] blk09 = { true, false, false, false };   // 左下右空き
            bool[] blk10 = { true, true, false, true };     // 下空き
            bool[] blk11 = { true, true, false, false };    // 下右空き
            bool[] blk12 = { true, true, true, false };     // 右空き
            bool[] blk13 = { false, false, true, false };   // 上左右空き
            bool[] blk14 = { false, true, false, false };   // 上下右空き
            bool[] blk15 = { false, true, false, true };    // 上下空き
            bool[] blk16 = { false, true, true, false };    // 上右空き
            bool[] myblk = new bool[4];
            foreach (Block block in BLOCKS) {
                int mx = block.MX;
                int my = block.MY;
                myblk[0] = BLOCKS.Any(blk => (blk.MX == mx && blk.MY == my - 1));   // 上側
                myblk[1] = BLOCKS.Any(blk => (blk.MX == mx - 1 && blk.MY == my));   // 左側
                myblk[2] = BLOCKS.Any(blk => (blk.MX == mx && blk.MY == my + 1));   // 下側
                myblk[3] = BLOCKS.Any(blk => (blk.MX == mx + 1 && blk.MY == my));   // 右側
                if (mx == Settings.MAP_OFFSET)
                    myblk[1] = true;
                if (mx == MAP_WIDTH - Settings.MAP_OFFSET - 1)
                    myblk[3] = true;
                if (my == Settings.MAP_OFFSET)
                    myblk[0] = true;
                if (my == MAP_HEIGHT - Settings.MAP_OFFSET - 1)
                    myblk[2] = true;

                if (Enumerable.SequenceEqual(blk01, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[5];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk02, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[12];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk03, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[6];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk04, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[3];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk05, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[0];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk06, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[15];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk07, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[9];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk08, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[11];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk09, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[2];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk10, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[14];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk11, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[8];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk12, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[13];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk13, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[1];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk14, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[4];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk15, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[10];
                    continue;
                }
                if (Enumerable.SequenceEqual(blk16, myblk)) {
                    MAP[my, mx] = _DXGRAPH.BLOCK[7];
                    continue;
                }
            }

            string mi_01 = "MAP INFO :";
            string mi_07 = "MAP #    = {0}";
            string mi_08 = "  WIDTH  = {0}";
            string mi_09 = "  HEIGHT = {0}";
            string mi_02 = "BLOCKS   = {0}";
            string mi_03 = "FLAGS    = {0}";
            string mi_04 = "ROCKS    = {0}";
            string mi_05 = "MY CAR   = {0}";
            string mi_06 = "RED CARS = {0}";
            Console.WriteLine(mi_01);
            Console.WriteLine(string.Format(mi_07, mapset.ToString()));
            Console.WriteLine(string.Format(mi_08, MAP_WIDTH.ToString()));
            Console.WriteLine(string.Format(mi_09, MAP_HEIGHT.ToString()));
            Console.WriteLine(string.Format(mi_02, BLOCKS.Count.ToString()));
            Console.WriteLine(string.Format(mi_03, INIT_FLAG.Count.ToString()));
            Console.WriteLine(string.Format(mi_04, INIT_ROCK.Count.ToString()));
            Console.WriteLine(string.Format(mi_05, INIT_MYCAR.Count.ToString()));
            Console.WriteLine(string.Format(mi_06, INIT_REDCAR.Count.ToString()));
        }

        /// <summary>
        /// チェックポイントフラグを通過したときの点数加算。
        /// </summary>
        /// <param name="cleared"></param>
        /// <param name="special_flag_got"></param>
        public void AddScore(int cleared, bool special_flag_got) {

            int save_score = SCORE;

            int special = (special_flag_got) ? 2 : 1;
            SCORE += cleared * 100 * special;
            if (SCORE > HISCORE)
                HISCORE = SCORE;

            // マイカーエクステンドボーナス
            if ((save_score < Settings.FIRST_EXTEND && SCORE >= Settings.FIRST_EXTEND) ||
                (save_score < Settings.SECOND_EXTEND && SCORE >= Settings.SECOND_EXTEND)) {
                DX.PlaySoundMem(_DXSOUND.SOUND[DxSound.SND_EXTEND_SOUND], DX.DX_PLAYTYPE_BACK, DX.TRUE);
                MYCAR_LEFT++;
            }
        }

        /// <summary>
        /// マップセットの番号を取得する。
        /// </summary>
        /// <returns>ラウンド数に応じたマップ番号を返却する。</returns>
        public int MapsetNumberOfRound() {

            int offset = (ROUND - 1) % _MAPSET_NUMBER.Length;
            return _MAPSET_NUMBER[offset];
        }

        /// <summary>
        /// ラウンド毎のレッドカー（敵車）の数を取り出す。
        /// </summary>
        /// <returns>レッドカー（敵車）の数を返却する。</returns>
        public int RedCarsOfRound() {

            return (_ROUND_OF_REDCARS.Length < ROUND) ? 7 : _ROUND_OF_REDCARS[ROUND - 1];
        }

        /// <summary>
        /// ラウンド毎の岩の数を取り出す。
        /// </summary>
        /// <returns>岩の数を返却する。</returns>
        public int RocksOfRound() {

            return (_ROUND_OF_ROCKS.Length < ROUND) ? 10 : _ROUND_OF_ROCKS[ROUND - 1];
        }

        /// <summary>
        /// フレーム単位で燃料を消費する。
        /// </summary>
        public void UseFuel(int used_fuel = 1) {

            FUEL_LEFT -= used_fuel;
            if (FUEL_LEFT < 0)
                FUEL_LEFT = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool BonusFuel() {

            const int value = 40;

            if (FUEL_LEFT <= 0)
                return true;

            int fuel = (FUEL_LEFT > value) ? value : FUEL_LEFT;
            FUEL_LEFT -= fuel;

            int save_score = SCORE;

            SCORE += 10;
            if (SCORE > HISCORE)
                HISCORE = SCORE;

            // マイカーエクステンドボーナス
            if ((save_score < Settings.FIRST_EXTEND && SCORE >= Settings.FIRST_EXTEND) ||
                (save_score < Settings.SECOND_EXTEND && SCORE >= Settings.SECOND_EXTEND)) {
                DX.PlaySoundMem(_DXSOUND.SOUND[DxSound.SND_EXTEND_SOUND], DX.DX_PLAYTYPE_BACK, DX.TRUE);
                MYCAR_LEFT++;
            }

            return (FUEL_LEFT <= 0) ? true : false;
        }

        /// <summary>
        /// ラッキーフラグ用に燃料を保存する。
        /// </summary>
        public void SaveFuel() {

            _SAVE_FUEL_LEFT = FUEL_LEFT;
        }

        /// <summary>
        /// ラッキーフラグ用に燃料をリストアする。
        /// </summary>
        public void RestoreFuel() {

            FUEL_LEFT = _SAVE_FUEL_LEFT;
        }

        public int Position(int n) {

            return _POSITION_OF_REDCARS[n];
        }

        public bool IsChallengingStage() {

            // *DEBUG*
            //if (ROUND == 2)
            //    return true;
            //return true;
            if (ROUND == 3)
                return true;
            if (ROUND > 3)
                return ((ROUND + 1) % 4 == 0);
            return false;
        }

        public bool IsNextChallengingStage() {

            // *DEBUG*
            //if (true)
            //    return true;
            //return true;
            if ((ROUND + 1) == 3)
                return true;
            if ((ROUND + 1) > 3)
                return ((ROUND + 1 + 1) % 4 == 0);
            return false;
        }
    }


	public class Block {

		public Block(int x, int y) {

			MX = x;
			MY = y;
		}
		public int MX { get; set; }
		public int MY { get; set; }
	}

	/// <summary>
	/// ゲームオブジェクトの設定値保持クラス。
	/// </summary>
    public class PositionState {

		/**
			int	X,		-- X軸座標
			int	Y,		-- Y軸座標
			int GrHandle	-- Bitmap番号

		*/
        public PositionState(int gr, int x, int y) {

			GrHandle = gr;
            X = x;
            Y = y;
			int width, height;
			DX.GetGraphSize(gr, out width, out height);
			Width = width;
			Height = height;
			OffsetX = Settings.CELL_SIZE - width / 2;
			OffsetY = Settings.CELL_SIZE - height / 2;
        }
		public int GrHandle { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int OffsetX { get; set; }
		public int OffsetY { get; set; }
    }

    public class Position {

        public Position(int mx, int my) {

            MX = mx;
            MY = my;
        }
        public int MX { get; set; }
        public int MY { get; set; }
    }

}

/**
 ROUND コース名    レッドカー数 
   1   初級          1
   2   初級          2
   3   初級          7     (チャレンジングステージ)
   4   中級          3
   5   中級          3
   6   中級          4
   7   中級          7     (チャレンジングステージ)
   8   上級          4
   9   上級          5
  10   上級          5
  11   上級          7     (チャレンジングステージ)
  12   エキスパート   6
  13   エキスパート   6
  14   エキスパート   6
  15   エキスパート   7     (チャレンジングステージ)
  16
  17                 6

ＲＯＵＮＤ１５以降は１からのループですが、レッドカーと岩が増え、ゲーム全体のスピードも早くなっています。

 */
