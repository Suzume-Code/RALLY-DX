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
	public class GameTitle : GameBase {
		
        //
        private int _TITLE = 0;

        private const int _WAIT_TIME_DEFAULT = 900;
        private int _WAIT_TIME = _WAIT_TIME_DEFAULT;
        
        //
        private string[] _INTRO_02 = {
            "        NEW RALLY-DX          ",
            "           CAST               ",
            "        MY CAR                ",
            "        RED CAR               ",
            "        CHECK POINT           ",
            "        SPECIAL CHECK POINT   ",
            "        LUCKY CHECK POINT     ",
            "        ROCK ( DANGER ! )     ",
            "        SMOKE SCREEN          ",
            "                              "
        };
        private bool[] _SKIP_02 = new bool[10];
        //
        private string[] _INTRO_03 = {
            "      PUSH START BUTTON       ",
            "   BONUS CAR FOR 20000 PTS    ",
            "        AND 100000 PTS        ",
            "          FREE PLAY           ",
            "                              "
        };
        private bool[] _SKIP_03 = new bool[5];

        /// <summary>
        /// ゲームを初期化する。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public override void Init() {

            _TITLE = 0;

            for (int i = 0; i < _SKIP_02.Length; i++)
                _SKIP_02[i] = false;
            STAGEMAP.SYSTIME = 0;
        }

        public override void Continue() {
        }

        public override void ReInit() {
        }

        /// <summary>
        /// DxLibへ描画する。
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public override GameState DrawScreen(GameState game) {

            // おまじない
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            DX.ProcessMessage();
            DX.ClearDrawScreen();

            // スペースキーを押下してゲーム開始
            bool key_down = (DXINPUT.IsKeyTrigger(DX.KEY_INPUT_SPACE) ||
                             DXINPUT.IsKeyTrigger(DX.KEY_INPUT_RETURN) ||
                             DXINPUT.IsPadTrigger(DX.PAD_INPUT_1) ||
                             DXINPUT.IsPadTrigger(DX.PAD_INPUT_2));

            if (key_down) {
                if (_TITLE == 2) {
                    game.State = Now.NormalStage;
                    return game;
                } else {
                    //DX.PlaySoundMem(DXSOUND.SOUND[DxSound.SND_CREDIT_SOUND], DX.DX_PLAYTYPE_BACK, DX.TRUE);
                    PlaySound(DxSound.SND_CREDIT_SOUND, DxSound.PLAYTYPE_BACK);
                    _TITLE = 2;
                }
            }

            DrawContent();

            //
            if (--_WAIT_TIME == 0) {
                _WAIT_TIME = _WAIT_TIME_DEFAULT;
                _TITLE++;
                if (_TITLE == 1)
                    for (int i = 0; i < _SKIP_02.Length; i++)
                        _SKIP_02[i] = false;
                if (_TITLE >= 2)
                    _TITLE = 0;
            }

            // おまじない
            DX.ScreenFlip();

            return game;
        }

        /// <summary>
        /// ステージに各アイテムを描画
        /// </summary>
        protected override void DrawContent() {

            if (_TITLE == 0)
                DrawAvantTitle();
            else if (_TITLE == 1)
                DrawCastPanel();
            else
                DrawCreditPanel();
            
            // スコアパネル
            DrawScorePanel();
        }

        /// <summary>
        /// アバンタイトルを描画する。
        /// </summary>
        private void DrawAvantTitle() {

            DX.DrawBox(0, 0, 480 + 1, 480 + 1, Settings.Black, DX.TRUE);

            DrawPixelImage(80, 80, DXGRAPH.AVANT[0]);
            DrawImage(11, 27, DXGRAPH.AVANT[1]);

            uint c = (STAGEMAP.SYSTIME % 30 >= 20) ? Settings.Black : Settings.White;
            DrawString(1, 20, "   PUSH SPACE KEY OR BUTTON   ", c);
        }

        /// <summary>
        /// キャストパネルを描画する。
        /// </summary>
        private void DrawCastPanel() {

            int[] row = { 3, 5, 8, 10, 12, 14, 16, 19, 22, 28 };

            DX.DrawBox(0, 0, 480 + 1, 480 + 1, Settings.BurlyWood, DX.TRUE);

            for (int i = 0; i < _SKIP_02.Length; i++) {
                uint c = (i == 0) ? Settings.Red : Settings.White;
                _SKIP_02[i] = DrawLineText(_SKIP_02[i], row[i], _INTRO_02[i], c);
                if (i == 2)
                    DrawPixelImage((5 * 16) - 8, (07 * 16) - 8, DXGRAPH.MYCAR[0]);
                else if (i == 3)
                    DrawPixelImage((5 * 16) - 8, (09 * 16) - 8, DXGRAPH.REDCAR[0]);
                else if (i == 4)
                    DrawImage(5, 11, DXGRAPH.FLAG[0]);
                else if (i == 5)
                    DrawImage(5, 13, DXGRAPH.FLAG[1]);
                else if (i == 6)
                    DrawImage(5, 15, DXGRAPH.FLAG[2]);
                else if (i == 7)
                    DrawImage(5, 18, DXGRAPH.ROCK[0]);
                else if (i == 8)
                    DrawImage(5, 21, DXGRAPH.SMOKE[0]);
                else if (i == 9)
                    DrawImage(11, 27, DXGRAPH.AVANT[1]);
                if (_SKIP_02[i] == false)
                    break;
            }
        }

        /// <summary>
        /// 指定した文字列から１文字ずつ描画する。
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="row"></param>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool DrawLineText(bool skip, int row, string s, uint c) {

            if (skip) {
                DrawString(1, row, s, c);
                return true;
            }
            int col = STAGEMAP.SYSTIME / 2;
            string str = s.Substring(0, col);
            DrawString(1, row, str, c);
            if (col >= 29) {
                STAGEMAP.SYSTIME = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// クレジットパネルを描画する。
        /// </summary>
        private void DrawCreditPanel() {

            int[] row = { 8, 12, 16, 20, 24 };

            for (int i = 0; i < _SKIP_03.Length; i++) {
                uint c = Settings.White;
                DrawString(1, row[i], _INTRO_03[i], c);
            }
        }
    }
}