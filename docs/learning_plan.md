# EyeData コードベース学習プラン

**前提**: C# は初めて / 他言語での実務経験あり / 週末のみ・期限なし
**最終目標**: 別の電子カルテへ接続できるようにする（接続先は未定 → 「差し替え可能な構造」を作ることがゴール）
**改修範囲**: EyeCenter + MedicalLibrary 両方

---

## 全体像（先に把握しておく地図）

```
EyeCenter.exe (17,000行 / 画面がほぼ全部)
    ↓ 参照
MedicalLibrary.dll (Entity 80+ / Boundary 100+ / Agent / Utility)
    ↓
Oracle DB ─┬─ EYE_* テーブル ……… EyeData 自前のデータ（移行しても残る）
           └─ D_* / M_* / L_* …… 電子カルテ(INNO)のデータ（← 差し替え対象）
```

**重要な発見**: DBテーブルは2グループにきれいに分かれている。

| 自前（残す） | カルテ側（差し替える） |
|---|---|
| EYE_KENSA, EYE_KENSA2, EYE_OPE, EYE_OPE_RECORD, EYE_OPE_RSV, EYE_OPE_DOCTOR, EYE_OPE_PASS, EYE_SUMMARY, EYE_INTERVIEW | D_NYUIN(25), D_ORDER_HEADER(14), D_UKETSUKE(12), D_NYUIN_NOW(12), D_YOYAKU(7), M_PATIENT, M_USR, M_DR, M_DEPT ほか約35テーブル |

括弧内は参照箇所数。**「カルテ側テーブルを触っている全箇所」を洗い出して抽象化する**のが改修の本体です。

### カルテとの接続点は5つ

| # | 接続点 | 実装場所 |
|---|---|---|
| 1 | Oracle 直結 | `MedicalLibrary/Utility/DB.cs`（静的シングルトン `Db1/Db2/Db3`）、`Entity/StdEntity.cs`（`Db = Db3`）、`Entity/StdClass.cs`（SQL実行の唯一の入口） |
| 2 | 患者選択の受け渡し | `Entity/PatBase.cs:701 ReadPatCSV()` — カルテが書く共有フォルダの `pat.csv`(50列)を読む |
| 3 | ログインユーザー | `Entity/LoginUser.cs`、`Entity/Staff.cs`、起動引数 `-u` |
| 4 | カルテ画面の操作 | `Utility/InnoProgram.cs` — UIAutomation で `InnoKarte.exe` のウィンドウを直接操作（呼び出しは `FormPat.cs:214, 1540` の2箇所のみ） |
| 5 | 受付ライブラリ | `InnoUketsukeLib.dll`（`LibSettings.cs:491`、`Staff.cs:378`。実行時オプション扱い） |

---

## Phase 0 — 足場を作る（週末 1〜2）

**目標**: 自分の手でビルドし、デバッガで止められる。

| ステップ | 検証 |
|---|---|
| MedicalLibrary を x86 Debug でビルド | `MedicalLibrary.dll` が生成される |
| EyeCenter を x86 Debug でビルド | `bin\x86\Debug\EyeData.exe` が生成される |
| VS でデバッグ実行（引数 `-u 519`） | 起動する |
| `MainForm.cs` の `MainForm_Load` にブレークポイント | そこで停止する |
| F10（ステップオーバー）/ F11（ステップイン）/ ローカル変数ウィンドウ | `LibSettings.Init()` の中に F11 で入れる |

**同時に学ぶ VS の概念**: ソリューション / プロジェクト / 参照(HintPath) / 構成(Debug・Release) / プラットフォーム(x86)。
`EyeCenter.csproj` を**テキストエディタで開いて眺める**こと。参照DLLの解決先が書いてあります。

> 補足: 環境構築の実績は `CLAUDE.md` と過去のメモに記録済み。ビルドが通らない場合はまず `docs/BUILD_REQUIREMENTS.md` を参照。

---

## Phase 1 — C# 固有の文法だけ（週末 2〜4）

他言語経験があるので、変数・ループ・関数は飛ばします。**このコードベースを読むのに必要な C# 固有の9項目**だけ:

| # | 項目 | このリポジトリでの実例 |
|---|---|---|
| 1 | 値型 / 参照型、`struct` vs `class`、`null` | `WinAPI.COPYDATASTRUCT`（struct）、`ref` 渡し |
| 2 | **プロパティ**（`get`/`set`） | `Entity/StdKarte1.cs` の `Pat`、`RegDateTimeShort` — 「フィールドに見えるが実行されるコード」 |
| 3 | **`static` とシングルトン** ★最重要 | `DB.Db1/Db2/Db3`、`StdEntity.Db`、`LoginUser.Status`、`Env.DB_LINK`。**このアプリの状態はほぼ全部グローバル静的**。ここを理解しないと何も読めない |
| 4 | **`partial class`** | `FormPat` は5ファイルに分割（`FormPat.cs` / `.Ope.cs` / `.Kensa.cs` / `.Excel.cs` / `.Designer.cs`）。全部で1つのクラス |
| 5 | 継承と `override` | `MainForm : StdForm1`、`MainForm.WndProc` の override |
| 6 | デリゲートとイベント | `button.Click += PatButton_Click`（`.Designer.cs` に配線が書いてある） |
| 7 | ジェネリクス `List<T>` / `Dictionary<K,V>` | `StdClass.DataDict`（`Dictionary<string, object>`）、`List<StdClass>` |
| 8 | 例外 `try`/`catch`、`using` 文 | `MainForm_Load` の `catch (Exception ex) { LibUtility.Except(ex); }` |
| 9 | 名前空間と `using` ディレクティブ | 各ファイル冒頭の `using MedicalLibrary.Entity;` など |

**検証課題（手を動かす）**:
1. 新規に小さな WinForms アプリを作り、ボタンを押すとテキストが変わる → イベントを理解
2. `List<自作クラス>` を `DataGridView` に `DataSource` でバインドして表示
3. `static` フィールドを持つクラスを作り、2つのフォームから同じ値を読み書きできることを確認 → シングルトンの体感

**この段階で読むべき本物のコード**: `Program.cs`（86行）、`MainForm.cs`（193行）、`AppConfig.cs`（小さい）。全部読み切れるサイズです。

---

## Phase 2 — WinForms と ADO.NET（週末 4〜6）

### WinForms
- フォームのライフサイクル: `Load` → `Shown` → `FormClosing` → `Dispose`
- `.cs` / `.Designer.cs` / `.resx` の3点セット。**Designer.cs はVSが自動生成するので手で編集しない**（`CLAUDE.md` の規約）
- 使われているコントロール: `TextBox` / `DataGridView` / `TabControl` / `Panel` / `UserControl`
- 自作コントロールの例: `KensaPanel.cs`, `TenkeyPanel.cs`, `ContrastPanel.cs`（比較的小さくて読みやすい）

### ADO.NET（DBアクセス）
読む順:
1. `MedicalLibrary/Utility/DB.cs`（248行） — `OracleConnection` / `OracleCommand` / `Open`/`Close`
2. `MedicalLibrary/Entity/StdClass.cs` — **SQL実行の唯一の入口**。`GetList(DB db, string sql, List<StdDbColumn> param_list)` が全エンティティから呼ばれる
3. `MedicalLibrary/Entity/PatBase.cs:496 Load()` — 典型パターン:
   ```
   SQL文字列を組み立て → StdClass.GetList(DB.Db3, cmd) → GetFromStdClass() でオブジェクトに詰め替え
   ```

**検証**: ローカルの Oracle(FREEPDB1) に対して、自作の小さなコンソールアプリから `M_PATIENT` を1行 SELECT できる。

> 注意点として押さえる: `DB` クラスは接続とコマンドを1個ずつ共有する設計で**スレッドセーフではない**。`Open`/`Close`/`Parameters.Clear` の順序が壊れると動かない。非同期検索が専用接続を使っている理由（`SearchTask.cs`）もここ。

---

## Phase 3 — アプリの縦の1本を通す（週末 6〜10）

**目標**: 「患者を開いて氏名が画面に出るまで」に通る関数を、順番に説明できる。

追跡する経路:
```
Program.Main
  → 単一インスタンス判定（WM_COPYDATA でパラメータを既存ウィンドウへ送る）
  → Application.Run(new MainForm())
      → MainForm_Load
          → LibSettings.Init()      … Setting.xml から接続文字列 → DB.Db1/2/3.Init()
          → DB.SetCommandTimeout()
          → EyeDict.Init() / FormControl.Init()
          → InitShow(args)
              → LoginUser.Init()          … ← 接続点3
              → PatBase.ReadPatCSV()      … ← 接続点2
              → FormControl.FormPat_Show(pat_id)
                  → FormPat（1,990行 + partial 3本）
                      → PatBase.Load(pt_id)
                          → StdClass.GetList(DB.Db3, "select * from M_PATIENT ...")  … ← 接続点1
```

週末ごとの区切り:

| 週 | 読む対象 | 検証 |
|---|---|---|
| 6 | `Program.cs` → `MainForm.cs` 全体 | 単一インスタンス制御の仕組みを図に描ける |
| 7 | `LibSettings.Init()` / `DB.cs` / `Env.cs` | 接続文字列がどこから来るか説明できる |
| 8 | `LoginUser.cs` / `PatBase.cs` | `-u 519` がどう使われるか追える |
| 9 | `FormPat.cs`（画面の初期化部分のみ） | ブレークポイントを5箇所置いて起動順を確認 |
| 10 | `FormPat.Ope.cs` / `FormPat.Kensa.cs` | 手術タブ・検査タブがどのテーブルを読むか列挙できる |

**やり方のコツ**: 読むだけでなく **ブレークポイントを置いて実際に止め、呼び出し履歴（コールスタック）ウィンドウを見る**。1,990行を上から読むのは非効率です。

---

## Phase 4 — カルテ境界の棚卸し（週末 10〜13）

ここからが改修の準備です。**コードは1行も変えず、調査と文書化だけ**を行います。

### 成果物1: カルテ依存箇所の一覧表

作り方（実際に使えるコマンド）:
```bash
# カルテ側テーブルを参照している全箇所
grep -rn "D_NYUIN\|D_ORDER_HEADER\|D_UKETSUKE\|D_YOYAKU\|M_PATIENT\|M_USR\|M_DR\|M_DEPT" --include=*.cs ..\MedicalLibrary .
```
列: `ファイル:行` / `テーブル` / `読み or 書き` / `何の業務のためか` / `新カルテで代替可能か`

### 成果物2: 5つの接続点それぞれの「入出力仕様」

各接続点について「**何を入力し、何を返すか**」を、Oracle や INNO という言葉を使わずに書き直す。これがそのまま将来のインターフェース定義になります。

例:
- 接続点2（pat.csv）→ 「**現在カルテで選択中の患者ID・氏名・性別・生年月日・入外区分・診療科・担当医・保険を取得する**」
- 接続点4（InnoProgram）→ 「**指定した患者をカルテ側の画面で開く**」（成否を bool で返す）

### 成果物3: 接続先カルテへの確認事項リスト

接続先が未定なので、**選定時に必ず確認すべき項目**を先に用意しておく。これが Phase 5 の設計を左右します:

- [ ] DBへの直接読み取りは許諾されるか（**ベンダー保証外になることが多い**。現行のINNO直結と同じ手が使える保証はない）
- [ ] DB種別（Oracle / SQL Server / PostgreSQL）とバージョン
- [ ] 患者選択の連携方法はあるか（ファイル / 起動引数 / ウィンドウメッセージ / API）
- [ ] ユーザー認証の連携方法
- [ ] 標準規格の対応（HL7 FHIR / SS-MIX2 / CSV出力）
- [ ] カルテ画面を外部から開く手段（コマンドライン引数など）
- [ ] EYE_* テーブルをどこに置くか（新カルテのDBに同居 / 別DBを立てる）

> **この確認事項リストを Phase 4 の最初に作り、接続先候補が挙がった時点で照会に出す**のが実務上いちばん効きます。答えによって Phase 5 の作業量が数倍変わります。

---

## Phase 5 — 差し替え可能にする（週末 13〜）

原則: **「動作を変えない改修」と「新カルテ対応」を絶対に混ぜない**。

### 5-1. 現行INNO実装を「実装クラス」として括り出す（動作は一切変えない）

インターフェースを定義し、中身は今のコードをそのまま移すだけ。

```
IKartePatientSource   … 患者情報の取得（現行: pat.csv + M_PATIENT）
IKarteUserSource      … ログインユーザー（現行: LoginUser + M_USR）
IKarteWindowControl   … カルテ画面操作（現行: InnoProgram）
IKarteDataSource      … 入院・受付・予約・オーダー（現行: D_* テーブル）
```

**検証**: 括り出しの前後でアプリの挙動が同一。既存の `EyeCenter.Tests` が通り続ける。

着手順は依存の少ない方から: `IKarteWindowControl`（呼び出し2箇所）→ `IKartePatientSource` → `IKarteUserSource` → `IKarteDataSource`（最大）。

### 5-2. DBアクセスをOracle固有から剥がす

`StdClass.GetList` が SQL 実行の唯一の入口なので、ここが最大の梃子。`OracleConnection`/`OracleCommand`/`OracleDbType` を `DbConnection`/`DbCommand`/`DbType`（`DbProviderFactory`）に置き換えると、DB製品の差し替えが可能になります。

**ただし**: x86 縛りは 32bit Oracle クライアントに由来します。Oracle から離れられればこの制約も外せます（`CLAUDE.md` 参照）。

### 5-3. 設定で切り替えられるようにする

`EyeDataSettings.ini` / `App.config` に「どのカルテ実装を使うか」を持たせ、起動時に実装クラスを選ぶ。

### 5-4. 新カルテ用の実装クラスを書く

ここまで来て初めて、接続先固有のコードを書きます。Phase 4 の成果物2（入出力仕様）がそのまま実装の仕様書になります。

---

## この計画の使い方

- **Phase 0〜2 は飛ばさない**。特に Phase 1 の項目3（static/シングルトン）と項目4（partial class）を理解しないと、Phase 3 で必ず詰まります。
- **Phase 3 の「検証」列を満たせないまま次に進まない**。読んだ気になるのが一番の罠です。
- **Phase 4 の確認事項リストは、接続先が決まる前に作る**。決まってから作ると照会が後手に回ります。
- 各 Phase で読んだ内容は、このファイルの隣にメモとして残すこと（`docs/` 配下）。

## 参考: 最初に読むべきファイル（サイズ順・小さい方から）

| ファイル | 行数 | 内容 |
|---|---|---|
| `Entity/StdEntity.cs` | 12 | `Db = Db3` の1行。全エンティティの親 |
| `AppConfig.cs` | 小 | 設定読み取り |
| `Program.cs` | 86 | エントリポイント |
| `Utility/Env.cs` | 127 | 環境パスと DB リンク |
| `MainForm.cs` | 193 | メイン画面 |
| `Utility/DB.cs` | 248 | DB接続の中核 |
| `Entity/StdClass.cs` | — | SQL実行の唯一の入口 |
| `Entity/LoginUser.cs` | 538 | ログイン |
| `Entity/PatBase.cs` | 1,033 | 患者基本情報 |
| `FormPat.cs` (+3 partial) | 1,990+ | 最大の画面。最後に読む |
