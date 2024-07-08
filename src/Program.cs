using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using DxLibDLL;

namespace RALLYDX {

	class Program {

		[STAThread]
		public static void Main(string[] args) {

			if (StartupProcessCheck())
				return;

			Settings settings = new Settings();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GameForm f = new GameForm();
            f.Show();

            while (f.Created) {
				//");
				f.MainLoop();
                Application.DoEvents();
            }
		}

        /// <summary>
		/// アプリケーション開始時のチェック。
		/// </summary>
        /// <returns>自分自身を含んで同名のプロセスが２つ以上起動していればtrue、以外はfalse。</returns>
        private static bool StartupProcessCheck() {

			string processName = Process.GetCurrentProcess().ProcessName;
			return (Process.GetProcessesByName(processName).Length > 1);
        }
	}

	/// <summary>
	/// プログラム全体の設定値クラス。
	/// </summary>
	public class Settings {

        public static readonly string WINDOW_TITLE = "RALLY-DX";

		public static readonly int FORM_WIDTH  = 640;
		public static readonly int FORM_HEIGHT = 480;
		public static readonly int COLOR_DEPTH = 8;

		// DxLib設定値
		public static int DX_LOG         = DX.TRUE;
		public static int DX_WINDOW_MODE = DX.TRUE;
		public static int DX_FPS         = 60;

		public static int MYCAR_MOVE     = NORMAL_MOVE;
		public static int REDCAR_MOVE    = NORMAL_MOVE;
		public const int EXTRA_MOVE      = 6;
		public const int NORMAL_MOVE     = 4;
		public const int SLOW_MOVE       = 3;
		public const int VERY_SLOW_MOVE  = 2;

		//
		public static readonly int CELL_SIZE      = 48;
		public static readonly int HALF_CELL_SIZE = 24;
		public static readonly int MAP_OFFSET     = 6;
		public static readonly int MYCAR_OFFSET   = 5;

		//
		public static readonly int INIT_MYCAR = 2;

		//
		public static readonly int FIRST_EXTEND  = 20000;
        public static readonly int SECOND_EXTEND = 100000;

		// DxLibカラー設定値
		public const uint Magenta   = 0xFF00FF;
        public const uint Yellow    = 0xFFFF00;
        public const uint SkyBlue   = 0x87CEEB;
        public const uint White     = 0xFFFFFF;
        public const uint LimeGreen = 0x32CD32;
		public const uint Red       = 0xFF0000;
		public const uint Black     = 0x000000;
		public const uint BurlyWood = 0xDEB887;
		public const uint Navy      = 0x000080;
		public const uint LightGray = 0xD3D3D3;

		// エラーメッセージ
		public static readonly string ERR_NO_GRAPH      = "指定のグラフィックファイルが見つかりません。\n{0}";
		public static readonly string ERR_NO_READ_GRAPH = "読み込めないグラフィックファイルです。\n{0}";
		public static readonly string ERR_NO_FONT       = "指定のフォントファイルが見つかりません。\n{0}";
		public static readonly string ERR_NO_SOUND      = "指定のサウンドファイルが見つかりません。\n{0}";
		public static readonly string ERR_NO_INIT       = "DxLibを初期化できませんでした。";
		public static readonly string ERR_NO_XMLFILE    = "設定ファイルがありません。\n{0}";
		public static readonly string ERR_WRONG_XML     = "設定ファイルに記載誤りがあります。\n{0}";
		public static readonly string ERR_NO_CHKPOINT   = "チェックポイントの個数が10未満です。\n10以上に設定してください。\n現在の設定値={0}";
		public static readonly string ERR_NO_ROCKS      = "岩{0}";
		public static readonly string ERR_NO_REDCAR     = "レッドカーの台数が8未満です。\n8以上に設定してください。";

		public Settings() {

			string xml_path = Path.Combine(Environment.CurrentDirectory, "Settings.xml");

            if (!File.Exists(xml_path))
				ExitApplication(string.Format(ERR_NO_XMLFILE, xml_path));

			XmlDocument xml_doc = new XmlDocument();
            try {
                xml_doc.PreserveWhitespace = false;
                xml_doc.Load(xml_path);
				//
				string path = "/SystemSettings/System/DxLog";
				bool dx_log = Convert.ToBoolean(xml_doc.SelectSingleNode(path).InnerText);
				DX_LOG = (dx_log) ? DX.TRUE : DX.FALSE;
				
				path = "/SystemSettings/System/FullScreenMode";
				bool full_screen_mode = Convert.ToBoolean(xml_doc.SelectSingleNode(path).InnerText);
				DX_WINDOW_MODE = (full_screen_mode) ? DX.FALSE : DX.TRUE;
			}
			catch (Exception ex) {
				ExitApplication(string.Format(ERR_WRONG_XML, ex.Message));
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
}