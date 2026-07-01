# EyeCenter ビルド・実行環境調査結果（2026-07-01時点）

## ビルド条件

| # | 条件 | 現状 |
|---|---|---|
| 1 | Visual Studio Community 2026 (18.7.2) または同等のMSBuild | ✅ インストール済み（`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`） |
| 2 | .NET Framework 4.8 ターゲティングパック | ✅ インストール済み |
| 3 | Platform=x86、`EyeCenter.slnx` を使用 | ✅ 確認済み（旧`.sln`は階層が古く、現在のリポジトリ構成と一致しないため使用不可） |
| 4 | 隣接プロジェクト `MedicalLibrary` のソース（`..\MedicalLibrary`） | ✅ 存在するが未ビルド |
| 5 | `C:\Shinseikai\MedicalLibrary.dll` を配置 | ❌ 現在未配置。MedicalLibraryをビルドし、その出力DLLを手動配置する運用 |
| 6 | `C:\macs\utility\Interop.Excel.dll` を配置 | ❌ 現在未配置。社内共有等から取得して配置する運用 |

`EyeCenter.csproj` はこの2つのDLLを `<Reference HintPath>` で参照しており（`<ProjectReference>` ではない）、ソリューションをビルドしても自動生成・自動コピーはされない。5・6が揃わない限り、`msbuild EyeCenter.slnx /p:Configuration=Debug /p:Platform=x86` は参照解決エラーで失敗する。

## 実行時条件

| # | 条件 | 現状 |
|---|---|---|
| 7 | Oracle DB接続設定 | ✅ 設定済み |
| 8 | テスト用DB・データ | ❌ 未作成（作成予定） |
| 9 | 起動引数 `-u <ユーザーID> -r`（`LoginUser.Init(true, args)` が解釈） | 要確認（テスト時に使うユーザーIDは別途要確認） |
| 10 | 外部機器EXE（`CanonRKF1.exe`, `NidekARK1.exe`、`Launcher.Start()`経由）、OpeOrder起動（`Launcher.OpeOrder()`） | 環境依存・未確認。実体はリポジトリ外 |

7・8が揃うまでは、ビルドが通っても実際のログイン・患者検索等のフロー動作確認はできない。

## 補足

- 医療DB・ネットワーク共有（`Pat.csv` 等）への接続先・認証情報は本リポジトリ内には存在せず、`MedicalLibrary` 側の `LibSettings.Init()` / `LoginUser.Init()` が保持している。
- `EyeCenter.sln`（旧形式）は `EyeCenter\EyeCenter.csproj` や `..\MedicalLibrary\MedicalLibrary\MedicalLibrary.csproj` という、現在のフラット化されたリポジトリ構成とは異なるパスを参照しており使用不可。ビルドには `EyeCenter.slnx` を使用すること。
