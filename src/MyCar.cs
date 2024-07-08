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

	public class MyCar : Car {

        // インスタンス
        private static MyCar singleton = null;

        /// <summary>
        /// MyCarのインスタンスを作成
        /// </summary>
        /// <returns>当クラスのインスタンスを返却する。シングルトン構成です。</returns>
        public static MyCar GetInstance() {

            if (singleton == null) {
                singleton = new MyCar();
            }
            return singleton;
        }

		/// <summary>
		/// MyCarクラスのコンストラクタ。
		/// </summary>
		//private MyCar() {
		//
			//_DXGRAPH = DxGraph.GetInstance();
			//_DXSOUND = DxSound.GetInstance();
			//_STAGEMAP = StageMap.GetInstance();
			//_CHECKPOINT = CheckPoint.GetInstance();
		//}

		/// <summary>
		/// MyCarの初期値初期値を設定する。
		/// </summary>
		public override void Init(int position) {

			//　マップ座標から実座標へ変換
			X = STAGEMAP.INIT_MYCAR[position].X * Settings.CELL_SIZE;
			Y = STAGEMAP.INIT_MYCAR[position].Y * Settings.CELL_SIZE;

			GrHandle = DXGRAPH.MYCAR[position];
			
			// マップ座標
			MX = STAGEMAP.INIT_MYCAR[position].X;
			MY = STAGEMAP.INIT_MYCAR[position].Y;

			MOD_X = 0;
			MOD_Y = 0;

			MIN_X = Settings.MAP_OFFSET * Settings.CELL_SIZE;
			MIN_Y = Settings.MAP_OFFSET * Settings.CELL_SIZE;
			MAX_X = (STAGEMAP.MAP_WIDTH - Settings.MAP_OFFSET - 1) * Settings.CELL_SIZE;
			MAX_Y = (STAGEMAP.MAP_HEIGHT - Settings.MAP_OFFSET - 1) * Settings.CELL_SIZE;

			DIRECTION = Direction.ToUp;
			Settings.MYCAR_MOVE = Settings.NORMAL_MOVE;

			//Console.WriteLine("-----------");
			//Console.WriteLine("MIN-X=" + MIN_X.ToString());
			//Console.WriteLine("MIN-Y=" + MIN_Y.ToString());
			//Console.WriteLine("MAX-X=" + MAX_X.ToString());
			//Console.WriteLine("MAX-Y=" + MAX_Y.ToString());

			//Console.WriteLine("-----------");
			//Console.WriteLine("X=" + X.ToString() + ",MX=" + MX.ToString() + ",MOD_X=" + MOD_X.ToString());
			//Console.WriteLine("Y=" + Y.ToString() + ",MY=" + MY.ToString() + ",MOD_Y=" + MOD_Y.ToString());
			//Console.WriteLine("-----------");
			//Console.WriteLine("----------------------");
		}

		/// <summary>
		/// MyCarを描画する。
		/// </summary>
		public override void Draw() {

			const int BASE_OFFSET = 216;	//(Settings.CELL_SIZE * 4) + 24;画面中央 Pixel
			const int MYCAR_OFFSET = 8;		// Pixel

			// MyCar転回中
			if (ROTATE.Count > 0) {
				int tt = ROTATE.Dequeue();
				GrHandle = DXGRAPH.MYCAR[tt];
			}

			DX.DrawGraph(BASE_OFFSET + MYCAR_OFFSET, BASE_OFFSET + MYCAR_OFFSET, GrHandle, DX.TRUE);
		}

		public void Bang() {

			const int BASE_OFFSET = 216;	//(Settings.CELL_SIZE * 4) + 24;画面中央 Pixel
			const int MYCAR_OFFSET = 0;		// Pixel

			DX.DrawGraph(BASE_OFFSET + MYCAR_OFFSET, BASE_OFFSET + MYCAR_OFFSET, DXGRAPH.BANG[0], DX.TRUE);
		}

		public override void Move() {
		}

		public override void Move(bool key_right, bool key_left, bool key_up, bool key_down) {

            bool moved = false;

            if (key_right && CanMoveRight())
				moved = MoveRight();
            if (key_left && CanMoveLeft())
                moved = MoveLeft();
            if (key_up && CanMoveUp())
                moved = MoveUp();
            if (key_down && CanMoveDown())
                moved = MoveDown();
            if (!moved)
                AutoDrive();

			// 燃料を消費する
            STAGEMAP.UseFuel();
		}

		/// <summary>
		/// MyCar右に移動する。
		/// </summary>
		private bool MoveRight() {

			// きっちり曲がれるおまじない
			//if (MOD_Y != 0)
			//	return false;
			// ブロックにぶつかり停止
			//if (!CanMoveRight() && MOD_X == 0)
			//	return false;
			//if (MAX_X <= X)
			//	return false;
			// 曲がるアニメーション
			if (DIRECTION != Direction.ToRight) {
				QueueSet(MovePattern(Direction.ToRight));
				DIRECTION = Direction.ToRight;
				return false;
			}

			X += Settings.MYCAR_MOVE;
			if (MAX_X <= X)
				X = MAX_X;

			MX = X / Settings.CELL_SIZE;
			MOD_X = X % Settings.CELL_SIZE;

			return true;
		}

		/// <summary>
		/// MyCar左に移動する。
		/// </summary>
		private bool MoveLeft() {

			// きっちり曲がれるおまじない
			//if (MOD_Y != 0)
			//	return false;
			// ブロックにぶつかり停止
			//if (!CanMoveLeft() && MOD_X == 0)
			//	return false;
			//if (X <= MIN_X)
			//	return false;
			// 曲がるアニメーション
			if (DIRECTION != Direction.ToLeft) {
				QueueSet(MovePattern(Direction.ToLeft));
				DIRECTION = Direction.ToLeft;
				return false;
			}

			X -= Settings.MYCAR_MOVE;
			if (X <= MIN_X)
				X = MIN_X;

			MX = X / Settings.CELL_SIZE;
			MOD_X = X % Settings.CELL_SIZE;

			return true;
		}

		/// <summary>
		/// MyCar上に移動する。
		/// </summary>
		private bool MoveUp() {

			// きっちり曲がれるおまじない
			//if (MOD_X != 0)
			//	return false;
			// ブロックにぶつかり停止
			//if (!CanMoveUp() && MOD_Y == 0)
			//	return false;
			//if (Y <= MIN_Y)
			//	return false;
			// 曲がるアニメーション
			if (DIRECTION != Direction.ToUp) {
				QueueSet(MovePattern(Direction.ToUp));
				DIRECTION = Direction.ToUp;
				return false;
			}

			Y -= Settings.MYCAR_MOVE;
			if (Y <= MIN_Y)
				Y = MIN_Y;

			MY = Y / Settings.CELL_SIZE;
			MOD_Y = Y % Settings.CELL_SIZE;

			return true;
		}

		/// <summary>
		/// MyCar下に移動する。
		/// </summary>
		/// <returns>下向きに曲がれた場合はtrueを返却する。曲がれない場合はfalseを返却する。</returns>
		private bool MoveDown() {

			// きっちり曲がれるおまじない
			//if (MOD_X != 0)
			//	return false;
			// ブロックにぶつかり停止
			//if (!CanMoveDown() && MOD_Y == 0)
			//	return false;
			//if (MAX_Y <= Y)
			//	return false;
			// 曲がるアニメーション
			if (DIRECTION != Direction.ToDown) {
				QueueSet(MovePattern(Direction.ToDown));
				DIRECTION = Direction.ToDown;
				return false;
			}

			Y += Settings.MYCAR_MOVE;
			if (MAX_Y <= Y)
				Y = MAX_Y;

			MY = Y / Settings.CELL_SIZE;
			MOD_Y = Y % Settings.CELL_SIZE;

			return true;
		}

		private int[] MovePattern(Direction m) {

			if (DIRECTION == Direction.ToRight) {
				if (m == Direction.ToUp)
					return RightToUp;
				if (m == Direction.ToDown)
					return RightToDown;
				return RightToLeft;
			}
			if (DIRECTION == Direction.ToLeft) {
				if (m == Direction.ToUp)
					return LeftToUp;
				if (m == Direction.ToDown)
					return LeftToDown;
				return LeftToRight;
			}
			if (DIRECTION == Direction.ToUp) {
				if (m == Direction.ToRight)
					return UpToRight;
				if (m == Direction.ToLeft)
					return UpToLeft;
				return UpToDown;
			}
			if (DIRECTION == Direction.ToDown) {
				if (m == Direction.ToRight)
					return DownToRight;
				if (m == Direction.ToLeft)
					return DownToLeft;
				return DownToUp;
			}
			return null;
		}

		/// <summary>
		/// 右に曲がれるかを判断する。
		/// </summary>
		/// <returns>曲がれる場合trueを返却する。</returns>
		private bool CanMoveRight() {

			if (MOD_Y != 0)
				return false;
			if (STAGEMAP.BLOCKS.Any(b => (b.MX == MX + 1 && b.MY == MY)))
				return false;
			if (MAX_X <= X)
				return false;
			return true;
			//return !_STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY));
		}

		private bool CanMoveLeft() {

			if (MOD_Y != 0)
				return false;
			if (STAGEMAP.BLOCKS.Any(b => (b.MX == MX - 1 && b.MY == MY)))
				return false;
			if (X <= MIN_X)
				return false;
			return true;
			//return !STAGEMAP.BLOCKS.Any(b => (b.MX == MX - 1 && b.MY == MY));
			//return !_STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY));
		}

		private bool CanMoveUp() {

			if (MOD_X != 0)
				return false;
			if (STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY - 1)))
				return false;
			if (Y <= MIN_Y)
				return false;
			return !STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY - 1));
			//return !_STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY));
		}

		private bool CanMoveDown() {

			if (MOD_X != 0)
				return false;
			if (STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY + 1)))
				return false;
			if (MAX_Y <= Y)
				return false;
			return !STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY + 1));
			//return !_STAGEMAP.BLOCKS.Any(b => (b.MX == MX && b.MY == MY));
		}

		

		private void QueueSet(int[] pat) {
		
			ROTATE.Clear();
			foreach (int  p in pat)
				ROTATE.Enqueue(p);
		}

		private void AutoDrive() {

			// ブロックを移動中
			if (MOD_X != 0 || MOD_Y != 0) {
				if (DIRECTION == Direction.ToRight)
					MoveRight();
				else if (DIRECTION == Direction.ToLeft)
					MoveLeft();
				else if (DIRECTION == Direction.ToUp)
					MoveUp();
				else
					MoveDown();
				return;
			}

			if (DIRECTION == Direction.ToRight) {
				if (!CanMoveRight()) {	//} || (MAX_X <= X)) {
					if (CanMoveDown())
						MoveDown();
					else if (CanMoveUp())
						MoveUp();
					else if (CanMoveLeft())
						MoveLeft();
					return;
				}
				MoveRight();
				return;
			}
			if (DIRECTION == Direction.ToLeft) {
				if (!CanMoveLeft()) {	//} || (X <= MIN_X)) {
					if (CanMoveUp())
						MoveUp();
					else if (CanMoveDown())
						MoveDown();
					else if (CanMoveRight())
						MoveRight();
					return;
				}
				MoveLeft();
				return;
			}
			if (DIRECTION == Direction.ToUp) {
				if (!CanMoveUp()) {		//} || (Y <= MIN_Y)) {
					if (CanMoveRight())
						MoveRight();
					else if (CanMoveLeft())
						MoveLeft();
					else if (CanMoveDown())
						MoveDown();
					return;
				}
				MoveUp();
				return;
			}
			if (DIRECTION == Direction.ToDown) {
				if (!CanMoveDown())	{		//|| (MAX_Y <= Y)) {
					if (CanMoveLeft())
						MoveLeft();
					else if (CanMoveRight())
						MoveRight();
					else if (CanMoveUp())
						MoveUp();
					return;
				}
				MoveDown();
				return;
			}
		}


	}

}