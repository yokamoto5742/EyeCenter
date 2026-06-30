# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

眼科クリニック向けの業務アプリ。C# / .NET Framework 4.8 の WinForms デスクトップアプリ（WinExe, 単一インスタンス制御に WM_COPYDATA を使用）。患者(Pat)・検査(Kensa)・手術記録(Ope)・予約(Rsv)・サマリーを扱う。

## 文字コード（重要）

ソースファイルのエンコーディングが混在している（Shift-JIS のファイルと UTF-8 BOM のファイルが混在）。**編集時はそのファイルの既存エンコーディングを必ず維持すること。** Shift-JIS のファイルを UTF-8 で保存すると日本語のコメント・文字列が文字化けする。

## MedicalLibrary 依存（重要）

コアロジックの大半はこのリポジトリには無く、隣接プロジェクト `..\MedicalLibrary\MedicalLibrary` にある。`LoginUser` / `FormControl` / `PatBase` / `EyeDict` / `LibSettings` / `WinAPI` / `LibUtility` や `MedicalLibrary.Boundary` / `.Entity` / `.Agent` / `.Utility` 名前空間の定義はこのリポジトリ内を探しても見つからない。`Interop.Excel` も外部パス参照。ビルドには MedicalLibrary のビルド成果物が必要。

## フォームの構成

各フォームは3点セット: `X.cs`（ロジック）+ `X.Designer.cs`（VS自動生成）+ `X.resx`（リソース）。`.Designer.cs` と `.resx` は Visual Studio が管理するため手動編集は避ける。

## ビルド

- Visual Studio で `EyeCenter.sln` を開いてビルド、または CLI: `msbuild EyeCenter.sln /p:Configuration=Debug /p:Platform=x86`
- 既定プラットフォームは **x86**（`EyeCenter.slnx` 参照）
- 旧形式（非SDKスタイル）の `.csproj` のため `dotnet build` ではなく `msbuild` を使う
- テストプロジェクトは存在しない

## 実行時の前提

起動時に医療DB・ネットワーク共有（患者データ, `Pat.csv` など、いずれもリポジトリ外）に接続する。外部機器EXE（`CanonRKF1.exe`, `NidekARK1.exe`）や OpeOrder を起動する箇所がある。これらは環境依存のため、ローカルでの実行は環境が整っていないと動かない。

## 規約

コミット・コーディング・応答スタイルの規約は `.claude/rules/`（`commit.md`, `coding-guidelines.md`, `response-style.md`）に定義済み。これらは自動で読み込まれる。
