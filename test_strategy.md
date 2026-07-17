# EyeCenter テスト戦略

作成日: 2026-07-17
対象: EyeCenter（眼科クリニック業務アプリ / C# .NET Framework 4.8 WinForms / x86）

---

## 1. コード構造の分析結果

### 1.1 全体構造

本体プロジェクトは約 20,500 行（`.Designer.cs` 含む）。レイヤー分離のないフォーム中心アーキテクチャで、ビジネスロジックの大半がイベントハンドラ内に直接記述されている。

| 区分 | 主なファイル | 特徴 |
|---|---|---|
| エントリ/起動 | `Program.cs`, `MainForm.cs` | 単一インスタンス制御（WM_COPYDATA）、コマンドライン解析、初期化 |
| 画面管理 | `FormControl.cs` | 各フォームのシングルトン的な生成・表示を静的メソッドで管理 |
| 患者画面（中核） | `FormPat.cs`（3,817行） | 手術記録・検査・サマリー・IVの表示/登録を1クラスに集約した最大の複雑箇所 |
| 検索 | `FormFindOpeRecord.cs`, `FormFindKensa.cs`, `FormFindSummary.cs`, `SearchTask.cs` | 2段階取得＋件数上限方式。`SearchTask` がワーカースレッド＋専用DB接続を管理 |
| エクスポート | `FormExport.cs`, `FormCsvColumnSelect.cs`, `FormSumColumnSelect.cs` | キー順ページング（PAGE_SIZE=5000）でのCSVストリーム出力、CSVエスケープ、列選択 |
| Excel/バーコード | `ExcelControl.cs`, `Barcode128.cs` | Excel COM 相互運用、CODE128-C バーコード生成（36桁の文書トレーサビリティ値） |
| 印刷 | `InPrint1/2/3.cs`, `InPrintCommon.cs`, `FormPrint.cs` | 入院患者一覧・ワークシートの一括印刷。`GetSummaryValue` がサマリー内容の解析を担う |
| 検査UI部品 | `KensaPanel*.cs`, `KensaTabPage*.cs`, `ContrastPanel.cs`, `TenkeyPanel.cs` | 検査項目ごとの表示/入力コントロール群 |
| 設定 | `AppConfig.cs` | App.config の appSettings 読み取り（件数上限・タイムアウト等を制御） |

### 1.2 依存関係と結合度

- **MedicalLibrary への強依存（最大の制約）**: `DB` / `EyeOpe` / `EyeKensa` / `EyeSummary` / `StdClass` / `LoginUser` / `PatBase` 等のコアエンティティ・DBアクセスはすべて隣接リポジトリの `MedicalLibrary.dll` にある。本リポジトリ単体ではロジックの多くが「MedicalLibrary の戻り値を画面に整形する」処理。
- **静的結合**: `DB.Db2/Db3`、`FormControl`、`EyeDict`、`LoginUser` など静的状態への直接参照が多く、モック差し替えの継ぎ目（seam）がない。DBなしでインスタンス化できないフォームが大半。
- **外部リソース依存**: Oracle DB（DBリンク INNO.WORLD 経由の患者マスタ含む）、ネットワーク共有（Pat.csv）、Excel COM、外部機器EXE。
- **単体テスト可能な独立部品**: `Barcode128`、`AppConfig`、`ExcelControl` の設定読込/検証ロジック、`FormExport` の CSV 整形、`InPrintCommon.GetSummaryValue`、`FormSumColumnSelect.ParseCont/FixedValue`、`FormCsvColumnSelect` のファイルI/O系。`InternalsVisibleTo("EyeCenter.Tests")` は設定済み。

### 1.3 ビジネスロジックの重要度

医療業務アプリとして、誤りが患者安全・記録の正確性に直結する箇所:

1. **手術記録・予約の登録/更新/削除**（FormPat の各 RegButton、FormOpeRsvList の削除）
2. **検索結果の正確性**（検索条件→SQL組立、件数上限到達時の警告）
3. **エクスポートデータの完全性**（ページング境界での欠落・重複、CSVエスケープ）
4. **バーコード値の正確性**（36桁: 患者ID9+文書コード5+診療科3+入力者ID5+日付8+時刻6。読み取り誤りは文書の患者取り違えに直結）
5. **サマリー内容の解析**（`<CR+LF>`トークン、カンマ区切りKey-Value形式の相互変換）

### 1.4 エラーハンドリングの実装状況

- **良好**: `Program.Main`（Application.Run 前の例外を必ず表示）、`SearchTask`（ワーカー例外の捕捉・接続の確実な破棄・キャンセル時のSQLキャンセル）、`ExcelControl.ReleaseExcel`（null ガード付きCOM解放）、`FormExport`（finally でUI復帰）。
- **要注意（テストで挙動を固定すべき箇所）**:
  - `ExcelControl.loadSettings` は `IOException` のみ捕捉。不正値は既定値維持（仕様）だが、この「静かなフォールバック」は退行しやすい。
  - `FormCsvColumnSelect.LoadExclusion/SaveExclusion` は全例外を握りつぶす（設定消失が無通知）。
  - `FormExport` のSQLは文字列連結だが、連結値はページングで直前に自DBから読んだキー値のみであり外部入力ではない。ただし数値でないキー値が来た場合の挙動は未定義。
  - `MainForm.ExportButton_Click` の権限チェックがユーザーIDのハードコード比較。

---

## 2. テスト優先度別 対象一覧

### 【P0 – 最優先（必須）】

| 対象（クラス/メソッド） | 優先度の理由 | テスト手段 |
|---|---|---|
| `Barcode128.Draw`（CODE-C のシンボル分解・チェックディジット計算・開始/停止コード） | 医療文書のトレーサビリティ基盤。誤った描画はスキャナ読取不能または誤読＝患者取り違えリスク | 単体（Bitmap描画→ピクセル検証。既存テストあり、拡張） |
| `ExcelControl` のバーコード値組立（36桁の連結・PadLeft）と `insertBarcode` の入力検証（36桁数字以外は挿入しない） | データ整合性の核。桁ズレは全文書のID誤りに直結 | 単体（値組立の検証はリフレクション or 内部メソッド経由） |
| `ExcelControl.loadSettings` / `isAllDigits` / `generateBarcodeImage`（画像サイズ計算） | 設定ファイルの不正値で既定値を維持する契約が崩れると本番でバーコード劣化 | 単体（一時INIファイル。既存テストあり、拡張） |
| `FormExport.CsvCell` / `WriteCsvLine`（CSVエスケープ、改行→`<CR+LF>`トークン化） | エクスポートデータの整合性。エスケープ漏れは行ズレ＝レコード破壊 | 単体（static メソッドで外部依存なし） |
| `FormExport.SaveKensa/SaveOpe/SavePat/SaveRsv/SaveSummary` のページング継続条件（複合キー `>` 条件、`page_list.Count < PAGE_SIZE` での終了、件数集計） | ページ境界での欠落・重複はデータ移行/監査で致命的。同一キー多数件時の境界挙動が要検証 | 統合（ローカルOracle FREEPDB1 に境界データを投入して検証。ロジックのみ抽出できれば単体化を検討） |
| `SearchTask.Run`（正常完了/例外/中止の3経路、専用接続の破棄、200ms以内完了時のダイアログ抑止） | 全検索機能の共通基盤。接続リークや中止時の結果混入は全画面に波及 | 統合（ローカルDB）＋コードレビュー。`DB.Db2` 静的依存のため純粋な単体化は不可 |
| `InPrintCommon.GetSummaryValue`（Kind分岐、Code一致行の値復元、`<CR+LF>`復元、複数値のカンマ連結） | 印刷物に載るサマリー値の解析。誤解析＝誤った臨床情報の印字 | 単体（`EyeSummary` を組み立て可能なら単体、不可ならDataRowのみ偽装した準単体） |
| `FormSumColumnSelect.ParseCont` / `FixedValue`（Cont のKey-Value解析、重複キーは先勝ち） | サマリーCSV結合出力の整合性。`GetSummaryValue` と対の解析ロジック | 単体 |
| `MainForm.ExportButton_Click` の権限チェック | セキュリティ（個人情報一括出力の許可ユーザー限定） | 統合/手動（許可ID・非許可IDでの起動確認）。将来は許可リストの外部化を検討 |
| `FormPat` の登録系（`OpeRegButton_Click` / `RecordRegButton_Click` / `DoctorRegButton_Click` / `PassRegButton_Click`）と削除（`OpeHistoryDeleteMenuItem_Click`） | 手術記録＝業務の中核データの書込み。入力検証と保存内容の一致が最重要 | 統合＋UIAシナリオ（ローカルDBで登録→再読込→値一致を確認） |

### 【P1 – 高優先度（推奨）】

| 対象 | 理由 | テスト手段 |
|---|---|---|
| `AppConfig.GetInt`（未設定/空/非数値/0/負数→既定値、正数→採用） | 件数上限・DBタイムアウトを制御する設定処理。境界値が仕様そのもの | 単体（App.config の差し替えは不可のため、テスト用configまたは正常系のみ＋レビュー） |
| `MainForm.InitShow`（`-p` `-r` 引数解析、Pat.csv のID優先順位） | 起動モードの分岐。誤解析で別患者を開くリスク | 準単体（ロジック抽出可能なら）＋手動 |
| `Program.MainBody`（単一インスタンス判定、WM_COPYDATA での引数転送） | 二重起動時の挙動。障害時に画面が出ない系の不具合が過去に発生している領域 | 統合/手動（2プロセス起動） |
| `FormFindOpeRecord/FormFindKensa/FormFindSummary` の `ListShow`（検索条件の取り出し、`limit` 到達時の警告、結果のDataTable整形、`PreCheckBox` フィルタ） | 検索結果の正確性はユーザーの意思決定に直結。UIスレッド外からコントロール参照しない設計の維持も含む | 統合（ローカルDB）＋UIA |
| `FormCsvColumnSelect.FilterColumns` / `ColumnNames`（同名列の集約、逆順インデックスでの列削除、Title2との整合） | 列削除のインデックス操作は境界バグの温床（Title2 が短い場合など） | 単体（`TableData` は MedicalLibrary だが DB 不要で構築可能） |
| `FormCsvColumnSelect.LoadExclusion/SaveExclusion`、`FormSumColumnSelect.LoadSelection/SaveSelection` | 設定の永続化。全例外握りつぶしのため退行に気づきにくい | 単体（LocalApplicationData 配下の実ファイルI/O） |
| `FormOpeCal.RsvTableShow`（週送り・基準日からのカレンダー展開）、`PtTwinkle` | 予約カレンダーの日付計算。ズレは予約事故につながる | 統合＋UIA |
| `FormOpeRsvList`（一覧表示、削除確認、印刷） | 予約データの削除を含む | 統合＋UIA |
| `FormPat.CalcPassData` / `BodyCalc`（パス・体格の計算）、`PtOpeHistoryShow` / `PtKensaHistoryShow`（履歴整形） | 表示値の計算ロジック。複雑度が高い | 統合（ローカルDB） |
| `ExcelControl.MakeDocument` の全体フロー（テンプレート判定、共通情報シート書込み、全シートアクティブ化、別名保存） | Excel COM 連携の複雑な回避策（フォームコントロール脱落対策等）を含む初期化処理 | 手動/半自動（Excel実機。CI不可） |
| `KensaPanelDetail` / `KensaTabPage2` の `KensaShow/KensaClear/MoveNext/MovePrev` | 検査値の表示・入力ナビゲーション | 統合＋UIA |

### 【P2 – 中優先度（可能であれば）】

| 対象 | 理由 |
|---|---|
| `FormControl` の各 `*_Show`（シングルトン再利用、Disposed 判定、WindowState 復元） | 定型的だが `FormPat_Count` の IsDisposed 集計はロジックを含む。UIAで代替可 |
| `FormPat` の各 `*Clear` メソッド群（`OpeClear`, `RecordClear`, `PassClear` 等） | 単純なコントロール初期化。登録系シナリオテストで間接カバー |
| `ContrastPanel` / `TenkeyPanel` の入力補助 | UI部品。手動確認で十分 |
| `FormExport.WriteManifest`（検証用マニフェスト出力） | 単純なファイル出力。P0のエクスポート統合テスト内で間接検証 |
| `InPrint1.GetDict` / `InPrint2` / `InPrint3` のデータ取得・整形 | 印刷プレビューの手動確認と併用 |
| コピー系メニュー（`SightRCopyMenuItem_Click` 等のクリップボード操作） | 単純処理。手動確認 |

### 【P3 – テスト不要またはオプション】

第3章参照。

---

## 3. テストが不要な部分（理由付き）

| 対象 | 理由 |
|---|---|
| `*.Designer.cs` 全部（`InitializeComponent` 等） | Visual Studio 自動生成。手動編集しない規約であり、テストする価値がない |
| `*.resx`, `Properties/`（AssemblyInfo, Resources, Settings） | リソース定義・メタデータのみ |
| `obj/` 配下の生成ファイル | ビルド生成物 |
| `Barcode128` のコンストラクタ（barTable 定数テーブル） | 定数定義。`Draw` のテストで間接的に全数検証される（テーブルが誤っていれば描画結果が変わる） |
| `MainForm` の単純ボタンハンドラ（`PatButton_Click` 等、`FormControl.*_Show` への1行委譲） | 副作用のない薄い委譲。UIAスモークテストで起動確認すれば十分 |
| `CanonButton_Click` / `NidekButton_Click`（`Launcher.Start` の1行呼び出し） | 外部機器EXEの薄いラッパー。環境依存でテスト環境に実機がない |
| `FormBase.cs` / 各フォームの `CloseButton_Click`, `FormClosed` 等の定型ハンドラ | 明らかに副作用が単純（Close/Dispose のみ） |
| MedicalLibrary 内部のロジック（`EyeOpe.GetList` のSQL、`DB` クラス等） | 別リポジトリの責務。本リポジトリのテストでは「呼び出しと結果の整形」のみを対象とする（MedicalLibrary 側に別途テスト戦略が必要） |
| `SearchTask` のダイアログ外観（ラベル位置・サイズ等） | 純粋なUI構築。目視確認で十分 |

---

## 4. テスト戦略の詳細

### 4.1 テストレベルの構成（推奨ピラミッド）

```
        UIA/手動シナリオ（少数・リリース前）
          ├ 起動〜患者表示〜登録〜印刷のスモーク
          └ Excel COM・二重起動・権限チェック
      ─────────────────────────────
        統合テスト（ローカル Oracle FREEPDB1）
          ├ 検索3画面（SearchTask 経由）
          ├ エクスポートのページング境界
          └ FormPat 登録→再読込の一致
      ─────────────────────────────
        単体テスト（EyeCenter.Tests / MSTest / DB・COM不要）
          ├ Barcode128, ExcelControl設定（既存8件を拡張）
          ├ FormExport CSV整形, AppConfig
          └ InPrintCommon / ParseCont / FilterColumns
```

- 単体テストは既存の `EyeCenter.Tests`（MSTest, net48/x86, `InternalsVisibleTo` 設定済み）に追加する。実行手順は従来どおり「本体を x86 Debug でビルド → `dotnet test`」。
- 統合テストはローカル環境（FREEPDB1 のテストDB、患者1001〜1003）を前提とし、CI必須にはしない。
- UIA テストは WinForms ボタンが InvokePattern 非対応のため座標クリック方式（既知の制約）。

### 4.2 カバレッジ目標（現実的な補正付き）

| 優先度 | 原則目標 | 本アプリでの現実的な達成手段 |
|---|---|---|
| P0 | ライン・ブランチ 100% | **単体テスト可能な P0**（Barcode128.Draw, CsvCell, GetSummaryValue, ParseCont, loadSettings, isAllDigits）は 100% を必須とする。**DB/COM 埋め込みの P0**（SaveKensa 系, SearchTask.Run, 登録系）は統合テストでの主要3経路（正常・異常・中止/境界）網羅をもって代替し、ツールでのカバレッジ計測対象から分離する |
| P1 | 90%以上 | 単体化可能なもの（AppConfig, FilterColumns, 永続化系）のみ数値目標を適用。フォームロジックはシナリオ網羅（条件分岐の一覧表ベース）で管理 |
| P2 | 80%以上 | シナリオテストによる間接カバーを許容 |
| P3 | 対象外 | — |

注: フォーム内ロジック（イベントハンドラ埋め込み）に数値カバレッジを課すと、達成のために大規模リファクタリングが必要になる。本戦略では「新規・変更ロジックはフォーム外の testable なクラス/staticメソッドに置く」（`AppConfig`, `CsvCell`, `ExcelControl` が既にこのパターン）ことを規約とし、既存フォームの掘り起こしリファクタリングは行わない。

### 4.3 テストケース設計方針（P0 の具体例）

**Barcode128.Draw**
- 正常系: 36桁数字（CODE-C）で開始コード105・正しいチェックディジット・停止コードが描画される。CODE-A/B の開始コード分岐
- 異常系: 数字以外を含む文字列 → `false` を返し描画しない
- 境界値: 空文字列、奇数桁（CODE-C で最後の1桁が無視される現仕様の固定）、2桁（最小データ）、"00"/"99"（テーブル端）

**バーコード値組立（ExcelControl）**
- 正常系: 患者ID・文書コード・入力者IDの PadLeft を含め36桁になること、各フィールドの位置
- 異常系: 患者IDが9桁超 → 36桁にならず `insertBarcode` が挿入をスキップすること（取り違え防止の最後の砦）
- 境界値: 患者ID 1桁/9桁ちょうど、文書コード5桁ちょうど

**FormExport.CsvCell / WriteCsvLine**
- 正常系: 通常文字列の引用符囲み、カンマ含み、日本語（Shift-JIS/UTF-8 両エンコーディング）
- 異常系: `"` を含む（`""` へ）、`\r\n` / `\r` / `\n` 混在（すべて `<CR+LF>` へ）、`<CR+LF>` という文字列自体を含むデータ（往復変換で区別不能になる既知制約の文書化）
- 境界値: 空文字列、空セルのみの行、末尾カンマなしの確認

**エクスポートのページング（統合）**
- 正常系: PAGE_SIZE 未満 / ちょうど / 超の件数で全件・重複なし（マニフェストの件数と突合）
- 異常系: エクスポート中のフォームクローズ（`IsDisposed` で中断）、DB切断
- 境界値: 同一 (PATIENT_ID, KENSA_ID) で KENSA_DATE 多数件がページ境界をまたぐケース、キー値の最大値近傍

**SearchTask.Run（統合）**
- 正常系: 200ms 未満で完了する検索（ダイアログ非表示で結果取得）、長時間検索（ダイアログ表示→完了）
- 異常系: 検索内で例外 → メッセージ表示・null 返却・接続破棄、中止ボタン → null 返却・SQLキャンセル
- 境界値: 結果0件、limit ちょうど（警告表示の閾値 `>=`）

**InPrintCommon.GetSummaryValue / ParseCont**
- 正常系: Kind 1/3/4 の分岐、Code 一致行の値取得、複数値のカンマ再連結、`<CR+LF>` の復元
- 異常系: Kind が "2" や空（cont 空 → 空文字）、カンマなし行、Code 不一致
- 境界値: 重複 Code（最初の行が勝つ）、値が空（`"CODE,"`）、行区切り `\r` 単独 / `\n` 単独 / `\r\n`（空要素の発生）

### 4.4 実施順序

1. **フェーズ1（単体・即着手可能）**: P0 の単体テスト可能群を `EyeCenter.Tests` に追加（Barcode128 拡張、CsvCell/WriteCsvLine、GetSummaryValue、ParseCont/FixedValue、AppConfig）
2. **フェーズ2（単体・準備小）**: P1 の FilterColumns/ColumnNames、永続化系、loadSettings 網羅拡張
3. **フェーズ3（統合・ローカルDB）**: エクスポートのページング境界、SearchTask 3経路、検索3画面
4. **フェーズ4（UIA/手動・リリース前チェックリスト化）**: FormPat 登録シナリオ、Excel COM、二重起動、権限チェック
