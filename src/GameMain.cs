using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

    /// <summary>
    /// GameMainクラス。
    /// </summary>
	public class GameMain : GameBase {

		/// <summary>
		/// GameMainクラスのコンストラクタ。
		/// </summary>
        public GameMain() {

            MYCAR = MyCar.GetInstance();
            SMOKE = SmokeScreen.GetInstance();
            CHECKPOINT = CheckPoint.GetInstance();
            ROCKS = Rocks.GetInstance();
        }

        public override void Init() {

            STAGEMAP.InitGame();

            MYCAR.Init(0);

            Settings.MYCAR_MOVE = Settings.NORMAL_MOVE;

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

            STATE = State.GameStart;
        }

        private void ContinueGame() {

            STAGEMAP.MYCAR_LEFT--;
            STAGEMAP.ResetFuel();

            MYCAR.Init(0);

            Settings.MYCAR_MOVE = Settings.NORMAL_MOVE;

            SMOKE.ContinueGame();
            CHECKPOINT.ContinueGame();

            if (REDCAR != null)
                REDCAR.Clear();
            REDCAR = new List<RedCar>();
            for (int i = 0; i < STAGEMAP.RedCarsOfRound(); i++) {
                RedCar r = new RedCar();
                r.Init(i);
                REDCAR.Add(r);
            }

            STATE = State.GameStart;
        }

        public void NextRound() {

            STAGEMAP.NextRound();

            MYCAR.Init(0);

            Settings.MYCAR_MOVE = Settings.NORMAL_MOVE;

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

            STATE = State.GameStart;
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

            // 開始ジングル
            if (STATE == State.Gingle) {
                if (CheckPlaySound(DxSound.SND_GAMESTART_MUSIC))
                    return game;
                STATE = State.Play;
                PlaySound(DxSound.SND_IN_GAME_MUSIC, DxSound.PLAYTYPE_LOOP);
                PlaySound(DxSound.SND_MY_CARRUN, DxSound.PLAYTYPE_LOOP);
            }

            if (STATE == State.Continue) {
                ContinueGame();
                STATE = State.GameStart;
            }

            //
            if (STATE == State.NextRound) {
                NextRound();
                STATE = State.GameStart;
                return game;
            }

            // ラッキーフラグ
            if (STATE == State.Lucky) {
                if (!STAGEMAP.BonusFuel()) {
                    DrawContent(game);
                    DX.ScreenFlip();
                    return game;
                }
                STATE = State.Play;
                Settings.MYCAR_MOVE = 4;
                StopSound(DxSound.SND_FUEL_BONUS_SOUND);
                PlaySound(DxSound.SND_MY_CARRUN, DxSound.PLAYTYPE_LOOP);
                STAGEMAP.RestoreFuel();
            }

            // ステージクリア
            if (STATE == State.StageClear) {
                if (CheckPlaySound(DxSound.SND_ROUND_CLEAR_MUSIC))
                    return game;
                if (STAGEMAP.FUEL_LEFT > 0) {
                    PlaySound(DxSound.SND_FUEL_BONUS_SOUND, DX.DX_PLAYTYPE_LOOP);
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
                STATE = State.NextRound;
                if (STAGEMAP.IsNextChallengingStage())
                    game.State = Now.ExtraStage;
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
                STATE = State.Continue;
                return game;
            }

            // スモークスクリーン
            SMOKE.DelaySmokeScreen(MYCAR.X, MYCAR.Y, MYCAR.DIRECTION);
            if (key_smoke)
                SMOKE.AddSmokeScreen(MYCAR.X, MYCAR.Y, MYCAR.DIRECTION);

            foreach (RedCar r in REDCAR)
                r.Move();

            // マイカー移動
            MYCAR.Move(key_right, key_left, key_up, key_down);

            // チェックポイント通過
            bool lucky = CHECKPOINT.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y);

            // 衝突判定
            if (ROCKS.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y)) {
                StopSound(DxSound.SND_IN_GAME_MUSIC);
                StopSound(DxSound.SND_MY_CARRUN);
                PlaySound(DxSound.SND_MY_CARCRASH, DxSound.PLAYTYPE_BACK);
                STATE = State.Crush;
            } else {
                foreach (RedCar r in REDCAR) {
                    if (r.JudgeConflict(MYCAR.MX, MYCAR.MY, MYCAR.MOD_X, MYCAR.MOD_Y)) {
                        StopSound(DxSound.SND_IN_GAME_MUSIC);
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
                StopSound(DxSound.SND_IN_GAME_MUSIC);
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

            if (STATE == State.GameStart) {
                PlaySound(DxSound.SND_GAMESTART_MUSIC, DxSound.PLAYTYPE_BACK);
                STATE = State.Gingle;
                return game;
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
    }
}