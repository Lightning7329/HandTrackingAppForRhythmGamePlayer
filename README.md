# Unity-VideoPlay
卒業制作用のUnityプロジェクトVideoPlayのローカルリポジトリ

# タイトル：スマートフォン向けリズムゲームプレイヤーのためのハンドトラッキングアプリ制作

# 章立て
0. 目次
1. 序論
2. 関連研究と状況把握
3. システムの概要と設計方針
    1. システム概要
        1. 本アプリの目的
        2. 本アプリが有する機能と想定される利用形態
        3. システムに求められる要件と全体構造
            1. ハードウェア
            2. ソフトウェア
    1. システム設計（システムのブロック図を示し、各部に必要な条件・機能を示す）
        1. 具体的なシステム構造（各部の働きや具体的な装置名など）
        2. 各部の詳細と機能
            1. アプリのモード
                - 記録モード
                - 再生モード
            2. 2つのモードに共通する仕様
                - ハンドモデル
                - ディスプレイ
                - 視点移動（カメラ操作）
            3. 記録モードの機能
                - ハンドモデルの位置と回転のキャリブレーション
                - ハンドモデルの関節の回転のキャリブレーション
                - モーションデータの記録と保存
                - スマホ/タブレット端末のミラーリング
                - スマホ/タブレット端末の画面収録（未実装）
            4. 再生モードの機能
                - 記録モードで保存したモーションデータのロード
                - 外部からインポートした動画データのロード
                - モーションデータの再生と動画データの再生の同期
                - シークバー
                - スキップ
                - 再生速度倍率変更
4. システム構築
    1. 2つのモード
    2. ディスプレイ
    3. ハンドモデルとリンク機構
    4. 視点移動
    5. ハンドモデルの位置と回転のキャリブレーション
    6. ハンドモデルの関節の回転のキャリブレーション
    7. モーションデータの記録と保存
    8. スマホ/タブレット端末のミラーリング
    9. 記録モードで保存したモーションデータのロード
    10. 外部からインポートした動画データのロード
    11. シークバー
    12. モーションデータの再生と動画データの再生の同期
    13. モーションデータ再生位置のオフセット調整
    14. 再生速度倍率変更
    15. スキップ
5. 制作結果とその分析・評価
6. 今後の見直し
7. 参考文献
8. 謝辞
9. ソースコード