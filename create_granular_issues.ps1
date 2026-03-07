$ErrorActionPreference = "Stop"
$env:GH_PROMPT_DISABLED = 1
chcp 65001

function Create-Issue {
    param([string]$Title, [string]$Body)
    $Body | gh issue create --title $Title -F -
}

# Day 1
Create-Issue -Title "[Day1] メイン画面UIの基本レイアウト構築" -Body "Canvasを用いた基本画面の構築。上部にインベントリ、中央に人形表示領域、下部にナビゲーション領域を配置し、ダミーのパネルとボタンを設置する。"
Create-Issue -Title "[Day1] マスタデータ設計（ScriptableObject）" -Body "MaterialDataやMagicRecipeDataなどのScriptableObjectスキーマ定義を行い、動作確認用の適当なテストデータを投入する。"
Create-Issue -Title "[Day1] セーブ/ロードシステムの基盤実装" -Body "PlayerSaveData（所持金、素材、解放済み魔法）およびDollSaveDataをJSON形式でローカルに保存・読み込みできる基盤クラスを作成する。"

# Day 2
Create-Issue -Title "[Day2] タイマーカウントダウンのコアロジック実装" -Body "Time.unscaledDeltaTimeなどを利用した、作業時間（25分）と休憩時間（5分）のカウントダウン処理および状態管理（Work/Break/Idle）を実装する。"
Create-Issue -Title "[Day2] バックグラウンド復帰時の時間経過補償ロジック" -Body "アプリの一時停止（スリープ、別アプリ遷移）時に現在時刻を保存し、復帰時に経過時間を算出してタイマー残量に反映するロジックを実装する。"
Create-Issue -Title "[Day2] タイマー状態のUI・プログレス表示処理" -Body "カウントダウンの残り時間をテキストUIで表示し、円形のプログレスバーなどに反映する処理を実装する。"

# Day 3
Create-Issue -Title "[Day3] 簡易タスク入力欄と完了検知" -Body "今回の作業内容を入力する1行のテキストボックスを用意し、タイマー開始・終了と連動させる。"
Create-Issue -Title "[Day3] タイマー完了時の報酬算出・付与ロジック" -Body "タイマーが正常に完了した際に、固定またはランダムで「お金」と「基本素材」をリザルトとして算出する処理を実装。"
Create-Issue -Title "[Day3] インベントリデータ更新・保存とUI同期" -Body "算出した報酬をセーブデータに加算し、上部UIの所持金・素材数表示を同期更新させる。"

# Day 4
Create-Issue -Title "[Day4] 魔法研究（Magic）パネルのUI実装" -Body "研究可能な魔法のリストを表示するUIを作成し、必要素材等の情報を表示する。"
Create-Issue -Title "[Day4] 魔法研究の条件判定・素材消費・解放ロジック" -Body "素材やお金が足りているか判定し、不足時はボタンを非活性化する。実行可能な場合は素材を消費し、セーブデータの「アンロック状態」を更新して保存する。"

# Day 5
Create-Issue -Title "[Day5] ホーム画面での人形表示（仮スプライト対応）" -Body "ホーム画面中央に仮の立ち絵（またはスプライト）を表示する機構を用意する。（今後の着せ替え拡張を見据え、パーツ分割を想定したPrefab構造にしておくとなお良し）"
Create-Issue -Title "[Day5] 人形タップ時のランダム台詞・吹き出しUI表示" -Body "人形をタップした際に性格・Moodなどのデータを参照し、適切なセリフをランダムに吹き出しUIとしてポップアップさせる機能。"

# Day 6
Create-Issue -Title "[Day6] 各種UI操作へのSE（効果音）追加" -Body "ボタンクリック音やタイマー開始・終了音などの簡単なSEを追加し、操作のフィードバックを強化する。"
Create-Issue -Title "[Day6] 報酬獲得・魔法完了時の簡易パーティクル（VFX）追加" -Body "完了時の報酬ポップアップや魔法アンロック時に、UnityのParticle System等を用いた簡単な視覚的演出を追加する。"
Create-Issue -Title "[Day6] 全体のカラーパレット・デザイン調整" -Body "デフォルトUIから、全体的に「Cozy & Cute」な色合いになるようカラーコードやフォントを設定・調整する。"

# Day 7
Create-Issue -Title "[Day7] 25分通しプレイでの通しテストとバグ修正" -Body "実際に25分作業＋5分休憩のサイクルを回し、進行不能バグや表示の崩れがないかチェック・修正する。"
Create-Issue -Title "[Day7] アプリ中断・再起動時のデータ整合性・タイマー復帰テスト" -Body "アプリキルや長時間のバックグラウンド放置後でも、セーブデータやタイマーの進行時間が破綻していないかを重点的にテストする。"

Remove-Item -Path $MyInvocation.MyCommand.Path -Force
