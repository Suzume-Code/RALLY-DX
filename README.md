# Name
RALLY-DX for C# with DxLib.

Rally-X/New Rally-Xゲームの自作版。

# DEMO


# Features
当初は、C#のみで作成していましたが音が上手くだせないと悩んでいたところ
C#でDxLibを使用できることがわかったので、DxLib版へ進路変更しました。
DxLibを使用するため、コントローラーも使えます。

# Requirement
下記のものが必要になります。
* DxLib (VisualC# 用パッケージ)
* C# version 5 (Windows標準)

# Installation
DxLib (VisualC# 用パッケージ)のインストール
ＤＸライブラリ置き場　https://dxlib.xsrv.jp/ の「ＤＸライブラリのダウンロード」ページにある
「VisualC# 用パッケージ」をダウンロードしてください。
zip形式で圧縮されているので、解凍後以下のファイルをプロジェクト内にコピーする。
* DxDLL.cs       コンパイル時に必要
* DxLib.dll      実行時に必要
* DxLib_x64.dll  実行時に必要
* DxLibDotNet    実行時に必要

実行時に必要なdllは、PATHの通った場所に配置してもらっても問題ありません。
プロジェクト内の「コンパイル.bat」を実行して.exeを作成します。

C#のコンパイラは、「コンパイル.bat」にパスを記載していますので適宜変更をお願いします。
DotNET Frameworkのバージョンはv4.0.30319です。（Windows11のもの）

# Usage
「コンパイル.bat」で作成した.exeを実行します。

Setings.xmlについて
* DxLog                  true:ログ作成 false:ログ作成しない
* FullScreenMode         true:フルスクリーンモード false:ウィンドウモード

# Note
Namco名作ゲーム集から

# Author
 
# License
