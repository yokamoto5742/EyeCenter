# 追加エクスポート機能 実装計画

`docs/export_coverage_check.md` で「エクスポートできないデータ」とした5テーブルを
`FormExport.cs` のエクスポート対象に追加する実装計画。

## 方針

既存の5種類のエクスポート（SaveKensa / SaveOpe / SavePat / SaveRsv / SaveSummary）と同じ設計を踏襲する。

- キー順ページング（`PAGE_SIZE = 5000`、`ROWNUM` サブクエリ）でストリーム書き込み
- 既存の `CsvCell` / `WriteCsvLine` ヘルパーを使用（改行は `<CR+LF>` トークン化）
- 出力後に `WriteManifest` でマニフェスト（対象・ファイル名・件数・出力日時）を出力
- UTF-8（BOM付き）/ Shift-JIS の選択（`GetSelectedEncoding`）は共通
- ヘッダーは1行（汎用インポートツール向け。列名はDB列名そのまま）

### 確定済みの設計判断（2026-07-24 ユーザー確認済み）

1. **論理削除行（STATUS=0）も全件出力し、STATUS列を含める。**
   既存の手術予約エクスポート（EYE_OPE全件＋STATUS/DEL_*列）と同方針。移行時に取捨選択できる。
2. **患者エクスポート（SavePat）のPATIENT_ID和集合に EYE_KENSA2 / EYE_INTERVIEW を追加する。**
   新テーブルにしか登場しない患者が患者CSVから漏れないようにする。
3. PDF_SAVE系の列は既存方針どおり除外（ロック用フラグで移行不要。`export_coverage_check.md` 6・7項の判断に準拠）。

## 各テーブルの仕様

列構成は `MedicalLibrary\Agent` の各クラス（SQL・StdDbColumn定義）から確認済み。

### 1. EYE_KENSA2（検査の連番記録）— SaveKensa2

| 項目 | 内容 |
|---|---|
| 出力列 | PATIENT_ID, KENSA_ID, KENSA_NAME, KENSA_DATE, KENSA_SEQ, CONT, STAFF, SAVE_DATE, SAVE_TIME |
| KENSA_NAME | SaveKensa と同様に `EyeKensaMaster.Dict` で解決（辞書に無ければ空欄） |
| ページングキー | PATIENT_ID, KENSA_ID, KENSA_DATE, KENSA_SEQ（SaveKensa の3列複合キーに KENSA_SEQ を加えた4列複合） |
| 除外列 | PDF_SAVE / PDF_DATE / PDF_TIME |

### 2. EYE_INTERVIEW（問診記録）— SaveInterview

| 項目 | 内容 |
|---|---|
| 出力列 | ID, PATIENT_ID, IV_DATE, CONT, STAFF, SAVE_DATE, SAVE_TIME, STATUS |
| CONT | 自由記載テキスト（`IVContBox` の内容）。`<CR+LF>` トークン化のみ |
| ページングキー | ID（`EYE_INTERVIEW_SEQ` 採番の単一数値キー） |
| 備考 | STATUS=0（論理削除）も出力。PDF_SAVE は除外 |

### 3. EYE_OPE_DOCTOR（手術の医師記載事項）— SaveOpeDoctor

| 項目 | 内容 |
|---|---|
| 出力列 | ID, PRE_CONT, DO_CONT, STAFF, SAVE_DATE, SAVE_TIME, STATUS |
| ID | EYE_OPE.ID（手術ID）と同一。手術1件につき最大1行 |
| ページングキー | ID |

### 4. EYE_OPE_PASS（手術の看護申し送り事項）— SaveOpePass

| 項目 | 内容 |
|---|---|
| 出力列 | ID, CONT, STAFF, SAVE_DATE, SAVE_TIME, STATUS |
| ID | EYE_OPE.ID（手術ID）と同一 |
| ページングキー | ID |

### 5. EYE_OPE_RSV（手術予約枠のスケジュール設定）— SaveOpeRsv

| 項目 | 内容 |
|---|---|
| 出力列 | OPE_DATE, OPE_WAKU, OPE_KIND, RSV_KIND, COMT |
| 主キー | (OPE_DATE, OPE_WAKU, OPE_KIND) の複合キー。ID列・STAFF・SAVE_DATE系の列は存在しない |
| ページングキー | OPE_DATE, OPE_WAKU, OPE_KIND の3列複合。**OPE_WAKU は VARCHAR2 のため WHERE句の比較値は引用符で囲む**（他はNUMBER） |
| RSV_KIND | コード 1/2 のハードコードで名称辞書が無いため、名称列は付けない |

### 6. SavePat の修正

対象PATIENT_IDの和集合SQLに2テーブルを追加:

```sql
select PATIENT_ID from EYE_KENSA
union select PATIENT_ID from EYE_OPE
union select PATIENT_ID from EYE_SUMMARY
union select PATIENT_ID from EYE_KENSA2      -- 追加
union select PATIENT_ID from EYE_INTERVIEW   -- 追加
```

## UI変更（FormExport.Designer.cs）

ラジオボタンを5個追加する（既存の RadioButton 6個 → 11個）。

- 追加ボタン: `Kensa2Button`「検査連番（EYE_KENSA2）」/ `InterviewButton`「問診（EYE_INTERVIEW）」/ `OpeDoctorButton`「手術医師記載（EYE_OPE_DOCTOR）」/ `OpePassButton`「手術申し送り（EYE_OPE_PASS）」/ `OpeRsvButton`「手術予約枠（EYE_OPE_RSV）」
- 既存ボタンは y=35〜123 に22px間隔で並んでいるため、y=145 から5個を同間隔で追加し、
  `Utf8Check`（現y=160）と実行/閉じるボタン（現y=238）を下へ移動、`ClientSize` の高さを約110px拡大
- `PatButton` / `RsvButton` 追加時と同様に Designer.cs を直接編集する（このフォームでは実績のある手順）
- `ExeButton_Click` の分岐に5メソッドの呼び出しを追加

## エンコーディング上の注意（重要）

| ファイル | エンコーディング | 編集方法 |
|---|---|---|
| `FormExport.cs` | **Shift-JIS** | iconv往復ワークフローで編集し、編集後にLF正規化（CRLF混入防止） |
| `FormExport.Designer.cs` | UTF-8 (BOM) + CRLF | 通常編集 |

## 実装ステップ

1. `FormExport.Designer.cs` にラジオボタン5個追加・レイアウト調整
   → 検証: msbuild x86 Debug でビルド成功、フォームの見た目崩れなし
2. `FormExport.cs` に SaveKensa2 / SaveInterview / SaveOpeDoctor / SaveOpePass / SaveOpeRsv を追加、
   `ExeButton_Click` に分岐追加、SavePat の union 修正
   → 検証: ビルド成功、Shift-JIS維持を確認（日本語コメントの文字化けなし）
3. ローカルDB（FREEPDB1）で動作確認
   → 検証: 5ボタンそれぞれで CSV＋manifest が生成され、件数がテーブル件数と一致。
   　STATUS=0 の行を作って出力に含まれることを確認。SavePat が EYE_KENSA2/EYE_INTERVIEW のみの患者を拾うことを確認
4. `dotnet test EyeCenter.Tests` の既存テスト（CsvCell / WriteCsvLine）がパスすることを確認
   （新メソッドは既存ヘルパーの再利用のため新規テストは不要）
5. `docs/export_coverage_check.md` の「エクスポートできないデータ」1〜5項を実装済みに更新

## リスク・留意点

- テーブル定義は MedicalLibrary のSQLからの逆算。実DBにここに無い列が存在する可能性は残る
  （`select *` でロードするため、列を明示出力する本方式では未知列は静かに無視される）
- EYE_OPE_RSV の複合キーページングは新規パターン（VARCHAR2列を含む）のため、
  1ページ境界をまたぐデータ量（5,000件超）での動作は机上確認を丁寧に行う
  （実運用では予約枠データが5,000件を超える可能性は低い）
