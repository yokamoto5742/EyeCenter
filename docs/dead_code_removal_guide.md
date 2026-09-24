# C# GUIアプリのデッドコード削除ガイド

WinForms / WPF などの C# デスクトップアプリで、デッドコードを**安全に・確実に**削除するための手順。
Agree リポジトリ（.NET Framework 4.8 / WinForms / 旧形式 csproj）で実施した作業（v1.1.2）をもとに、他のリポジトリでも使える形にまとめた。

## 1. 基本方針

1. **候補を出す → GUI特有の参照を除く → 小さく削除 → ビルドとテスト、実際の画面で確認**、の順で進める。
2. アナライザーの結果だけで判断しない。**検出器が本当に動いているかを最初に確かめる**（3章）。
3. 1コミットには1カテゴリだけを入れる（参照 / ファイル / 画面コントロール など）。失敗したらそのコミットだけ戻す。
4. 削除する前に「残す理由」を探す。消してよい根拠がそろわない限り残す。

## 2. 事前調査

| 確認項目 | 見る場所 | 理由 |
|---|---|---|
| csproj の形式 | `<Project Sdk=...>` か `ToolsVersion=...` か | 旧形式では `dotnet build` で IDE ルールが動かないことがある |
| ビルド方法 | VS のフル MSBuild / `dotnet build` | COM 参照や resx の扱いが違う。**両方でビルドできるなら両方で確認する** |
| テストの参照方法 | テスト csproj の `ProjectReference` / `Compile Link` | ファイル単位で取り込んでいると、テストが見ているのは本体の一部だけ |
| 外部環境が必要なテスト | DB 結合テストなど | 常に実行できるテストだけを絞り込む（`--filter`）|
| 既存の `.editorconfig` | リポジトリ直下 | 重複した設定や競合を避ける |
| ベースライン | Debug / Release のビルド結果と警告数 | 変更後に比べるため |

フル MSBuild の例（VS 2026 Community）:

```bash
MSB="/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
"$MSB" App.csproj -t:Rebuild -p:Configuration=Release -p:Platform=x86 -nologo -v:m -clp:Summary
```

## 3. 候補の検出

### 3.1 コンパイラとアナライザー

注目するルール:

| ルール | 内容 | 注意 |
|---|---|---|
| CS0169 / CS0414 / CS0219 / CS0162 | 未使用フィールド / 代入のみのフィールド / 未使用変数 / 到達不能コード | コンパイラ警告なので、どのビルド方法でも出る |
| IDE0051 / IDE0052 | 未使用の private メンバー / 読まれない private フィールド | **ビルド時には出ない環境がある** |
| IDE0059 / IDE0060 / IDE0035 | 不要な代入 / 未使用パラメーター / 到達不能コード | IDE0060 はイベントハンドラーやインターフェース実装を除いて考える |
| IDE0058 | 戻り値を使っていない式 | デッドコードではないので対象外 |

#### 検出器の動作確認（必須）

「警告0件」は、実際に未使用コードが無いのか、検出器が動いていないのか区別できない。**わざと未使用コードを書いたファイルをコンパイルし、警告が出ることを確かめる**。

```csharp
namespace App
{
    internal class Probe
    {
        private int unusedField;                                          // CS0169 / IDE0052
        private void UnusedMethod(int unusedParam) { int x = 1; x = 2; } // IDE0051 / IDE0060 / CS0219
    }
}
```

> Agree での結果: 旧形式 csproj では `dotnet build` で IDE ルールがまったく動かなかった。SDK 形式の解析用プロジェクトでも、出たのは CS0169 / CS0219 だけで、IDE0051 / 0060 は出なかった。**このためアナライザーは補助にとどめ、3.2 の参照数カウントを主な手段にした。**

#### 旧形式 csproj を解析するための一時プロジェクト

リポジトリは変えずに、スクラッチ用フォルダに SDK 形式のプロジェクトを作り、本体のソースを取り込んで解析する。

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <OutputType>WinExe</OutputType>
    <PlatformTarget>x86</PlatformTarget>
    <UseWindowsForms>true</UseWindowsForms>
    <LangVersion>latest</LangVersion>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="C:/path/to/repo/App/*.cs" />
    <Compile Include="C:/path/to/repo/Properties/*.cs" />
    <Compile Include="Probe.cs" />
    <GlobalAnalyzerConfigFiles Include=".globalconfig" />
    <Reference Include="C:/path/to/repo/External.dll" />
    <Reference Include="Microsoft.CSharp" />   <!-- dynamic を使うなら必要 -->
    <Reference Include="System.Windows.Forms" />
  </ItemGroup>
</Project>
```

```ini
# .globalconfig（.editorconfig はソースの場所でしか効かないので globalconfig を使う）
is_global = true
global_level = 100
dotnet_diagnostic.IDE0051.severity = warning
dotnet_diagnostic.IDE0052.severity = warning
dotnet_diagnostic.IDE0059.severity = warning
dotnet_diagnostic.IDE0060.severity = warning
```

ハマりどころ:
- 別フォルダの `.editorconfig` を `EditorConfigFiles` で追加すると **CS8700**（同じフォルダに構成ファイルが複数ある）になる。globalconfig を使う。
- `dynamic` を使っているコードは `Microsoft.CSharp` の参照が無いと **CS0656** になる。

### 3.2 参照数のカウント（主な手段）

宣言ごとに、リポジトリ全体での出現回数を数える。**1回（宣言だけ）なら候補**。2回なら、もう1回がどこにあるか（Designer / テスト / 自分自身）を確かめる。

```bash
export LC_ALL=C
for f in App/*.cs; do
  case $f in *Designer.cs) continue;; esac
  grep -nE "^\s*(public|private|internal|protected|static)[^=;(]*\s([A-Za-z_][A-Za-z0-9_]*)\s*(\(|;|=|\{|$)" "$f" \
  | grep -vE "\bclass\b|\benum\b" | while IFS= read -r line; do
    n=$(echo "$line" | sed -E 's/^([0-9]+):.*\s([A-Za-z_][A-Za-z0-9_]*)\s*(\(|;|=|\{|$).*/\2/')
    c=$(grep -rwo --include=*.cs --include=*.resx --include=*.xaml --include=*.config --include=*.ini "$n" . | wc -l)
    [ "$c" -le 2 ] && echo "$c $f:${line%%:*} $n <- $(grep -rlw --include=*.cs "$n" . | tr '\n' ' ')"
  done
done
```

- 名前だけで数えるので、同じ名前の別シンボルがあると候補から漏れる。**漏れる方向の誤差**なので安全側。
- アクセス修飾子の無い宣言は拾えない。必要に応じて正規表現を足す。
- 「テストからしか参照されない public メンバー」も一覧にして確認する（本体では不要な可能性がある）。

### 3.3 画面コントロール（WinForms）

`*.Designer.cs` にあるのに、Designer 以外のコードから参照されないコントロールを一覧にし、`Visible = false` かどうかも表示する。

```bash
for d in App/*.Designer.cs; do
  base=${d%.Designer.cs}
  grep -oE "^\s*(private|internal|public) [A-Za-z.]+ [A-Za-z0-9_]+;" "$d" | awk '{print $3}' | tr -d ';' | while read c; do
    n=$(ls $base.cs ${base}.*.cs | grep -v Designer | xargs cat | grep -wo "$c" | wc -l)
    vis=$(grep -q "this.$c.Visible = false" "$d" && echo HIDDEN)
    [ "$n" -eq 0 ] && echo "$d: $c $vis"
  done
done
```

- 普通のラベルやボタンもたくさん出てくる。**`HIDDEN` で、イベント登録も無いもの**が本命の候補。
- partial ファイル（`Form1.*.cs`）を数えるとき、`Form1.Designer.cs` を含めないように注意する。
- 非表示のラベルは、対になる入力欄が使われていれば見た目は変わらないので、Designer の差分を増やしてまで消す価値は低い（Agree では対象外にした）。

### 3.4 プロジェクト構成とファイル

| 対象 | 確認方法 |
|---|---|
| アセンブリ参照 | 名前空間や型名（`HttpClient`, `XDocument`, `AsEnumerable` など）をソースから検索 |
| COM 参照（Office.Core / VBIDE など） | `MsoTriState` などの型の使用を検索。`dynamic` 経由で数値を渡していれば不要 |
| `Properties/Settings.settings` | `<Settings />` が空で `Settings.Default` が使われていなければ不要 |
| `Properties/Resources.resx` | `<data>` がテンプレートのコメント内だけで、`Properties.Resources` が使われていなければ不要 |
| 用途不明のファイル（`*.udl` など） | csproj / コード / ドキュメントから参照があるか。**認証情報を含むなら git 管理から外す** |
| ini / config のキー | 各キーがコードの文字列として参照されているか |

## 4. 削除してはいけないもの（除外チェックリスト）

すべての候補について、次のどれにも当てはまらないことを確認する。

- [ ] `*.Designer.cs` の `InitializeComponent` でのイベント登録（`xxx.Click += ...`）
- [ ] `*.resx` のリソース名、`DataBindings.Add(...)`、`BindingSource`
- [ ] WPF の場合: XAML の `Click=` / `Command=` / `{Binding}` / `x:Name` / Converter / Resource / DataContext
- [ ] リフレクション（`GetMethod` / `Invoke` / `Type.GetType`）、`nameof`、文字列での呼び出し
- [ ] シリアライザーや ORM が使う DTO のプロパティ、SQL の列名との対応
- [ ] DI 登録、属性を手がかりにした自動検出、プラグイン
- [ ] public / protected API、他のプロジェクトやテストからの参照
- [ ] COM オブジェクトの解放処理（`Marshal.ReleaseComObject` など）。**参照が少なく見えても消さない**
- [ ] インターフェース実装やイベントハンドラーのシグネチャで必要な引数

「テストで通らないコードだから」は削除の理由にならない。

## 5. 削除の進め方

1. 候補を表にまとめ、判定（削除可 / 要確認 / 削除禁止）をつけてユーザーに確認する。
   - 仕様の名残か、今後使う予定があるかは**コードからは判断できない**。必ず聞く。
2. 次の順番でカテゴリごとにコミットする（安全なものから）。

| 順番 | カテゴリ | 検証 |
|---|---|---|
| 1 | 未使用のアセンブリ参照・COM 参照 | 両ビルド方法 + Excel などの COM 連携を実際に動かす |
| 2 | 空の Settings / Resources、不要ファイル | 両ビルド方法 + 単体テスト |
| 3 | 非表示の未使用コントロール | 両ビルド方法 + デザイナーで開く + 画面操作 |
| 4 | 未使用のメソッド・フィールド | 両ビルド方法 + 単体テスト + 該当機能の操作 |

3. Designer の編集は、できれば VS デザイナーで行う。手で編集するときは次の **4か所だけ**を消し、差分が削除行だけであることを `git diff` で確認する。
   - フィールド宣言 / `new` の行 / `Controls.Add` の行 / プロパティを設定しているブロック
4. 最後に、ドキュメント（設計書・README）に削除したものが書かれていないか検索する。

## 6. 検証

変更前と各コミットのたびに:

```bash
# フル MSBuild（COM 参照を解決する）
"$MSB" App.csproj -t:Rebuild -p:Configuration=Debug   -p:Platform=x86 -nologo -v:m -clp:Summary
"$MSB" App.csproj -t:Rebuild -p:Configuration=Release -p:Platform=x86 -nologo -v:m -clp:Summary
# dotnet CLI
dotnet build App.slnx -c Debug --no-incremental
# 外部環境が要らないテストだけ
dotnet test App.Tests/App.Tests.csproj --filter "FullyQualifiedName!~IntegrationTests"
# 消したシンボルが残っていないか
grep -rnE "removedName1|removedName2" --include=*.cs --include=*.resx --include=*.xaml --include=*.md .
```

人が行う最終確認:
- VS デザイナーで対象フォームを開き、エラーが出ないこと、レイアウトやタブ順が変わらないこと
- 主な操作（新規作成・保存・削除・印刷・画面の切り替え）
- COM 参照を外した場合は、Excel などへの出力を実機で確認する

## 7. 作業環境でのハマりどころ（Windows + Git Bash）

| 現象 | 対策 |
|---|---|
| ビルドログに `grep` すると `Binary file matches` になる | `grep -a` を使い、`LC_ALL=C` を設定する |
| 日本語を含む heredoc をコマンドに埋め込むと、コミットやファイルが壊れる | コミットメッセージは一時ファイルに書き、`git commit -F file` で渡す |
| `printf` / `>>` で追記した日本語が別のエンコーディングになる | 追記はエディターや編集ツールで行い、`file` コマンドで UTF-8 のままか確かめる |
| Python の正規表現に書いた `\\` がシェルを通るとエスケープとして崩れる | csproj の修正は、文字列を完全一致で置き換える編集ツールで行う |
| csproj を書き換えると BOM や改行コードが変わる | 書き換えた後に `file` と `git diff --stat` で確かめる |

## 8. AI エージェントに依頼するときのプロンプト例

```text
この C# GUI アプリのデッドコードを監査してください。この段階ではファイルを編集しないでください。
- csproj の形式とビルド方法（フル MSBuild / dotnet）を確認し、両方で変更前のビルド結果を記録する
- わざと未使用コードを書いたファイルで、アナライザーが実際に警告を出すか確認する
- 宣言ごとの参照数カウント、Designer の未参照コントロール、未使用のプロジェクト参照、
  空の Settings / Resources、用途不明ファイルを調べる
- docs/dead_code_removal_guide.md の除外チェックリストを全候補に適用する
- 結果を「パス / シンボル / 根拠 / 動的参照リスク / 判定」の表で出し、
  仕様上の判断が必要なものは質問としてまとめる
```

```text
監査で合意した候補だけを、カテゴリごとに1コミットずつ削除してください。
各コミットの前に、フル MSBuild（Debug/Release）、dotnet build、外部環境の要らないテストを実行し、
差分が対象の削除だけであることを確認してください。失敗したらそのコミットは戻して報告してください。
```

## 付録: Agree での実施結果（v1.1.2）

| カテゴリ | 結果 |
|---|---|
| 未使用の private メンバー・変数 | 0件（以前の整理で解消済み） |
| アセンブリ参照 | System.Net.Http / System.Deployment / System.Xml.Linq / System.Data.DataSetExtensions / System.Xml を削除 |
| COM 参照 | Microsoft.Office.Core / VBIDE を削除（`AddPicture` は dynamic 経由のため不要） |
| ファイル | 空の Settings.settings / Resources.resx と自動生成コードを削除。test.udl は `.gitignore` に追加 |
| 画面 | 非表示で未使用の眼選択チェックボックス3つを削除。非表示ラベルは見た目が変わらないため残した |
| 検証 | フル MSBuild（Debug/Release）と dotnet build で警告0・エラー0、単体テスト15件合格 |
