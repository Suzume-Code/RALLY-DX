using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// CheckPointクラス。
    /// </summary>
	public class CheckPoint {
		
        // インスタンス
        private static CheckPoint singleton = null;

        private DxGraph _DXGRAPH = null;
        private DxSound _DXSOUND = null;
        
        private StageMap _STAGEMAP = null;

        private Random _RANDOM = new Random();

        public List<CheckPointInfo> FLAG = null;
        public List<CheckPointScore> FLAG_SCORE = null;

        public int _CLEARED_FLAG = 0;
        private bool _GOT_SPECIAL_FLAG = false;

        /// <summary>
        /// CheckPointのインスタンスを作成
        /// </summary>
        /// <returns>当クラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static CheckPoint GetInstance() {

            if (singleton == null) {
                singleton = new CheckPoint();
            }
            return singleton;
        }

		/// <summary>
		/// CheckPointクラスのコンストラクタ。
		/// </summary>
        private CheckPoint() {

            _DXGRAPH = DxGraph.GetInstance();
            _DXSOUND = DxSound.GetInstance();

            _STAGEMAP = StageMap.GetInstance();
        }

        public void InitGame() {

            // チェックポイントの設定
            int allCount = _STAGEMAP.INIT_FLAG.Count;
            // チェックポイントによるエラーを防ぐため
            if (allCount < 10)
                ExitApplication(string.Format(Settings.ERR_NO_CHKPOINT, allCount.ToString()));

            List<int> vs = new List<int>();
            while(vs.Count < 10) {
                int nbr = _RANDOM.Next(0, allCount);
                if (!vs.Any(x => x == nbr))
                    vs.Add(nbr);
            }
            FLAG = new List<CheckPointInfo>();
            foreach(int i in vs)
                FLAG.Add(_STAGEMAP.INIT_FLAG[i]);
            
            FLAG[8].IsSpecial = true;
            FLAG[8].GrHandle = _DXGRAPH.FLAG[1];
            FLAG[9].IsLucky = true;
            FLAG[9].GrHandle = _DXGRAPH.FLAG[2];

            FLAG_SCORE = new List<CheckPointScore>();

            _CLEARED_FLAG = 0;
            _GOT_SPECIAL_FLAG = false;
        }

        /// <summary>
        /// ステージ内コンティニュー
        /// </summary>
        public void ContinueGame() {

            FLAG_SCORE = new List<CheckPointScore>();

            // ステージ内でクリアしたチェックポイント数の初期化
            _CLEARED_FLAG = 0;
            _GOT_SPECIAL_FLAG = false;
        }

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="MX"></param>
        /// <param name="MY"></param>
        /// <param name="MOD_X"></param>
        /// <param name="MOD_Y"></param>
        /// <returns>ラッキーチェックポイントの場合trueを返却する。以外は、falseを返却する。</returns>
        public bool JudgeConflict(int mx, int my, int mod_x, int mod_y) {

            // マイカーの位置を計算
            int mycar_x = mx * Settings.CELL_SIZE + mod_x;
            int mycar_y = my * Settings.CELL_SIZE + mod_y;

			// 表示されているチェックポイントを検索対象にする。
			foreach (CheckPointInfo check in FLAG.Where(chk => (!chk.Cleared))) {
                if (check.MX == mx || check.MY == my) {
                    int x = Math.Abs((check.MX * Settings.CELL_SIZE) - mycar_x);
                    int y = Math.Abs((check.MY * Settings.CELL_SIZE) - mycar_y);
                    if (x <= 16 && y <= 16) {
    					check.Cleared = true;
                        int sound = (check.IsSpecial) ? DxSound.SND_SPECIAL_FLAGGET : DxSound.SND_FLAGGET;
                        _GOT_SPECIAL_FLAG = (check.IsSpecial) ? check.IsSpecial : _GOT_SPECIAL_FLAG;

                        DX.PlaySoundMem(_DXSOUND.SOUND[sound], DX.DX_PLAYTYPE_BACK, DX.TRUE);
                        _CLEARED_FLAG++;
                        _STAGEMAP.AddScore(_CLEARED_FLAG, _GOT_SPECIAL_FLAG);
                        //
                        FLAG_SCORE.Add(new CheckPointScore(_DXGRAPH.POINTS[0], mx, my, _CLEARED_FLAG, _GOT_SPECIAL_FLAG));
                        return check.IsLucky;
                    }
                }
				//if (check.MX == mx && check.MY == my && mod_x == 0 && mod_y == 0) {
                    //
				//	check.Cleared = true;

                //    int sound = (check.IsSpecial) ? DxSound.SND_SPECIAL_FLAGGET : DxSound.SND_FLAGGET;
                //    _GOT_SPECIAL_FLAG = (check.IsSpecial) ? check.IsSpecial : _GOT_SPECIAL_FLAG;

				//	DX.PlaySoundMem(_DXSOUND.SOUND[sound], DX.DX_PLAYTYPE_BACK, DX.TRUE);
                //    _CLEARED_FLAG++;
				//	_STAGEMAP.AddScore(_CLEARED_FLAG, _GOT_SPECIAL_FLAG);
                    //
                //    FLAG_SCORE.Add(new CheckPointScore(_DXGRAPH.POINTS[0], MX, MY, _CLEARED_FLAG, _GOT_SPECIAL_FLAG));
				//	return check.IsLucky;
				//}
			}
            return false;
        }

        /// <summary>
        /// チェックポイント通過時の点数を表示
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="my"></param>
        /// <param name="plot_x"></param>
        /// <param name="plot_y"></param>
        public void DrawScore(int mx, int my, int plot_x, int plot_y) {

            foreach(CheckPointScore score in FLAG_SCORE)
                if (mx == score.MX && my == score.MY)
                    DX.DrawRectGraph(plot_x, plot_y + 16, score.X, score.Y, score.Width, score.Height, score.GrHandle, DX.TRUE, DX.FALSE) ;
        }

        /// <summary>
        /// 当クラスのクリーンアップ
        /// </summary>
        public void CleanUp() {

            // 点数表示
            for (int i = 0; i < FLAG_SCORE.Count; i++) {
                FLAG_SCORE[i].Live--;
                if (FLAG_SCORE[i].Live == 0)
                    FLAG_SCORE.RemoveAt(i);
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

    public class CheckPointInfo {

        public CheckPointInfo(int gr, int mx, int my) {

            GrHandle = gr;
            MX = mx;
            MY = my;
            Cleared = false;
            IsSpecial = false;
            IsLucky = false;
        }
        public int GrHandle { get; set; }
		public int MX { get; set; }
		public int MY { get; set; }
        public bool Cleared { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsLucky { get; set; }
    }

    /// <summary>
    /// 通過したチェックポイントの点数を表示する。
    /// </summary>
    public class CheckPointScore {

        public CheckPointScore(int gr, int mx, int my, int cl, bool sp) {

            GrHandle = gr;
            X = 0;
            Y = (cl - 1) * 16;
            Width = (sp) ? 64 : 40;
            Height = 16;
            MX = mx;
            MY = my;
            Live = 90;      // 約1.5秒
        }
        public int GrHandle { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MX { get; set; }
        public int MY { get; set; }
        public int Live { get; set; }
    }
}