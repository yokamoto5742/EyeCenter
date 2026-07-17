# EyeCenter

眼科クリニック向けの診療記録・患者管理業務アプリケーション。患者台帳、検査記録、手術記録、予約管理、診療サマリーを一元管理する Windows デスクトップアプリケーション。

## 主な機能

- **患者管理** — 患者基本情報、問診履歴、手術歴の一括管理
- **検査記録** — 眼科検査データ（視力、眼圧、屈折度など）の記録・閲覧
- **手術記録** — 手術内容、手術日程、執刀医情報の管理
- **予約管理** — 患者の診察・手術予約スケジューリング
- **診療サマリー** — 患者の診療履歴・治療経過の要約作成
- **Excel出力** — 手術記録・診療サマリー・検査データを Excel ファイルで出力（バーコード自動生成対応）

## システム要件

### ビルド環境

- **Windows** 10 以上（仮想環境での開発可能）
- **.NET Framework 4.8** ターゲティングパック
- **Visual Studio 2022 Community** または **MSBuild** 相当
- **MedicalLibrary プロジェクト** — このリポジトリの親ディレクトリにある隣接プロジェクト `MedicalLibrary` の DLL 出力物
- **Interop.Excel.dll** — Excel COM ライブラリの相互運用アセンブリ

### 実行環境

- **Windows 7 SP1** 以上
- **.NET Framework 4.8** 実行時環境
- **Oracle クライアント** 11.2 以上（本番環境）または **Oracle Instant Client** 11.2（開発環境、オプション）
- ネットワークドライブアクセス — `Pat.csv` など患者データ共有フォルダ
- **`C:\Shinseikai\EyeData\`** ディレクトリ — 実行可能ファイルと設定ファイルの配置先

> **注** : 眼科医療 DB、ネットワーク共有、外部機器用 EXE（Canon キャプチャ、Nidek デバイスなど）の詳細設定はリポジトリ外部の文書を参照してください。

## インストール

### 1. リポジトリのクローン

```bash
git clone <repository-url>
cd EyeCenter
```

### 2. 依存 DLL の配置

EyeCenter は `MedicalLibrary.dll` と `Interop.Excel.dll` を外部参照で使用します。これらを以下のフォルダに配置してください:

```
C:\Shinseikai\EyeData\
  ├── MedicalLibrary.dll        (隣接プロジェクト MedicalLibrary のビルド出力)
  └── Interop.Excel.dll         (社内共有から取得)
```

MedicalLibrary のビルド方法は隣接プロジェクトのドキュメントを参照してください。

### 3. ビルド

**Visual Studio を使用する場合:**
```bash
# EyeCenter.slnx をオープンして、Visual Studio でビルド
# Platform を x86 に設定してビルド
```

**コマンドラインで実行する場合:**
```bash
msbuild EyeCenter.sln /p:Configuration=Debug /p:Platform=x86
```

ビルド成果物は `bin\x86\Debug\` (Debug) または `bin\x86\Release\` (Release) に出力されます。

### 4. 配置（デプロイ）

ビルド後、以下のコマンドで実行環境に配置します:

```powershell
# Debug ビルド
.\deploy.ps1 -Configuration Debug

# Release ビルド（既定）
.\deploy.ps1
```

配置スクリプトは `EyeData.exe`、DLL、`.exe.config`、`.ini` ファイルを `C:\Shinseikai\EyeData\` にコピーします。本番環境用設定がある場合は、`EyeData.exe.config.production` も同時に配置されます。

## 使用方法

### アプリケーションの起動

```bash
# デバッグ用（ユーザーID 519 でログイン、リセットモード）
EyeData.exe -u 519 -r

# 通常起動（ユーザーID指定）
EyeData.exe -u <USER_ID>

# リセットモードなし
EyeData.exe -u <USER_ID>

# 患者を指定して起動
EyeData.exe -u <USER_ID> -p <PATIENT_ID>
```

### シングルインスタンス制御

同じアプリケーションの複数起動を防ぎます。既に起動している場合、コマンドラインパラメータは WM_COPYDATA を通じて既存ウィンドウに渡されます。

### 設定ファイル

**App.config** — ビルド時に自動生成。カスタマイズ可能な項目:

| 設定キー | 既定値 | 用途 |
|---------|-------|------|
| `MainFormWidth` | (画面サイズ) | メイン画面の幅（ピクセル） |
| `MainFormHeight` | (画面サイズ) | メイン画面の高さ（ピクセル） |
| `IVContBoxWidth` | 350 | 患者台帳・問診入力パネルの幅 |
| `IVHistoryViewWidth` | 342 | 患者台帳・問診履歴パネルの幅 |
| `OpeHistoryViewWidth` | 275 | 患者台帳・手術歴パネルの幅 |
| `KensaHistoryViewWidth` | (自動調整) | 患者台帳・検査歴パネルの幅 |
| `FindRowLimit` | 10000 | 検索（手術/サマリー/検査）取得件数上限 |
| `DbCommandTimeout` | 60 | DB コマンドのタイムアウト秒数 |

**EyeDataSettings.ini** — アプリケーション実行時の環境設定（リポジトリに含まれる）

**EyeData.exe.config.production** — 本番環境用設定（必要に応じて `EyeData.exe.config` に置き換える）

## プロジェクト構造

```
EyeCenter/
├── Program.cs                    # エントリポイント（シングルインスタンス制御）
├── MainForm.cs / Designer.cs     # メイン画面
├── FormPat.cs / Designer.cs      # 患者台帳フォーム
├── FormKensaHistory.cs           # 検査履歴フォーム
├── FormFindOpeRecord.cs          # 手術記録検索フォーム
├── FormOpeRsv.cs                 # 手術予約フォーム
├── FormFindSummary.cs            # サマリー検索フォーム
├── FormExport.cs                 # Excel エクスポートフォーム
│
├── Barcode128.cs                 # CODE128 バーコード描画エンジン
├── ExcelControl.cs               # Excel COM 操作（Interop.Excel）
├── SearchTask.cs                 # 検索タスク管理
├── ControlSumPage.cs             # サマリー入力コンポーネント
├── KensaPanel.cs                 # 検査データ表示パネル
│
├── App.config                    # ビルド設定（画面サイズ、DB タイムアウト等）
├── EyeDataSettings.ini           # アプリケーション設定
├── deploy.ps1                    # デプロイスクリプト
├── EyeData.exe.config.production # 本番環境設定
│
├── EyeCenter.csproj              # プロジェクトファイル（x86 プラットフォーム指定）
├── EyeCenter.slnx                # ビルド用 Solution ファイル
│
├── EyeCenter.Tests/              # ユニットテスト
│   ├── Barcode128Tests.cs        # Barcode128 単体テスト
│   ├── ExcelControlTests.cs      # ExcelControl 単体テスト
│   └── EyeCenter.Tests.csproj    # テストプロジェクト（SDK スタイル）
│
└── docs/                         # ドキュメント
    ├── BUILD_REQUIREMENTS.md     # ビルド環境チェックリスト
    ├── CHANGELOG.md              # 変更履歴
    ├── MIGRATION_PLAN.md         # マイグレーション計画
    └── PRINT_MIGRATION.md        # 帳票出力ライブラリ移行ガイド
```

### エンコーディング注意

**重要:** ソースファイルのエンコーディングが混在しています（Shift-JIS と UTF-8 BOM）。

- **既存ファイル** を編集する場合、ファイルのエンコーディングを変更しないでください。
- 新規ファイルは **UTF-8 BOM** を推奨します。
- Visual Studio で保存時に自動変換されないよう、以下を確認してください:
  - ファイル → 詳細保存オプション → エンコーディング

## 主要クラス・モジュール

### Barcode128 — バーコード生成

CODE128 フォーマットのバーコードを .NET Graphics に描画します。手術記録・診療サマリーの Excel 出力時に自動生成されます。

```csharp
// 例: 36 桁数字を CODE128-C で 80 ピクセル高さで描画
Barcode128 bc = new Barcode128();
bool result = bc.Draw(Barcode128.CODE.C, "000001001399110070051920260705123456", 
                       graphics, 30f, 0f, 80f, 3f);
```

**パラメータ:**
- `CODE` — CODE128 エンコード形式（A/B/C）
- `bar` — バーコード値（通常は 36 桁の数字）
- `graphics` — 描画先の Graphics オブジェクト
- `left, top` — 描画位置（ピクセル）
- `height, moduleWidth` — 高さとモジュール幅

### ExcelControl — Excel ファイル操作

`Interop.Excel` を使用して、Excel ファイルの生成・編集・保存を行います。

```csharp
// 例: テンプレートを基に Excel ファイルを新規作成
ExcelControl excel = new ExcelControl();
bool success = excel.CreateFromTemplate("template.xlsm", "output.xlsx");
```

テンプレートファイルパスは、以下の優先順で決定されます:
1. exe と同じフォルダ内の同名 `.xls*` ファイル（優先）
2. Oracle DB `OpeExcel` テーブルに登録されたフルパスの Excel テンプレート

### SearchTask — 非同期検索

大量データの検索（手術記録・サマリー・検査データ）を非同期実行し、タイムアウトを防ぎます。取得件数は `FindRowLimit` で制限。

## 開発情報

### テストの実行

テストは外部依存（DB・MedicalLibrary・Excel COM）を持たない `Barcode128` と `ExcelControl` のロジックのみをカバーします。

```bash
# Step 1: 本体を Debug でビルド（必須）
msbuild EyeCenter.sln /p:Configuration=Debug /p:Platform=x86

# Step 2: テスト実行
dotnet test EyeCenter.Tests/EyeCenter.Tests.csproj
```

テストプロジェクト（`EyeCenter.Tests`）は本体 `.sln` には含まれていません（本体ビルドに NuGet 復元を持ち込まないため）。

### ビルド時の注意

- **プラットフォーム指定** — `Platform=x86` を忘れずに指定してください（`EyeCenter.slnx` の既定）。
- **MSBuild使用** — `dotnet build` は非 SDK スタイルの `.csproj` に非対応のため、`msbuild` を使用してください。
- **依存 DLL の確認** — ビルド前に `C:\Shinseikai\EyeData\` に `MedicalLibrary.dll` と `Interop.Excel.dll` が配置されていることを確認してください。

### 外部依存

このアプリケーションは実行時に以下の外部リソースに依存します:

- **Oracle 医療 DB** — 患者・検査・手術・予約情報の読み書き
- **ネットワーク共有** — `Pat.csv` などの患者マスタデータ
- **外部機器 EXE** — キャプチャ装置ドライバ（`CanonRKF1.exe` など）、手術スケジューラ（`OpeOrder.exe`）

これらは環境依存のため、ローカルでの開発では整備されていない可能性があります。詳細は隣接プロジェクト `MedicalLibrary` のドキュメントを参照してください。

## トラブルシューティング

### ビルドエラー: MedicalLibrary.dll が見つからない

**原因:** `C:\Shinseikai\EyeData\MedicalLibrary.dll` が存在しない

**解決方法:**
1. 隣接プロジェクト `MedicalLibrary` をビルドする
2. 生成された DLL を `C:\Shinseikai\EyeData\` にコピーする
3. 再度ビルドを実行

### ビルドエラー: Interop.Excel.dll が見つからない

**原因:** `C:\Shinseikai\EyeData\Interop.Excel.dll` が存在しない

**解決方法:**
- 社内共有または IT から `Interop.Excel.dll` を取得し、`C:\Shinseikai\EyeData\` に配置

### 起動時エラー: LoginUser が初期化されていない

**原因:** Oracle DB への接続に失敗、または起動引数 `-u <USER_ID>` が不正

**解決方法:**
1. `App.config` の `DbCommandTimeout` を確認（既定値 60 秒）
2. 起動引数に正しいユーザーID を指定: `EyeData.exe -u 519`
3. Oracle クライアント接続設定を確認（本番環境の場合）

### 起動時エラー: Pat.csv が見つからない

**原因:** ネットワーク共有へのアクセス失敗、または共有フォルダの接続不具合

**解決方法:**
1. ネットワークドライブのマウントを確認
2. Pat.csv が配置されているパス（MEMORY.md を参照）を確認
3. ファイアウォール・VPN 接続を確認

### Excel 出力エラー: テンプレートファイルが見つからない

**原因:** Excelテンプレート（`オペ録.xlsm` など）が exe と同じフォルダにない

**解決方法:**
- デプロイスクリプト実行時に Excel テンプレートファイルも `C:\Shinseikai\EyeData\` にコピーしてください
- または、Oracle DB の `OpeExcel` テーブルに正しいフルパスが登録されているか確認

### 日本語文字が文字化けする

**原因:** Shift-JIS ファイルを UTF-8 で保存した

**解決方法:**
- `.Designer.cs`, `.resx` は Visual Studio 自動管理のため直接編集しない
- その他のファイル編集時は、以下で既存エンコーディングを確認・維持:
  - Visual Studio: ファイル → 詳細保存オプション → エンコーディング確認
  - または `git show HEAD:<file>` で確認

## ライセンス

このプロジェクトのライセンスは `docs/LICENSE` を参照してください。

## 関連ドキュメント

- **docs/BUILD_REQUIREMENTS.md** — ビルド環境チェックリスト
- **docs/CHANGELOG.md** — 変更履歴
- **docs/MIGRATION_PLAN.md** — アーキテクチャマイグレーション計画
- **docs/PRINT_MIGRATION.md** — 帳票出力ライブラリ移行ガイド
- **CLAUDE.md** — 開発者向け AI ガイドライン（文字コード、フォーム構成、ビルド方法など）
