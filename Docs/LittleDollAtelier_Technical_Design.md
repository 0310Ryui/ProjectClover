# Little Doll Atelier: Unity Technical Design (MVP)

このドキュメントでは、MVPの要件に基づいたUnityでの具体的な設計と実装方針（シーン構成、マネージャー群、スクリプトの責務、セーブデータ構造、UIフロー）を定義します。ソロ開発での保守性と拡張性を重視したアーキテクチャです。

## 1. シーン構成 (Scenes)
MVPおよび今後の拡張を見据え、**シングルシーン・アーキテクチャ**を採用します。ロード時間をなくし、Cozyな没入感を維持するためです。

*   **`00_Boot` (初期化用シーン / オプション)**
    *   **役割**: アプリ起動時に一度だけ読み込まれ、グローバルなマネージャーの初期化、セーブデータのロード、アセットの事前読み込みを行います。
    *   **動作**: 処理完了後、自動的に `01_Main` を追加ロード（LoadSceneMode.Additive）します。
*   **`01_Main` (メインゲームシーン)**
    *   **役割**: ゲームの全てのコアロジックとUIが存在するシーンです。
    *   **構成**:
        *   `Environment`: 背景や3Dモデル/2Dスプライトのルート。
        *   `Doll`: お人形のオブジェクト群。
        *   `UI_Canvas`: 全てのUIパネル（Home, Timer, Magicなど）を内包するCanvas。
        *   `Managers`: 各種システムを管理するGameObject群。

## 2. マネージャー群とスクリプトの責務 (Managers & Responsibilities)
シングルトンパターン、またはService Locator/DI（VContainer等）を用いて各マネージャーを疎結合に保ちます。（MVP段階ではシンプルなシングルトンでも可）

### システム系マネージャー
*   **`GameManager`**
    *   **責務**: ゲーム全体の状態（Booting, Home, Working, MagicCrafting等）の管理と、各マネージャーの初期化順序の制御。
*   **`DataManager`**
    *   **責務**: セーブデータのロード、保存（JSONシリアライズ/デシリアライズ）。オートセーブのスケジューリング。
*   **`TimeManager`**
    *   **責務**: 実時間の計測とバックグラウンド処理。
    *   **機能**: `OnApplicationPause` / `OnApplicationFocus` を監視し、アプリが裏に回った時刻と復帰時刻の差分を計算。タイマーの経過時間に合算する。
*   **`AudioManager`**
    *   **責務**: BGM（魔法完成でアンロックされる楽曲）とSEの再生管理。クロスフェード処理。

### ゲームロジック系マネージャー
*   **`PomodoroManager`**
    *   **責務**: ポモドーロタイマーのコアロジック。
    *   **機能**: 状態（Stop, Work, Break）の管理、残り時間の計算、完了時のイベント発火。`TimeManager` と連携して正確な時間を刻む。
*   **`ResourceManager` (または InventoryManager)**
    *   **責務**: お金（Money）と素材（Materials）の所持数管理。増減ロジックと、変動時のUI更新イベントのブロードキャスト。
*   **`DollManager`**
    *   **責務**: お人形のデータ（性格、Mood、関係値）の管理と、状態変化の処理。
    *   **機能**: タイマー完了時や魔法研究時の結果を受けて、Moodを変化させる。タッチされたときのリアクション（台詞など）の決定処理。
*   **`MagicResearchManager`**
    *   **責務**: 魔法レシピのマスタデータ参照と、研究の進捗管理、素材消費ロジック、アンロック処理。

## 3. UIフローとUIスクリプトの責務 (UI Flow & Scripts)
画面遷移はCanvas配下のPanelの表示/非表示（アクティブ切り替え、またはCanvasGroupのAlpha操作）で実装します。

### UIフロー（ステートマシンまたはスタックベース）
1.  **Home View**: デフォルト状態。人形が中央に表示され、上部にリソース、下部にナビゲーションボタン（Timer, Magic 等）。
2.  **→ Timer View**: ナビゲーションから遷移。画面中央に大きくタイマーUIが表示される。Homeの背景は暗くなるかぼける。
3.  **→ Magic View**: ナビゲーションから遷移。レシピ一覧と必要素材、研究ボタンが表示される。

### 主要UIスクリプト
*   **`UIManager`**
    *   **責務**: 現在アクティブなViewの管理、View間の遷移アニメーション（DOTween等の利用を推奨）、バックボタン階層の管理。
*   **`HomeView`**
    *   **責務**: リソース量（お金、素材）のUIバインディング。人形の吹き出しテキストの表示トリガー。
*   **`TimerView`**
    *   **責務**: `PomodoroManager` の時間を購読し、円形プロゲージとテキストで残り時間を描画。タスク入力フィールドの管理。Start/Give Upボタンの入力受付。イベント完了時のリザルトポップアップ表示。
*   **`MagicView`**
    *   **責務**: `ResourceManager` と `MagicResearchManager` を参照し、各レシピのクラフト可能状態（ボタンの活性/非活性）の描画。進捗ゲージの更新。

## 4. セーブデータ構造 (Save Data Structure)
将来の拡張を見据えたC#のクラス/構造体の定義例です。JSONのシリアライゼーションを前提としています（`[System.Serializable]` 付与、Newtonsoft.Json等の利用）。

```csharp
[System.Serializable]
public class SaveData
{
    public string lastSaveTime; // ISO8601形式。バックグラウンド時間の計算用フォールバック
    public PlayerProfile playerProfile = new PlayerProfile();
    public InventoryData inventory = new InventoryData();
    public DollData activeDoll = new DollData(); // MVPでは1体。将来は List<DollData> 
    public ProgressionData progression = new ProgressionData();
    public TimerSessionData currentTimerSession = new TimerSessionData(); // 中断復帰用
}

[System.Serializable]
public class PlayerProfile
{
    public int money;
}

[System.Serializable]
public class InventoryData
{
    // 素材IDと所持数のマッピング。Unityの標準JSONUtilityはDictionaryをサポートしないため、リストか自作ラッパーを使用。
    // 例: public List<ItemSlot> materials = new List<ItemSlot>();
    public Dictionary<string, int> materials = new Dictionary<string, int>(); 
}

[System.Serializable]
public class DollData
{
    public string id;
    public string name;
    
    // Personality Stats (0-100)
    public int curiosity;
    public int diligence;
    public int sociability;
    public int emotion;
    public int caution;
    public int confidence;
    
    // Relationship Stats
    public int trust;
    public int affection;
    public int dependency;
    
    // Current State
    public string currentMood; // Enum(Energy, Calm...)を文字列として保存
    public string equippedOutfitId; // MVPでは未使用想定だが枠だけ用意
}

[System.Serializable]
public class ProgressionData
{
    // 魔法の進捗（ID -> 進捗度）
    public Dictionary<string, int> magicResearchProgress = new Dictionary<string, int>();
    // 完全にアンロック済みの魔法・楽曲リスト
    public List<string> unlockedMagics = new List<string>();
    public List<string> unlockedMusics = new List<string>();
}

[System.Serializable]
public class TimerSessionData
{
    public bool isRunning;
    public string sessionType; // "Work" or "Break"
    public float remainingSeconds;
    public string currentTaskName;
    public string lastTickTime; // タイマーが最後に動いた時刻（裏に行く直前の時刻）
}
```

## 5. ScriptableObject（静的データ）の構造
変更されないマスタデータはScriptableObjectとして実装し、アセットとして保存します。

```csharp
[CreateAssetMenu(fileName = "MagicRecipe", menuName = "Data/MagicRecipe")]
public class MagicRecipeData : ScriptableObject
{
    public string recipeId;
    public string recipeName;
    public int requireMoney;
    public List<MaterialRequirement> requiredMaterials;
    public int requiredResearchPoints; // 完成までに必要なトータルの進捗ポイント
    
    public AudioClip unlockedMusic; // 完成時に解放される楽曲
}

[System.Serializable]
public struct MaterialRequirement
{
    public string materialId; // または MaterialData の参照
    public int amount;
}
```

## 開発の推奨順序
この設計に基づき、まずは **`TimeManager`（バックグラウンド時間の計算）と `PomodoroManager`（タイマー状態の管理）の連携** をプロトタイピングし、UI（`TimerView`）で正確に時間が減ることを確認する作業から着手することを強く推奨します。これがアプリの心臓部となります。
