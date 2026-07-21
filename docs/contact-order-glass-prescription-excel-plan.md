# コンタクトレンズ注文書・眼鏡処方 Excel生成 実装計画（改訂版）

## 要件（確定事項）

- シード注文・パナコム注文・処方箋の3ボタン（`KensaPanelDetail.cs`）を、
  オペ録・申し送り書と同じ「共通情報シート書き込み＋TEMPへ別名保存＋最前面表示」処理に乗せる。
  **バーコードは付与しない。**
- **眼鏡処方**: 共通情報シートへの出力は **B1〜B12 のみ**。27行目以降は出力しない。
- **コンタクトレンズ注文書**: B1〜B12 に加えて **27行目以降** に現在の実出力と同じ内容を出力する。
  - A〜C列（27行目〜）: `ItemList` = `コンタクト注文/注文日/検査日` + パネル上の各コントロール
    （`401R_品名`, `402R_箱`, `411R_BC` … `401N_登録番号`）を定義順に Kind/Name/Value で出力。
  - E〜G列（27行目〜）: `ContactList` = 家族・連絡先（`EyeDoc` コンストラクタがDBから取得）。
  - これは既存 `ExcelControl.writeItemList()`（27行目起点・3列一括書き込み）と
    既存 `ContactButton_Click` の `ItemList` 組み立てをそのまま組み合わせれば再現できる。
    **新しい書き込みロジックは不要。**
- テンプレートファイル名は `EyeDataSettings.ini` に一本化する
  （`ContactOrderButton`/`GlassPrescButton` テーブルの `File` 列は参照しない）。
- シード用／パナコム用の判別はボタンの `Text` に「パナコム」を含むかどうかで行う。

## 前回計画からの主な変更点（再検討の結果）

1. **`MakeDocument` 内のテンプレート名分岐をやめ、バーコードなし専用の公開メソッドを新設する。**
   前回案は「テンプレート名がINIの3ファイル名と一致したらバーコードなし」という
   文字列一致依存の分岐だったが、呼び出し側（`KensaPanelDetail`）はどの帳票かを
   最初から知っているため、明示的に `MakeSimpleDocument()` を呼ぶ方が単純で壊れにくい。
   `MakeDocument` は現状のまま変更最小限（共通処理の抽出のみ）。
2. **ボタンの `Tag` にファイルパスを仕込む手順を廃止。**
   クリックハンドラ内で `((Button)sender).Text` を使って直接ファイル名を解決すれば足りる
   （ボタン生成コードは一切変更しない）。
3. **眼鏡処方は27行目以降を書かない**（要件確定を反映）。
   `GlassButton_Click` の `ItemList` 組み立てループは不要になるため削除する。

## 実装ステップ

### 1. `EyeDataSettings.ini` に新セクションを追加

```ini
[DOCUMENT_SETTINGS]
; 共通情報シート書き込み・別名保存の対象テンプレートファイル名（バーコードは付与しない）
CONTACT_ORDER_SEED_FILE=コンタクトレンズ注文書(シード).xlsx
CONTACT_ORDER_PANACOM_FILE=コンタクトレンズ注文書(パナコム).xlsx
GLASS_PRESCRIPTION_FILE=眼鏡処方.xlsx
```

検証: ini未設定・ファイル無しの場合は上記と同じ既定値にフォールバックする。

### 2. `ExcelControl.cs` の変更（UTF-8。既存エンコーディング維持）

#### 2-1. 共通処理の抽出（リファクタリング）

`MakeDocument()` の「ワークブックを開く〜B1〜B12書き込み〜保存〜表示」部分を
private コアメソッドに抽出する:

```csharp
private void createDocument(EyeDoc doc, string documentCode, string targetSheets,
    bool useBarcode, bool writeLists)
```

- `useBarcode = false` のとき:
  - B12（バーコード値）は空文字を書き込む。
  - バーコード画像挿入のシートループをスキップする。
- `writeLists = false` のとき: 27行目以降の5リスト
  （ItemList/ContactList/PatInfoList/SumList/AllergyList）の書き込みをスキップする。
- 保存形式: テンプレート拡張子が `.xlsm` なら `52`（マクロ有効）、
  それ以外（`.xlsx`）なら `51`（xlOpenXMLWorkbook）を指定する。

#### 2-2. 公開メソッド

- `MakeDocument(EyeDoc doc)`: シグネチャ・外部挙動は現状維持。
  オペ録/眼科申し送り書 → `createDocument(doc, code, sheets, useBarcode: true, writeLists: true)`。
  それ以外 → 従来どおり `doc.ExcelOpen()` + 前面化（変更なし）。
- **新設** `MakeSimpleDocument(EyeDoc doc, bool writeLists)`:
  バーコードなしで `createDocument(doc, "", "", useBarcode: false, writeLists)` を呼ぶ。
  コンタクト注文は `writeLists: true`、眼鏡処方は `writeLists: false` で呼び出す。

#### 2-3. ファイル名解決の静的メソッド（新設）

- `public static string GetContactOrderFileName(string buttonText)`:
  `buttonText` に「パナコム」を含めば `CONTACT_ORDER_PANACOM_FILE`、
  それ以外は `CONTACT_ORDER_SEED_FILE` の値を返す。
- `public static string GetGlassPrescriptionFileName()`: `GLASS_PRESCRIPTION_FILE` の値を返す。
- いずれも `EyeDataSettings.ini` の `[DOCUMENT_SETTINGS]` を読み（無ければ既定値）、
  `AppDomain.CurrentDomain.BaseDirectory` と結合したフルパスを返す。

検証: `.xlsm`（オペ録・申し送り書）が従来どおりバーコード付きで保存できること／
`.xlsx` が形式エラー・警告なく保存できること。

### 3. `KensaPanelDetail.cs` の変更（**Shift-JIS。iconv往復ワークフローで編集**）

ボタン生成コード（コンストラクタ／`ControlShow`）は一切変更しない。

#### 3-1. `ContactButton_Click`

- `ContactOrderButton` テーブルの `File` 列参照ループを削除し、
  `tmpDoc.FileName = ExcelControl.GetContactOrderFileName(((Button)sender).Text);` に置き換える。
- `ItemList` の組み立て（`注文日` + コントロール走査）は**現状のまま維持**
  （これが27行目以降の出力内容そのもの）。
- 末尾の `tmpDoc.ExcelOpen()` を `FormPat.Excel.cs` と同じパターンに置き換える:

```csharp
ExcelControl excelControl = new ExcelControl();

try
{
    excelControl.MakeSimpleDocument(tmpDoc, true);
}
catch (Exception ex)
{
    LibUtility.Except(ex);
}
finally
{
    excelControl.ReleaseExcel();
}
```

#### 3-2. `GlassButton_Click`

- `tmpDoc.FileName = ExcelControl.GetGlassPrescriptionFileName();` に置き換える。
- `ItemList` の組み立て（`処方日` + コントロール走査）を**削除**する
  （眼鏡処方はB1〜B12のみ出力するため不要）。
- 末尾を `MakeSimpleDocument(tmpDoc, false)` の try/catch/finally パターンに置き換える。

検証:
1. シード注文／パナコム注文ボタン → 共通情報にB1〜B12＋27行目以降
   （A〜C列にコンタクト注文項目、E〜G列に家族・連絡先）が書き込まれ、
   TEMPへ `患者ID_日時_ファイル名` で保存・最前面表示される。
2. 処方箋ボタン → 共通情報にB1〜B12のみ書き込まれ、同様に保存・表示される。
3. オペ録・眼科申し送り書 → 従来どおり（リグレッションなし）。

## 変更しないもの

- ボタンの表示位置・サイズ・Text・KensaID紐付けなどUI生成ロジック。
- `ContactOrderButton`/`GlassPrescButton` テーブル自体（`File` 列はコード側で参照しなくなるだけ）。
- オペ録・眼科申し送り書のバーコード生成処理（共通処理の抽出のみで挙動は不変）。
- `LensMeterButton`（レンズメーター取込）関連。

## リスク・要確認事項

- 実機テンプレート（3ファイル）に「共通情報」シートが存在し、B1〜B12のレイアウトが
  オペ録・申し送り書と一致していること（ユーザー申告ベース、未実物確認）。
- 眼鏡処方テンプレートの他シートが共通情報の27行目以降を数式参照して**いない**こと
  （B1〜B12のみ出力するため、参照していると値が空になる）。
- `.xlsx` × `fileFormat=51` の組み合わせは本Interop.Excelで未検証
  （実績があるのは `.xlsm` × `52` のみ）。
