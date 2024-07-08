using System;
using System.Collections.Generic;
using DxLibDLL;

namespace RALLYDX {

    public abstract class Car {

        protected DxGraph DXGRAPH = null;
        protected DxFonts DXFONTS = null;
        protected DxSound DXSOUND = null;
        protected DxInput DXINPUT = null;

        protected StageMap STAGEMAP = null;
        
        protected int[] RightToUp   = { 3, 2, 1, 0 };
		protected int[] RightToDown = { 3, 4, 5, 6 };
		protected int[] RightToLeft = { 3, 4, 5, 6, 7, 8, 9 };
		protected int[] LeftToRight = { 9, 10, 11, 0, 1, 2, 3 };
		protected int[] LeftToUp    = { 9, 10, 11, 0 };
		protected int[] LeftToDown  = { 9, 8, 7, 6 };
		protected int[] UpToRight   = { 0, 1, 2, 3 };
		protected int[] UpToLeft    = { 0, 11, 10, 9 };
		protected int[] UpToDown    = { 0, 1, 2, 3, 4, 5, 6 };
		protected int[] DownToRight = { 6, 5, 4, 3 };
		protected int[] DownToLeft  = { 6, 7, 8, 9 };
		protected int[] DownToUp    = { 6, 7, 8, 9, 10, 11, 0 };
		protected int[] None        = { 0 };

		public int X;
		public int Y;
		public int GrHandle;
		public int MX;
		public int MY;

		public int MOD_X = 0;
		public int MOD_Y = 0;

		protected int MIN_X = 0;
		protected int MIN_Y = 0;		
		protected int MAX_X = 0;
		protected int MAX_Y = 0;

		public Direction DIRECTION = Direction.ToUp;

        protected Queue<int> ROTATE = new Queue<int>();

        /// <summary>
		/// Carクラスのコンストラクタ。
        /// クラス全体で一度初期化すればよいものを設定する。
		/// </summary>
        public Car() {

            DXGRAPH = DxGraph.GetInstance();
            DXFONTS = DxFonts.GetInstance();
            DXSOUND = DxSound.GetInstance();
            DXINPUT = DxInput.GetInstance();

            STAGEMAP = StageMap.GetInstance();
        }

		public abstract void Init(int position);

		public abstract void Draw();

        public abstract void Move();
		
		public abstract void Move(bool key_right, bool key_left, bool key_up, bool key_down);

        public virtual void Movef(int mx, int my) {
        }
    }

    public enum Direction {
        ToRight,
        ToLeft,
        ToUp,
        ToDown
    }
}