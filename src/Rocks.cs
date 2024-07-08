using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// Rocksクラス。
    /// </summary>
	public class Rocks {
		
        // インスタンス
        private static Rocks singleton = null;

        private DxGraph _DXGRAPH = null;
        private DxSound _DXSOUND = null;
        private StageMap _STAGEMAP = null;

        private Random _RANDOM = null;

        public List<PositionRock> ROCKS = null;

        /// <summary>
        /// Rocksのインスタンスを作成
        /// </summary>
        /// <returns>当クラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static Rocks GetInstance() {

            if (singleton == null) {
                singleton = new Rocks();
            }
            return singleton;
        }

		/// <summary>
		/// Rocksクラスのコンストラクタ。
		/// </summary>
        private Rocks() {

            _DXGRAPH = DxGraph.GetInstance();
            _DXSOUND = DxSound.GetInstance();
            
            _STAGEMAP = StageMap.GetInstance();

            _RANDOM = new Random();
        }

        public void InitGame() {

            // チェックポイントの設定
            int allCount = _STAGEMAP.INIT_ROCK.Count;
            if (allCount == 0)
                ExitApplication(string.Format(Settings.ERR_NO_ROCKS, allCount.ToString()));

            int max_rocks = _STAGEMAP.RocksOfRound();
            int max = (allCount < max_rocks) ? allCount : max_rocks;

            List<int> vs = new List<int>();
            while (vs.Count < max) {
                int nbr = _RANDOM.Next(0, allCount);
                if (!vs.Any(x => x == nbr))
                    vs.Add(nbr);
            }
            ROCKS = new List<PositionRock>();
            foreach (int i in vs)
                ROCKS.Add(new PositionRock(_DXGRAPH.ROCK[0], _STAGEMAP.INIT_ROCK[i].MX, _STAGEMAP.INIT_ROCK[i].MY));
        }

        /// <summary>
        /// 岩とマイカーの衝突を変亭する。
        /// </summary>
        /// <param name="mx">マイカーのマップX座標</param>
        /// <param name="my">マイカーのマップY座標</param>
        /// <param name="mod_x"></param>
        /// <param name="mod_y"></param>
        /// <returns>衝突した場合にtrueを返却する。</returns>
        public bool JudgeConflict(int mx, int my, int mod_x, int mod_y) {

            // マイカーの位置を計算
            int mycar_x = mx * Settings.CELL_SIZE + mod_x;
            int mycar_y = my * Settings.CELL_SIZE + mod_y;

			// 表示されているチェックポイントを検索対象にする。
			foreach (PositionRock rock in ROCKS) {
                if (rock.MX == mx || rock.MY == my) {
                    int x = Math.Abs((rock.MX * Settings.CELL_SIZE) - mycar_x);
                    int y = Math.Abs((rock.MY * Settings.CELL_SIZE) - mycar_y);
                    if (x <= 16 && y <= 16)
                        return true;
                }
				//if (rock.MX == mx && rock.MY == my && mod_x == 0 && mod_y == 0)
				//	return true;
			}
            return false;
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

    public class PositionRock {

        public PositionRock(int gr, int mx, int my) {

			GrHandle = gr;
            MX = mx;
            MY = my;
        }
		public int GrHandle { get; set; }
        public int MX { get; set; }
        public int MY { get; set; }
    }
}