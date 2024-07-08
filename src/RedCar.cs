using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using DxLibDLL;

namespace RALLYDX {

	public class RedCar : Car {

        private MyCar _MYCAR = null;
        private SmokeScreen _SMOKE = null;
        private Rocks _ROCKS = null;

        private Random _RANDOM = new Random();

        private int _CAR_NUMBER = 0;

        private bool _IN_CHALLENGING_STAGE = false;
        private int _FIRST_WAIT = 0;

        private int[] _POSITION_OF_REDCARS = { 4, 3, 5, 2, 6, 0, 1 };

        private Queue<int> _SMOKESPIN = new Queue<int>();
        private int[] _UpToSpin    = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };


        /// <summary>
        /// レッドカーを設定する。
        /// </summary>
        /// <param name="position">レッドカーズのｎ番目を設定する。</param>
        public override void Init(int position) {

            _MYCAR = MyCar.GetInstance();
            _SMOKE = SmokeScreen.GetInstance();
            _ROCKS = Rocks.GetInstance();

            // ｎ番目の位置を取得する
            _CAR_NUMBER = STAGEMAP.Position(position);

            Position rc = STAGEMAP.INIT_REDCAR[_CAR_NUMBER];

            MX = rc.MX;
            MY = rc.MY;
            X = MX * Settings.CELL_SIZE;
            Y = MY * Settings.CELL_SIZE;
            MOD_X = 0;
            MOD_Y = 0;

			MIN_X = Settings.MAP_OFFSET * Settings.CELL_SIZE;	// + 24;
			MIN_Y = Settings.MAP_OFFSET * Settings.CELL_SIZE;	// + 24;
			MAX_X = (STAGEMAP.MAP_WIDTH - Settings.MAP_OFFSET - 1) * Settings.CELL_SIZE;	// + 24;
			MAX_Y = (STAGEMAP.MAP_HEIGHT - Settings.MAP_OFFSET - 1) * Settings.CELL_SIZE;	// + 24;

            GrHandle = DXGRAPH.REDCAR[0];

            if (_MYCAR.MY > MY) {
                GrHandle = DXGRAPH.REDCAR[6];
                DIRECTION = Direction.ToDown;
            } else {
                GrHandle = DXGRAPH.REDCAR[0];
                DIRECTION = Direction.ToUp;
            }

            _IN_CHALLENGING_STAGE = STAGEMAP.IsChallengingStage();
            _FIRST_WAIT = 60 * 5;         // ５秒
        }

        public override void Draw() {

            // 描画起点のマップセルの位置を計算
            int startX = _MYCAR.MX - Settings.MYCAR_OFFSET;
            int startY = _MYCAR.MY - Settings.MYCAR_OFFSET;
        }

        public override void Move() {

            if (_IN_CHALLENGING_STAGE) {
                if (STAGEMAP.FUEL_LEFT != 0)
                    return;
            } else {
                // ５秒程度スタートを待つ
                //if (--_FIRST_WAIT != 0)
                //    return;
                if (_FIRST_WAIT != 0) {
                    _FIRST_WAIT--;
                    return;
                }
            }

            //
            if (_SMOKESPIN.Count > 0) {
                int ss = _SMOKESPIN.Dequeue();
                GrHandle = DXGRAPH.REDCAR[ss];
                return;
            }

			// MyCar転回中
			if (ROTATE.Count > 0) {
				int tt = ROTATE.Dequeue();
				GrHandle = DXGRAPH.REDCAR[tt];
			}

            if(DIRECTION == Direction.ToUp) {
                if (_SMOKE.SMOKESCREEN.Any(y => y.MX == MX && (y.MY - 1) == MY)) {
                    SmokeSpinQueueSet(_UpToSpin);
                    return;
                }
                Y -= Settings.REDCAR_MOVE;
                if (Y <= MIN_Y)
				    Y = MIN_Y;
            }
            if(DIRECTION == Direction.ToDown) {
                if (_SMOKE.SMOKESCREEN.Any(y => y.MX == MX && (y.MY) == MY)) {
                    SmokeSpinQueueSet(_UpToSpin);
                    return;
                }
                Y += Settings.REDCAR_MOVE;
			    if (MAX_Y <= Y)
				    Y = MAX_Y;
            }
            if(DIRECTION == Direction.ToLeft) {
                if (_SMOKE.SMOKESCREEN.Any(y => (y.MX - 1) == MX && y.MY == MY)) {
                    SmokeSpinQueueSet(_UpToSpin);
                    return;
                }
			    X -= Settings.REDCAR_MOVE;
                if (X <= MIN_X)
				    X = MIN_X;
            }
            if(DIRECTION == Direction.ToRight) {
                if (_SMOKE.SMOKESCREEN.Any(y => (y.MX) == MX && y.MY == MY)) {
                    SmokeSpinQueueSet(_UpToSpin);
                    return;
                }
                X += Settings.REDCAR_MOVE;
                if (MAX_X <= X)
				    X = MAX_X;
            }
            
            MX = X / Settings.CELL_SIZE;
            MY = Y / Settings.CELL_SIZE;
            MOD_X = X % Settings.CELL_SIZE;
            MOD_Y = Y % Settings.CELL_SIZE;

            Direction mb = DIRECTION;

            IfCanChengeDirect();

            QueueSet(MovePattern(mb, DIRECTION));
        }

		public override void Move(bool key_right, bool key_left, bool key_up, bool key_down) {
		}

        private void IfCanChengeDirect() {

            // きっちり曲がれるおまじない
            if (MOD_X != 0 || MOD_Y != 0)
                return;

            List<Direction> directs = GetCanChengeDirects();

            if (directs.Count > 0) {

                // 効率よく追うことができる方向を取得する
                bool right = (X < _MYCAR.X);
                bool left = (X > _MYCAR.X);
                bool down = (Y < _MYCAR.Y);
                bool up = (Y > _MYCAR.Y);
 
                // 効率よく追うことができる方向に進路変換は可能か？
                List<Direction> directs2 = new List<Direction>();
                if (directs.Any(x => x == Direction.ToUp) && up == true)
                    directs2.Add(Direction.ToUp);
                if (directs.Any(x => x == Direction.ToDown) && down == true)
                    directs2.Add(Direction.ToDown);
                if (directs.Any(x => x == Direction.ToLeft) && left == true)
                    directs2.Add(Direction.ToLeft);
                if (directs.Any(x => x == Direction.ToRight) && right == true)
                    directs2.Add(Direction.ToRight);
 
                if (directs2.Count == 1) {
                    // 効率よく追うことができる方向がひとつだけならそれを選ぶ
                    DIRECTION = directs2[0];
                } else if (directs2.Count > 1) {
                    // 効率よく追うことができる方向が複数あるなら乱数で決める
                    int i = _RANDOM.Next(0, directs2.Count);
                    DIRECTION = directs2[i];
                } else {
                    // 効率よく追うことができる方向に移動できない場合はこれまでどおりの方法で選ぶ
                    int i = _RANDOM.Next(0, directs.Count);
                    DIRECTION = directs[i];
                }
            } else {
                // 袋小路にはまった場合はバックする
                if (DIRECTION == Direction.ToUp)
                    DIRECTION = Direction.ToDown;
                else if (DIRECTION == Direction.ToDown)
                    DIRECTION = Direction.ToUp;
                else if (DIRECTION == Direction.ToRight)
                    DIRECTION = Direction.ToLeft;
                else if (DIRECTION == Direction.ToLeft)
                    DIRECTION = Direction.ToRight;
            }
        }

        /// <summary>
        /// 移動できる方向を取得する。
        /// </summary>
        /// <returns>移動できる方向のリストを返却する。</returns>
        private List<Direction> GetCanChengeDirects() {

            List<Direction> directs = new List<Direction>();

            // 上に
            if (!STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY - 1)))
                if (!_ROCKS.ROCKS.Any(r => (r.MX == MX && r.MY == MY - 1)))
                    if(DIRECTION != Direction.ToDown)
                        directs.Add(Direction.ToUp);

            // 下に
            if (!STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY + 1)))
                if (!_ROCKS.ROCKS.Any(r => (r.MX == MX && r.MY == MY + 1)))
                    if(DIRECTION != Direction.ToUp)
                        directs.Add(Direction.ToDown);

            // 左に
            if (!STAGEMAP.BLOCKS.Any(b => (b.MX == MX + 1 && b.MY == MY)))
                if (!_ROCKS.ROCKS.Any(r => (r.MX == MX + 1 && r.MY == MY)))
                    if(DIRECTION != Direction.ToLeft)
                        directs.Add(Direction.ToRight);

            // 右に
            if (!STAGEMAP.BLOCKS.Any(b => (b.MX == MX - 1 && b.MY == MY)))
                if (!_ROCKS.ROCKS.Any(r => (r.MX == MX + 1 && r.MY == MY)))
                    if(DIRECTION != Direction.ToRight)
                        directs.Add(Direction.ToLeft);

            return directs;
        }

		private int[] MovePattern(Direction bm, Direction m) {

			if (bm == Direction.ToRight) {
				if (m == Direction.ToUp)
					return RightToUp;
				if (m == Direction.ToDown)
					return RightToDown;
				return RightToLeft;
			}
			if (bm == Direction.ToLeft) {
				if (m == Direction.ToUp)
					return LeftToUp;
				if (m == Direction.ToDown)
					return LeftToDown;
				return LeftToRight;
			}
			if (bm == Direction.ToUp) {
				if (m == Direction.ToRight)
					return UpToRight;
				if (m == Direction.ToLeft)
					return UpToLeft;
				return UpToDown;
			}
			if (bm == Direction.ToDown) {
				if (m == Direction.ToRight)
					return DownToRight;
				if (m == Direction.ToLeft)
					return DownToLeft;
				return DownToUp;
			}
			return null;
		}

		private void QueueSet(int[] pat) {
		
			ROTATE.Clear();
			foreach (int  p in pat)
				ROTATE.Enqueue(p);
		}

		private void SmokeSpinQueueSet(int[] pat) {
		
			_SMOKESPIN.Clear();
			foreach (int  p in pat)
				_SMOKESPIN.Enqueue(p);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="my"></param>
        /// <param name="mod_x"></param>
        /// <param name="mod_y"></param>
        /// <returns></returns>
        public bool JudgeConflict(int mx, int my, int mod_x, int mod_y) {

            if (MX == mx) {
                int x = Math.Abs((MX * 48 + MOD_X) - (mx * 48 + mod_x)); 
                int y = Math.Abs((MY * 48 + MOD_Y) - (my * 48 + mod_y));
                if (x <= 16 && y <= 16)
                    return true;
                return false;
            }
            if (MY == my) {
                int x = Math.Abs((MX * 48 + MOD_X) - (mx * 48 + mod_x));
                int y = Math.Abs((MY * 48 + MOD_Y) - (my * 48 + mod_y));
                if (x <= 16 && y <= 16)
                    return true;
                return false;
            }
            return false;
        }
    }
}