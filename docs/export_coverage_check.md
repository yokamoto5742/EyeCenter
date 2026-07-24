# エクスポート機能のデータ網羅性チェック

`FormExport.cs` のエクスポート機能（患者・検査・手術記録・手術予約・サマリーの5種類）を、
本アプリが実際に保存している全データ（`EYE_*` テーブル、`MedicalLibrary\Agent` の Eye 系クラスから確認）と突き合わせた結果。

## エクスポートできているデータ

| ボタン | 対象テーブル | 備考 |
|---|---|---|
| 検査 | EYE_KENSA | PDF_SAVE列を除き網羅 |
| 手術記録 | EYE_OPE_RECORD + EYE_OPE（結合） | PDF_SAVE/PDF_DATE/PDF_TIME列を除き網羅 |
| 患者 | EYE_KENSA/EYE_OPE/EYE_SUMMARYに登場する全PATIENT_IDを対象にPatBaseから氏名等を取得 | |
| 手術予約 | EYE_OPE（全カラム、DEL_STAFF/DEL_DATE/DEL_TIME含む） | |
| サマリー | EYE_SUMMARY | 全カラム網羅 |

## エクスポートできないデータ

以下は本アプリが保存・参照しているが、エクスポート画面の対象になっていない。

1. **EYE_KENSA2**（検査の連番記録） — `FormKensa2.cs` / `MedicalLibrary.Agent.EyeKensa2` が使用。EYE_KENSAと似た構造（PATIENT_ID/KENSA_ID/KENSA_DATE+KENSA_SEQ）だが別テーブルで、エクスポート対象に含まれない。
2. **EYE_INTERVIEW**（問診記録） — `ControlIVPage.cs` / `MedicalLibrary.Agent.EyeIV` が使用。PATIENT_ID紐付けの臨床データ。
3. **EYE_OPE_DOCTOR**（手術の医師記載事項：術前所見PRE_CONT・執刀記録DO_CONT） — `FormPat.cs` / `MedicalLibrary.Agent.EyeOpeDoctor` が使用。手術ID紐付け。
4. **EYE_OPE_PASS**（手術の看護申し送り事項） — `FormPat.cs` / `MedicalLibrary.Agent.EyeOpePass` が使用。手術ID紐付け。
5. **EYE_OPE_RSV**（手術予約枠のスケジュール設定：日付・枠・診療区分・コメント） — `FormOpeCal.cs` / `MedicalLibrary.Agent.EyeOpeRsv` が使用。患者に紐付かない予約枠設定データ。

6. **EYE_KENSA の PDF_SAVE列**（PDF保存済みフラグ） — `SaveKensa`のSELECT対象には入るが、CSV出力列には含まれない。
7. **EYE_OPE_RECORD の PDF_SAVE / PDF_DATE / PDF_TIME列**（PDF保存日時） — `SaveOpe`は列を明示指定して結合しており、この3列は選択されていない。

## 参考：エクスポート対象外と判断したテーブル

`MedicalLibrary\Agent` 配下には `BILL` / `BILL_PAY` / `COME_REPORT` / `COME_REPORT_SCHEMA` / `POS_DEMAND` / `POSREG_HISTORY_DETAIL` 等のテーブルも存在するが、EyeCenter本体のコードからは一切参照されていない（他システム向けの共通ライブラリ機能）ため、本チェックの対象外とした。

## 前提・限界

- テーブル定義は本リポジトリには無く、隣接プロジェクト `MedicalLibrary`（`Agent\Eye*.cs`）のSQL/保存ロジックから逆算した。実際のDBスキーマにここで確認した以外の列がある可能性は排除できない。
- 上記1〜5は、電子カルテ移行時のデータ移行対象としても未考慮（`docs/eyedata_migration_plan.md`の移行対象リストに含まれていない）。移行を行う場合はこれらの追加エクスポート手段の要否を確認する必要がある。

- 6.EYE_KENSA.PDF_SAVE列 — CSVには含まれませんが、アプリ内で実際に使われています。
KensaTabPage.cs:347,357 / KensaTabPage2.cs:413,423 / FormKensa2.cs:169,192 で kensa.PDFSave.Equals("1") を判定し、「登録ボタン」「削除ボタン」の活性/非活性を制御しています（PDF保存済み＝確定済みとして編集不可にするロック用フラグ）。
ただし、このアプリのコード内で PDF_SAVE に "1" を書き込む箇所は見当たらず（外部システムか手動更新で立つ想定）、CSV出力機能自体は問診記録の PDFButton_Click（Launcher.PdfViewer() 呼び出し）と同様、別の外部PDFビューア連携を指すもので、EYE_KENSAのPDF_SAVEを書き込む処理ではありません。

→ エクスポート対象外のままで問題なし。CSV出力の目的（検査データの転記・移行）にはロック状態フラグは不要で、除外は妥当です。

- 7.EYE_OPE_RECORD.PDF_SAVE / PDF_DATE / PDF_TIME列 — リポジトリ内を全文検索しましたが、EyeOpeRecord に対してこれら3列を参照・使用している箇所は一切ありません（読み込みも書き込みも無し）。EYE_KENSAとは異なり、業務ロジックへの関与もゼロです。

→ エクスポート対象外で完全に問題なし。