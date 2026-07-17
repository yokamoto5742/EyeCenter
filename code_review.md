# EyeCenter コードレビュー

対象: リポジトリ内の全 C# ソース（`*.Designer.cs` を除く約 15,000 行）
観点: 可読性・メンテナンス性の向上、KISS原則によるシンプル化

## 総評

動作実績のある業務アプリとして完成度は高く、特に近年追加されたコード（`SearchTask.cs`, `ExcelControl.cs`, `FormExport.cs`, `AppConfig.cs`, `InPrintCommon.cs`）は XML ドキュメントコメントと「なぜそうするか」の説明が丁寧で、手本になる品質です。

一方、レガシー部分には **同一ロジックのコピー＆ペーストが大量に存在** し、これが可読性・保守性の最大の課題です。代表例は「`code,value` 形式（`<CR+LF>` トークン付き）の Cont 文字列のパース／組み立て」で、ほぼ同じ 20 行のブロックが **11 ファイル・37 箇所以上** に散在しています。修正が必要になった場合、全箇所を漏れなく直すのは現実的に困難です。

以下、優先度順に指摘します。**リファクタリング時の注意: ソースは Shift-JIS と UTF-8 BOM が混在しているため、編集時は既存エンコーディングの維持が必須です（CLAUDE.md 参照）。また大半のコードにテストがないため、まず純粋ロジック（Cont パーサ等）を抽出してテストを付けてから置き換える手順を推奨します。**

---

## 優先度: 高

### 1. Cont 形式のパース／組み立てロジックの重複（最重要）

`"code,value"` を改行区切りで持つ Cont 文字列を Dictionary に展開する次のパターンが、リポジトリ全体で繰り返されています。

```csharp
foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
{
    string[] s = line.Split(',');
    if (tmpDict.ContainsKey(s[0]))
    {
        tmpValue = "";
        for (int i = 1; i < s.Length; i++)
        {
            if (tmpValue.Length > 0) tmpValue += ",";
            tmpValue += s[i].Replace("<CR+LF>", "\r\n");
        }
        tmpDict[s[0]] = tmpValue;
    }
}
```

主な出現箇所:

- `FormPat.cs` — `PtKensaHistoryShow()` 内に **7 回**（KensaId 1/3/5/7/8/9/31 の分岐ごと。`FormPat.cs:1725`, `1818`, `1857`, `1892`, `1929`, `1974`, `2019`）、`PtOpeHistoryShow()`（`FormPat.cs:1514`）、`RecordShow()`（`FormPat.cs:2248`）、`PassShow()`（`FormPat.cs:2344`）
- `ControlSumPage.cs` — `Show()` 内に 3 回（`ControlSumPage.cs:726`, `807`, `843`）
- `FormFindOpeRecord.cs:375`, `395`（`MakeTableData()`）
- `InPrintCommon.cs:44`（こちらは共通化済みの好例）

逆方向（コントロール → Cont 文字列）の組み立ても同様に `FormPat.cs:3221`（RecordRegButton）、`FormPat.cs:3324`（PassRegButton）、`KensaTabPage.cs:248`、`ControlSumPage.cs` の Save 内 3 回で重複しています。

**提案:** 静的ヘルパークラスを 1 つ作り、全箇所を置き換える。

```csharp
/// <summary>Cont 形式（"code,value" 行 + &lt;CR+LF&gt; トークン）の相互変換。</summary>
static class ContData
{
    /// <summary>Cont 文字列を code → value の Dictionary に展開する（先勝ち）。</summary>
    public static Dictionary<string, string> Parse(string cont)
    {
        var dict = new Dictionary<string, string>();
        foreach (string line in cont.Split('\r', '\n'))
        {
            string[] s = line.Split(',');
            if (s.Length >= 2 && !dict.ContainsKey(s[0]))
            {
                dict.Add(s[0], string.Join(",", s, 1, s.Length - 1).Replace("<CR+LF>", "\r\n"));
            }
        }
        return dict;
    }

    /// <summary>パネル内の TextBox/ComboBox/CheckBox を Cont 文字列に組み立てる。</summary>
    public static string Build(Control.ControlCollection controls) { /* 省略 */ }
}
```

これは外部依存のない純粋ロジックなので `EyeCenter.Tests` でテスト可能です。この 1 件だけで数百行の削減と、フォーマット仕様の一元化（現状は箇所ごとに「先勝ち／後勝ち」「`<CR+LF>` 置換の有無」が微妙に揺れている）が実現します。

### 2. FormPat.cs の神クラス化（3,817 行）

`FormPat` は患者情報表示・手術基本情報・手術記録タブ・経過記録・検査・サマリー・点滴・Excel 出力の全責務を 1 ファイルで担っています。`ControlSumPage` / `ControlIVPage` に切り出す方針は既にあるので、同じパターンで続きを進めるのが低リスクです。

- `PtKensaHistoryShow()`（`FormPat.cs:1663`–2087、約 425 行）→ 検査履歴の行構築を別クラスへ。指摘 1 のパーサ導入後は KensaId ごとの分岐が「マッピングだけ」になり大幅に縮みます
- `ExcelOpen()`（`FormPat.cs:2421`–2863、約 440 行）→ 指摘 3 参照
- `RecordTabControlInit()` / `PassPanelControlInit()`（合わせて約 480 行）→ 指摘 6 の動的コントロール生成ヘルパーへ

C# 的には `partial class FormPat`（例: `FormPat.Kensa.cs`, `FormPat.Ope.cs`）で機械的に分割するだけでも、目的のコードを探すコストが大きく下がります。

### 3. ExcelOpen() の item1〜item30 の逐語的繰り返し

`FormPat.cs:2438`–2846 で、以下の 5 行ブロックが 30 回近く繰り返されています。

```csharp
EyeDoc.Item item2 = new EyeDoc.Item();
item2.Kind = "手術基本情報";
item2.Name = "手術日";
item2.Value = this.OpeDateTimePicker.Value.ToString("yyyy/MM/dd");
tmpDoc.ItemList.Add(item2);
```

**提案:** ローカルヘルパーで 1 行化。CheckBox → "1"/"0" 変換も吸収する。

```csharp
void AddItem(List<EyeDoc.Item> list, string kind, string name, string value)
{
    list.Add(new EyeDoc.Item { Kind = kind, Name = name, Value = value });
}
// 使用側
AddItem(tmpDoc.ItemList, "手術基本情報", "手術日", OpeDateTimePicker.Value.ToString("yyyy/MM/dd"));
```

さらに RecordTabControl の TabPages[0]/[1]/[2]、SumPanel1/3/4 に対する「コントロールを列挙して Item 化」するループ（`FormPat.cs:2609`–2805）は Kind 名以外同一なので、`AddPanelItems(panel, kind)` として 1 つにまとめられます。約 440 行 → 100 行以下が見込めます。

### 4. コントロール種別判定 `GetType().Name.Equals("TextBox")`

7 ファイル・56 箇所で使われています（`FormPat.cs` 22 箇所、`ControlSumPage.cs` 18 箇所ほか）。

```csharp
// 現状
if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
// 提案
if (c is TextBox || c is ComboBox)
```

`is` 演算子は型安全（タイプミスがコンパイルエラーになる）で短く、意図も明確です。機械的に置換可能でリスクの低い改善です。CheckBox のキャストも `if (c is CheckBox cb)` は C# 7 以降の構文のため、対象言語バージョンが古い場合は `is` 判定 + 既存キャストの維持で十分です。

---

## 優先度: 中

### 5. 「表示中フォームの取得 or 生成 → Show → Activate」定型の重複（FormControl.cs）

`FormControl.cs:61`–158 で `FormPat_Show` 系 4 メソッドがほぼ同一の生成・表示処理を持ち、`FormOpeRsv_Show` 以下 5 メソッドも「null か未生成なら new → Show → Activate → 最小化解除」の定型を繰り返しています。

**提案:**

```csharp
static FormPat GetOrCreateFormPat()
{
    FormPat f = FormPat_List.Count > 0 ? FormPat_List[0] : null;
    if (f == null || !f.Created)
    {
        f = new FormPat();
        FormPat_List.Add(f);
    }
    return f;
}

static void ShowFront(Form f)
{
    f.Show();
    f.Activate();
    if (f.WindowState == FormWindowState.Minimized)
    {
        f.WindowState = FormWindowState.Normal;
    }
}
```

なお引数なしの `FormPat_Show()`（`FormControl.cs:61`）だけ `!f.Created` チェックがなく、他の 3 つと挙動が異なります。意図的でなければ統一を推奨します。

### 6. 動的コントロール生成コードの重複

DataRow から Label/TextBox/ComboBox/CheckBox を組み立てる処理が、`FormPat.RecordTabControlInit()`, `FormPat.PassPanelControlInit()`, `ControlSumPage.Init()`（SumItem1/2/3/4 の 4 ループ）, `KensaPanelDetail.ControlShow()` にそれぞれ実装されています。特に以下の Ime / Align の if-else 連鎖はほぼ全箇所に出現します。

```csharp
if (r["Ime"].ToString().Equals("Hiragana")) tmpBox.ImeMode = ImeMode.Hiragana;
else if (r["Ime"].ToString().Equals("Disable")) tmpBox.ImeMode = ImeMode.Disable;
else if (r["Ime"].ToString().Equals("Off")) tmpBox.ImeMode = ImeMode.Off;
else tmpBox.ImeMode = ImeMode.NoControl;
```

**提案:** まず共通の小物ヘルパー（`ApplyIme(Control, string)`, `ApplyAlign`, `ApplySize(DataRow, Control)`) を導入するだけで各生成メソッドが 3〜4 割縮みます。完全な共通ファクトリ化は各所の差分（イベント購読など）が多いため、無理に統一せず段階的で構いません。

また `ControlSumPage.Init()` の SumItem2 処理（`ControlSumPage.cs:201`–286）は `Line == "1"` と `"2"` の分岐が座標変数以外完全に同一です。座標を変数化すれば 1 本になります。

### 7. OpeInit() の同型ループ 10 連（FormPat.cs:375–469)

```csharp
OpeRoomBox.Items.Clear();
OpeRoomBox.Items.Add("");
foreach (DataRow r in EyeDict.EyeSet.Tables["OpeRoom"].Rows)
{
    OpeRoomBox.Items.Add(r["Value"].ToString());
}
```

これが対象テーブル名だけ変えて 10 回続きます。**提案:**

```csharp
void FillCombo(ComboBox box, string tableName)
{
    box.Items.Clear();
    box.Items.Add("");
    foreach (DataRow r in EyeDict.EyeSet.Tables[tableName].Rows)
    {
        box.Items.Add(r["Value"].ToString());
    }
}
```

### 8. InRoomBoxChange() の分岐（FormPat.cs:1253–1287)

「外来」「わかば」「ふれあい」「せせらぎ」の 4 分岐が、Select のフィルタ文字列以外同一です。入力値をそのまま使えば分岐自体が不要です。

```csharp
InRoomBox.Text = "";
InRoomBox.Items.Clear();
InRoomBox.Items.Add("");

foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = '" + InOutBox.Text + "'"))
{
    InRoomBox.Items.Add(r["Value"].ToString());
}
```

同様に `InDateChange()`（`FormPat.cs:1238`）の if-else は 1 行にできます:
`InDateTimePicker.Enabled = InOutBox.Text.Equals("わかば") || InOutBox.Text.Equals("ふれあい") || InOutBox.Text.Equals("せせらぎ");`

### 9. PtOpeHistoryWide() の 2 分岐がほぼ同一（FormPat.cs:1555–1658)

幅 300 超／以下の 2 ブランチは、`手術` 列幅（220/145）、`医師` 列幅（55/30）、OpeHistory 由来列の表示可否しか違いません。共通部分をくくり出し、差分 3 点だけ条件分岐にすれば半分になります。

### 10. 日付ラベル計算の重複（FormPat.cs）

`PassPanelControlInit()`（`FormPat.cs:742`–762）と `PassPanelControlDateChange()`（`FormPat.cs:982`–1002）に AddDays/AddMonths/AddYears の同一計算があります。`string CalcPassDate(DataRow r, DateTime opeDate)` として抽出すれば 1 箇所になり、両者の将来的な乖離も防げます。

### 11. KensaTabPage / KensaTabPage2 / KensaPanelDetail の親探索プロパティ重複

`P_FormPat` / `PtId` / `KensaDate`（親コントロールを遡って FormPat や KensaDate を探すロジック）が `KensaTabPage.cs:18`–78、`KensaTabPage2.cs:18`–78 に逐語的に重複し、`KensaPanelDetail.cs:16`–59 にも同型があります。前者 2 つは共通基底 `KensaTabPageBase` があるので、そこへ protected プロパティとして移動するだけで解消します。

### 12. 権限ユーザーIDのハードコード（MainForm.cs:182)

```csharp
if (LoginUser.Id.Equals("519") || LoginUser.Id.Equals("363") || LoginUser.Id.Equals("305") || ...)
```

エクスポート許可ユーザーの追加・削除のたびに再ビルド・再配布が必要になります。App.config（既に `AppConfig` の仕組みがある）へ `ExportAllowedUsers=519,363,305,752,1034` のように移すことを推奨します。

---

## 優先度: 低（気づいた点）

### 13. CheckBox ↔ "1" 変換の冗長な if 文

`OpeRegButton_Click` / `OpeBaseShow` などで以下が十数回続きます。

```csharp
if (this.AllCheckBox.Checked) { ope.AllCheck = "1"; }
// ↓
ope.AllCheck = AllCheckBox.Checked ? "1" : "";
```

表示側も `AgreeBox.Checked = ope.Agree.Equals("1");` と 1 行にできます。

### 14. BodyCalc() の二重パース（FormPat.cs:2921–2934)

`double.TryParse` で得た `d1`/`d2` を使わず、`double.Parse(HeightBox.Text)` を 7 回呼び直しています。`d1`/`d2` をそのまま使えば読みやすく、パース仕様の不一致リスクもなくなります。

### 15. Program.cs の引数連結（Program.cs:47–55)

for ループでの文字列連結は `string.Join(" ", args)` の 1 行で置き換えられます。

### 16. コメントアウトされた死にコード

`FormPat.cs:253`, `736`, `829`, `2894`, `3487`, `3793`（無効化された確認ダイアログ）、`ControlSumPage.cs:99` ほか、`KensaTabPage.cs:291` など。履歴は git にあるため削除を推奨します。「なぜ外したか」が重要なもの（`FormPat.cs:3792` の「操作性悪いため外す 2018/08/17」など）は理由コメントだけ残す形が良いです。

### 17. 命名規約の不統一

`_SumPage` / `F_OpeRsv` / `dSet` / `tmpBox` / `pos_x` / `KensaId`（private フィールドが PascalCase）など複数流儀が混在しています。全面的なリネームはエンコーディング混在のリスクに見合わないため推奨しませんが、**新規コードは `.claude/rules/coding-guidelines.md` に命名規約を明文化して統一** することを勧めます（例: private フィールドは `_camelCase`、ローカルは `camelCase`）。`tmp` プレフィックス（tmpRow, tmpBox, tmpDict…）は情報量がないので、`itemRow`, `diagBox` のような役割名を推奨します。

### 18. DataTable.Select へのユーザー入力文字列連結

`FormFindOpeRecord.cs:504`, `524` など `Select("Name = '" + RecordBox11.Text + "'")` は、値に `'` が含まれると実行時例外になります。コンボボックス選択値のため実害は限定的ですが、`'` → `''` エスケープの共通ヘルパーを通すと堅牢になります。

### 19. FormFindOpeRecord.MakeTableData() 内の繰り返し Split

`FormFindOpeRecord.cs:375`–413 でループ内の同じ `line.Split(',')` を 4 回呼んでいます。1 回変数に受ければ十分です（指摘 1 のパーサ導入で自然に解消）。

---

## 良い点（維持すべきプラクティス）

- **SearchTask.cs** — スレッド安全性の制約（共有 DB 接続を触らない）を XML コメントで明示し、接続の所有権・破棄タイミングまで説明している。設計・文書化ともに模範的
- **ExcelControl.cs** — COM 解放の徹底、遅延バインディングが必要な理由、マジックナンバー 52 の由来など「なぜ」のコメントが揃っている
- **FormExport.cs** — キーセットページングの採用理由、ヘッダー 1 行の理由など判断根拠が残されている
- **AppConfig.cs / InPrintCommon.cs** — 小さく単機能で再利用可能。重複ロジックの共通化先の好例
- **FormControl.cs** — フォームのライフサイクル管理を一元化する設計自体は適切

新規・修正コードはこれらのスタイル（XML ドキュメントコメント + 判断理由の記述）に揃えることを推奨します。

## 推奨する進め方

1. `ContData`（指摘 1）を新規クラスとして追加し、`EyeCenter.Tests` にテストを作成 → 呼び出し箇所を順次置換
2. 機械的で安全な置換（指摘 4, 8, 13, 15）を実施
3. `FormPat` の partial 分割（指摘 2）→ その後 `ExcelOpen` / 動的生成のヘルパー化（指摘 3, 6, 7）
4. 設定の外部化（指摘 12）と死にコード削除(指摘 16)

いずれの段階でも、対象ファイルの既存エンコーディング（Shift-JIS / UTF-8 BOM）の維持を確認しながら進めてください。
