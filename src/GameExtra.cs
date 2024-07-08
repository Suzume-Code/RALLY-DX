using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// GameExtraクラス。
    /// </summary>
	public class GameExtra : GameBase {

        private readonly string[] _EXTRA_01 = {
            "      CHALLENGING STAGE       ",
            "           NO.{0}             ",
            "              = {0}           ",
            "              = {0}           ",
            "     RED CARS DON'T MOVE      ",
            "    UNTIL FUEL RUNS OUT.      "
        };
        private bool[] _SKIP_01 = new bool[6];

		/// <summary>
		/// GameMainクラスのコンストラクタ。
		/// </summary>
        public GameExtra() {

            MYCAR = MyCar.GetInstance();
            SMOKE = SmokeScreen.GetInstance();
            CHECKPOINT = CheckPoint.GetInstance();
            ROCKS = Rocks.GetInstance();
        }

        /// <summary>
        /// ゲームを初期化する。
        /// プレイごとに初期化すればよいものを設定する。
        /// </summary>
        public override void Init() {

            STAGEMAP.NextRound();

            MYCAR.Init(0);

            Settings.MYCAR_MOVE = Settings.EXTRA_MOVE;

            SMOKE.InitGame();
            CHECKPOINT.InitGame();
            ROCKS.InitGame();

            if (REDCAR != null)
                REDCAR.Clear();
            REDCAR = new List<RedCar>();
            for (int i = 0; i < STAGEMAP.RedCarsOfRound(); i++) {
                RedCar r = new RedCar();
                r.Init(i);
                REDCAR.Add(r);
            }

            for (int i = 0; i < _SKIP_01.Length; i++)
                _SKIP_01[i] = false;

            STATE = State.GameStart;
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

            bool key_smoke = (DXINPUT.IsKeyTrigger(DX.KEY_INPUT_RETURN) ||
                              DXINPUT.IsKeyTrigger(DX.KEY_INPUT_SPACE) ||
                              DXINPUT.IsPadTrigger(DX.PAD_INPUT_1) ||
                              DXINPUT.IsPadTrigger(DX.PAD_INPUT_2));
            bool key_right = (DXINPUT.IsKeyPress(DX.KEY_INPUT_RIGHT) ||
                              DXINPUT.IsPadPress(DX.PAD_INPUT_RIGHT));
            bool key_left  = (DXINPUT.IsKeyPress(DX.KEY_INPUT_LEFT) ||
                              DXINPUT.IsPadPress(DX.PAD_INPUT_LEFT));
            bool key_up    = (DXINPUT.IsKeyPress(DX.KEY_INPUT_UP) ||
                              DXINPUT.IsPadPress(DX.PAD_INPUT_UP));
            bool key_down  = (DXINPUT.IsKeyPress(DX.KEY_INPUT_DOWN) ||
                              DXINPUT.IsPadPress(DX.PAD_INPUT_DOWN));

            //
            if (STATE == State.GameStart) {
                PlaySound(DxSound.SND_CHALLENGING_STAGE_START_MUSIC, DxSound.PLAYTYPE_BACK);
                DrawStageGingle();
                STATE = State.Gingle;
                DX.ScreenFlip();
                return game;
            }
            // 開始ジングル
            if (STATE == State.Gingle) {
                DrawStageGingle();
                DX.ScreenFlip();
                if (CheckPlaySound(DxSound.SND_CHALLENGING_STAGE_START_MUSIC))
                    return game;
                STATE = State.Play;
                PlaySound(DxSound.SND_CHALLENGING_STAGE_MUSIC, DxSound.PLAYTYPE_LOOP);
                PlaySound(DxSound.SND_MY_CARRUN, DxSound.PLAYTYPE_LOOP);
            }

            // ラッキーフラグ
            if (STATE == State.Lucky) {
                if (!STAGEMAP.BonusFuel()) {
                    DrawContent(game);
                    DX.ScreenFlip();
                    return game;
                }
                STATE = State.Play;
                Settings.MYCAR_MOVE = Settings.EXTRA_MOVE;
                StopSound(DxSound.SND_FUEL_BONUS_SOUND);
                PlaySound(DxSound.SND_MY_CARRUN, DxSound.PLAYTYPE_LOOP);
                STAGEMAP.RestoreFuel();
            }

            // ステージクリア
            if (STATE == State.StageClear) {
                if (CheckPlaySound(DxSound.SND_ROUND_CLEAR_MUSIC))
                    return game;
                if (STAGEMAP.FUEL_LEFT > 0) {
                    PlaySound(DxSound.SND_FUEL_BONUS_SOUND, DxSound.PLAYTYPE_LOOP);
                    StopSound(DxSound.SND_MY_CARRUN);
                }
                STATE = State.Bonus;
                return game;
            }

            // 残燃料ボーナス
            if (STATE == State.Bonus) {
                if (!STAGEMAP.BonusFuel()) {
                    DrawContent(game);
                    DX.ScreenFlip();
                    return game;
                }
                StopSound(DxSound.SND_FUEL_BONUS_SOUND);
                STATE = State.Continue;
                game.State = Now.NextStage;
                return game;
            }

            // 衝突
            if (STATE == State.Crush) {
                if (CheckPlaySound(DxSound.SND_MY_CARCRASH)) {
                    DrawContent(game);
                    DX.ScreenFlip();
                    return game;
                }
                if (STAGEMAP.MYCAR_LEFT == 0) {
                    game.State = (STAGEMAP.TODAYS_HISCORE < STAGEMAP.HISCORE) ? Now.HighScore : Now.GameTitle;
                    return game;
                }
                // continueではなくnextへ
                STAGEMAP.MYCAR_LEFT--;
                STATE = State.NextRound;
                game.State = Now.NextStage;
                return game;
            }

            //
            SMOKE.DelaySmokeScreen(MYCAR.X, MYCAR.Y, MYCAR.DIRECTION);
            if (key_smoke) {
                SMOKE.AddSmokeScreen(MYCAR.X, MYCAR.Y, MYCAR.DIRECTION);
                STAGEMAP.UseFuel(120);
            }

            foreach (RedCar r in REDCAR)
                r.Move();

            MYCAR.Move(key_right, key_left, key_up, key_down);

            // チェックポイント
            bool lucky = CHECKPOINT.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y);

            // 衝突判定
            if (ROCKS.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y)) {
                StopSound(DxSound.SND_CHALLENGING_STAGE_MUSIC);
                StopSound(DxSound.SND_MY_CARRUN);
                PlaySound(DxSound.SND_MY_CARCRASH, DxSound.PLAYTYPE_BACK);
                STATE = State.Crush;
            } else {
                foreach (RedCar r in REDCAR) {
                    if (r.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y)) {
                        StopSound(DxSound.SND_CHALLENGING_STAGE_MUSIC);
                        StopSound(DxSound.SND_MY_CARRUN);
                        PlaySound(DxSound.SND_MY_CARCRASH, DxSound.PLAYTYPE_BACK);
                        STATE = State.Crush;
                        break;
                    }
                }
            }

            //
            DrawContent(game);

            // おまじない
            DX.ScreenFlip();


            CHECKPOINT.CleanUp();
            SMOKE.CleanUp();

            // チェックポイントフラグの残りを確認して
            bool remain = CHECKPOINT.FLAG.Any(chk => (!chk.Cleared));
            if (!remain) {
                STATE = State.StageClear;
                StopSound(DxSound.SND_CHALLENGING_STAGE_MUSIC);
                StopSound(DxSound.SND_MY_CARRUN);
                PlaySound(DxSound.SND_ROUND_CLEAR_MUSIC, DxSound.PLAYTYPE_BACK);
            }

            if (lucky) {
                if (remain) {
                    STATE = State.Lucky;
                    StopSound(DxSound.SND_MY_CARRUN);
                    PlaySound(DxSound.SND_FUEL_BONUS_SOUND, DxSound.PLAYTYPE_LOOP);
                    STAGEMAP.SaveFuel();
                }
            }

            return game;
        }

        /// <summary>
        /// ステージに各アイテムを描画
        /// </summary>
        private void DrawContent(GameState game) {

            DX.DrawBox(0, 0, 480 + 1, 480 + 1, Settings.BurlyWood, DX.TRUE);

            // 描画起点のマップセルの位置を計算
            int startX = MYCAR.MX - Settings.MYCAR_OFFSET;
            int startY = MYCAR.MY - Settings.MYCAR_OFFSET;

            // セル単位で静止オブジェクトを描画
            const int loop_max = 12;
            for (int x = 0; x < loop_max; x++) {
                for (int y = 0; y < loop_max; y++) {

                    // 画面の左上を起点にする。セルサイズの半分を外側にオフセット。
                    int plotX = (x * Settings.CELL_SIZE) - MYCAR.MOD_X - Settings.HALF_CELL_SIZE;
                    int plotY = (y * Settings.CELL_SIZE) - MYCAR.MOD_Y - Settings.HALF_CELL_SIZE;

                    // マップを描画する
                    int gr = STAGEMAP.MAP[(startY + y), (startX + x)];
                    DrawPixelImage(plotX, plotY, gr);

                    // チェックポイントフラグ
                    foreach (CheckPointInfo cp in CHECKPOINT.FLAG) {
                        if (cp.MX == (startX + x) && cp.MY == (startY + y) && !cp.Cleared)
                            DrawPixelImage(plotX, plotY, cp.GrHandle);
                    }

                    // 岩
                    foreach (PositionRock cp in ROCKS.ROCKS) {
                        if (cp.MX == (startX + x) && cp.MY == (startY + y))
                            DrawPixelImage(plotX, plotY, cp.GrHandle);
                    }

			        // スモークスクリーンの描画
                    foreach (Smoke smoke in SMOKE.SMOKESCREEN) {
                        if (smoke.MX == (startX + x) && smoke.MY == (startY + y))
                            DrawPixelImage(plotX, plotY, smoke.GrHandle);
                    }

                    // レッドカー
                    foreach (RedCar r in REDCAR)
                        if (r.MX == (startX + x) && r.MY == (startY + y))
                            DrawPixelImage(plotX + 8 + r.MOD_X, plotY + 8 + r.MOD_Y, r.GrHandle);

                    // スコア表示
                    CHECKPOINT.DrawScore(startX + x, startY + y, plotX, plotY);
                }
            }

            MYCAR.Draw();
            if (STATE == State.Crush) {
                MYCAR.Bang();
                if (STAGEMAP.MYCAR_LEFT == 0) {
                    string sg0 = "G A M E";
                    string sg1 = "O V E R";
                    DrawString(13, 14, sg0, Settings.White);
                    DrawString(13, 17, sg1, Settings.White);
                }
            }

            DrawScorePanel();
        }

        private void DrawStageGingle() {

            int[] row = { 6, 9, 13, 17, 20, 22 };

            DX.DrawBox(0, 0, 480 + 1, 480 + 1, Settings.BurlyWood, DX.TRUE);

            for (int i = 0; i < _SKIP_01.Length; i++) {

                // イメージの描画
                if (i == 2)
                    // Rockイメージを描画
                    DrawImage(10, 12, DXGRAPH.ROCK[0]);
                else if (i == 3)
                    // レッドカーイメージを描画
                    DrawPixelImage(9 * 16 + 8, 15 * 16 + 8, DXGRAPH.REDCAR[0]);

                string s = string.Empty;
                if (i == 1)
                    s = string.Format(_EXTRA_01[i], STAGEMAP.EXTRA_ROUND.ToString().PadRight(3));
                else if (i == 2)
                    s = string.Format(_EXTRA_01[i], STAGEMAP.RocksOfRound().ToString().PadRight(3));
                else if (i == 3)
                    s = string.Format(_EXTRA_01[i], STAGEMAP.RedCarsOfRound().ToString().PadRight(3));
                else
                    s = _EXTRA_01[i];
                uint c = (i == 0) ? Settings.Red : Settings.White;
                _SKIP_01[i] = DrawLineText(_SKIP_01[i], row[i], s, c);
                if (_SKIP_01[i] == false)
                    break;
            }

            DrawScorePanel();
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
    }
}