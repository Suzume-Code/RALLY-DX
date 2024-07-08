using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// GameChampクラス。
    /// </summary>
	public class GameChamp : GameBase {
		
        //
        private int _WAIT_TIME = 0;

        private readonly string[] _DIDIT_01 = {
            "        YOU DID IT !!         ",
            "       THE HIGH SCORE         ",
            "          OF THE DAY.         ",
            "                              ",
            "       GO FOR THE WORLD       ",
            "         RECORD NOW !!        "
        };
        private string _HISCORE = string.Empty;

        /// <summary>
        /// ゲームを初期化する。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public override void Init() {

            string sc = string.Format("\"{0}\"", STAGEMAP.HISCORE.ToString());
            int len = (30 - sc.Length) / 2;
            _HISCORE = _DIDIT_01[3].Substring(0, len) + sc + _DIDIT_01[3].Substring(0, 30 - (len + sc.Length));

            _WAIT_TIME = 60;

            PlaySound(DxSound.SND_HIGH_SCORE_MUSIC, DX.DX_PLAYTYPE_BACK);
        }

        public override void Continue() {
        }

        public override void ReInit() {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public override GameState DrawScreen(GameState game) {

            // おまじない
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            DX.ProcessMessage();
            DX.ClearDrawScreen();

            if (!CheckPlaySound(DxSound.SND_HIGH_SCORE_MUSIC)) {
                game.State = Now.GameTitle;
                return game;
            }

            if (--_WAIT_TIME == 0)
                _WAIT_TIME = 60;

            //
            DrawContent(game);

            // おまじない
            DX.ScreenFlip();

            return game;
        }

        /// <summary>
        /// ステージに各アイテムを描画
        /// </summary>
        private void DrawContent(GameState game) {

            DX.DrawBox(0, 0, 480 + 1, 480 + 1, Settings.Navy, DX.TRUE);

            DrawString(1, 07, _DIDIT_01[0], Settings.Red);
            DrawString(1, 12, _DIDIT_01[1], Settings.White);
            DrawString(1, 14, _DIDIT_01[2], Settings.White);
            uint c = (_WAIT_TIME % 30 > 14) ? Settings.Red : Settings.Navy;
            DrawString(1, 17, _HISCORE, c);
            DrawString(1, 20, _DIDIT_01[4], Settings.Yellow);
            DrawString(1, 22, _DIDIT_01[5], Settings.Yellow);

            DrawScorePanel();
        }
    }
}