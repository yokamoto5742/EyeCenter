# DB書き込み（直接編集）操作 洗い出し

電子カルテ移行にあたり、EyeData が**データベースのデータを直接編集（INSERT / UPDATE / DELETE）する処理**を全数洗い出したもの。
参照（SELECT）機能は残す前提のため、参照のみの箇所は「4. 参照のみの箇所」に整理した。

**前提: 移行後も EyeData は継続使用する。** したがって2章の書き込み操作はいずれも廃止対象ではなく、
「書き込み先（OPENスキーマ）をどこに置き直すか」を決めるための一覧である。

調査対象: `EyeCenter`（EyeData 本体）および呼び出し先の `MedicalLibrary`。
調査日: 2026-07-27

---

## 1. 前提：接続先DBの整理

`MedicalLibrary_Settings.xml` で3系統の接続を定義し、EyeData は **Db2 と Db3 のみ**使用する。

| 接続 | 接続文字列 | 内容 | EyeDataでの用途 |
|---|---|---|---|
| `DB.Db2` | `open / inno_open` | **OPENスキーマ**（EyeData自身の `EYE_*` テーブル群） | 参照 **＋ 書き込み** |
| `DB.Db3` | `medb / inno_orcl` | **電子カルテ本体スキーマ**（`M_PATIENT` 等） | **参照のみ** |
| `DB.Db1` | `macs / wgs_odbc_orcl` | レガシー | 未使用（`LibSettings.Init` でも初期化されない） |

DBリンク `@INNO.WORLD`（`MedicalLibrary/Utility/Env.cs:124`）は OPEN → 電子カルテ本体への参照用。
**EyeData の書き込み処理でDBリンクを使っている箇所は無い。**

### 結論

> **電子カルテ本体スキーマ（`medb` / `Db3`）への書き込みは無い。**
> 唯一の書き込みだった患者台帳の「伝達」ボタン（`InfoShareButton` → `D_KARTE_MEMO`）は
> **2026-07-27 に機能ごと削除済み**（6章 論点1 参照）。
> 書き込みはすべて OPENスキーマ（`open` / `Db2`）の `EYE_*` テーブルに対するもので、
> 患者マスタ・オーダー・カルテ本文・伝達情報を編集する処理は存在しない。

OPENスキーマは現行電子カルテ（InnoKarte）のOracle環境上に同居しているため、
「電子カルテのDBを直接編集している作業」＝下記2章の一覧、として扱うのが移行検討上は妥当。

#### 調査方法（網羅性の根拠）

1. EyeCenter 側の全 `.cs` から書き込み系呼び出し（`Save` / `Delete` / `ExecuteNonQuery`）を抽出
2. **MedicalLibrary 側で `DB.Db3` に書き込む全箇所**を列挙し、EyeData から到達可能かを個別に確認
   - 書き込み可能なエンティティ: `Memo` / `PostIt` / `KarteMessage` / `AddressGroup` / `DiagDPC` の5種
   - このうち EyeData から到達していたのは `Memo` のみ（`FormMemo` 経由）だが、伝達ボタン削除により
     **現在はどれにも到達しない**。他4種を使うライブラリ側フォーム
     （`FormKarteMessage1/2` / `FormAddressGroup` / `CtrlPostItGridView1` 等）は EyeData から開かれない
3. EyeCenter が生成する MedicalLibrary 製フォーム／コントロールを全数確認
   - `FormString1`（表示のみ）、`StdControlPat1`・`LoginChange`（書き込みなし）
   - `FormMemo`（唯一の書き込みあり）は伝達ボタン削除により生成箇所が無くなった

> 注意: 書き込みは EyeCenter のコードではなく **MedicalLibrary のフォーム内**で起きる場合があるため、
> EyeCenter 内の grep だけでは検出できない。上記2・3の確認が必須。

---

## 2. 書き込み操作 一覧（全17操作 / 対象9テーブル ＝ すべてOPENスキーマ）

| # | 機能・画面 | 操作（ボタン/メニュー） | 呼び出し元 | 実処理 | 対象テーブル | SQL種別 |
|---|---|---|---|---|---|---|
| 1 | 患者台帳 問診タブ | 「登録」ボタン | `FormPat.cs:1832` → `ControlIVPage.cs:159` | `EyeIV.Save` | `EYE_INTERVIEW` | UPDATE / INSERT |
| 2 | 患者台帳 問診タブ | 「削除」ボタン | `FormPat.cs:1841` → `ControlIVPage.cs:178` | `EyeIV.Delete` | `EYE_INTERVIEW` | UPDATE（論理削除） |
| 3 | 患者台帳 サマリタブ | 「登録」ボタン | `FormPat.cs:1824` → `ControlSumPage.cs:686` | `EyeSummary.Save` | `EYE_SUMMARY` | UPDATE / INSERT |
| 4 | 患者台帳 検査タブ | 「登録」ボタン | `KensaTabPage.cs:258` | `EyeKensa.Save` | `EYE_KENSA` | UPDATE / INSERT |
| 5 | 患者台帳 検査タブ | 「削除」ボタン | `KensaTabPage.cs:301` | `EyeKensa.Delete` | `EYE_KENSA` | **DELETE（物理削除）** |
| 6 | 患者台帳 検査タブ（複数回検査） | 「登録」ボタン | `KensaTabPage2.cs:329` | `EyeKensa2.Save` | `EYE_KENSA2` | UPDATE / INSERT |
| 7 | 患者台帳 検査タブ（複数回検査） | 「削除」ボタン | `KensaTabPage2.cs:373` | `EyeKensa2.Delete` | `EYE_KENSA2` | **DELETE（物理削除）** |
| 8 | 検査入力画面（FormKensa2） | 「登録」ボタン | `FormKensa2.cs:277` | `EyeKensa2.Save` | `EYE_KENSA2` | UPDATE / INSERT |
| 9 | 検査入力画面（FormKensa2） | 「削除」ボタン | `FormKensa2.cs:308` | `EyeKensa2.Delete` | `EYE_KENSA2` | **DELETE（物理削除）** |
| 10 | 患者台帳 手術タブ 基本情報 | 「登録」ボタン | `FormPat.cs:1409` | `EyeOpe.Save` | `EYE_OPE` | UPDATE / INSERT |
| 11 | 患者台帳 手術タブ 手術記録 | 「登録」ボタン | `FormPat.cs:1439` | `EyeOpeRecord.Save` | `EYE_OPE_RECORD` | UPDATE / INSERT |
| 12 | 患者台帳 手術タブ 医師記録 | 「登録」ボタン | `FormPat.cs:1470` | `EyeOpeDoctor.Save` | `EYE_OPE_DOCTOR` | UPDATE / INSERT |
| 13 | 患者台帳 手術タブ パス | 「登録」ボタン | `FormPat.cs:1500` | `EyeOpePass.Save` | `EYE_OPE_PASS` | UPDATE / INSERT |
| 14 | 患者台帳 手術歴 | 右クリック→「削除」 | `FormPat.cs:1290` | `EyeOpe.Delete` | `EYE_OPE` | UPDATE（論理削除） |
| 15 | 手術予約一覧（FormOpeRsvList） | 「削除」ボタン | `FormOpeRsvList.cs:381` | `EyeOpe.Delete` | `EYE_OPE` | UPDATE（論理削除） |
| 16 | 手術予約カレンダー（FormOpeCal） | 右クリック→「枠設定 診療」/「枠設定 休診」 | `FormOpeCal.cs:669` / `:695` | `EyeOpeRsv.Save` | `EYE_OPE_RSV` | UPDATE / INSERT |
| 17 | 手術予約カレンダー（FormOpeCal） | 右クリック→「診察・休診設定を削除」 | `FormOpeCal.cs:711` | `EyeOpeRsv.Delete` | `EYE_OPE_RSV` | **DELETE（物理削除）** |

\#1〜17 はすべて `DB.Db2`（OPENスキーマ）に対する操作。`DB.Db3`（電子カルテ本体スキーマ）への書き込みは無い。

---

## 3. 操作の詳細

### 3-1. 問診（`EYE_INTERVIEW`）

- **登録**: `MedicalLibrary/Agent/EyeIV.cs:24 Save()`
  - `ID` が入っていれば `where ID = :ID` で UPDATE
  - 空なら `ID = EYE_INTERVIEW_SEQ.nextval` で INSERT
  - `STATUS = 1` 固定（`ControlIVPage.cs:157`）
- **削除**: `MedicalLibrary/Agent/EyeIV.cs:110 Delete()`
  - `update EYE_INTERVIEW set STATUS = 0 where ID = ...` の**論理削除**。行は残る。
- 使用シーケンス: `EYE_INTERVIEW_SEQ`

### 3-2. サマリー（`EYE_SUMMARY`）

- **登録**: `MedicalLibrary/Agent/EyeSummary.cs:45 Save()`
  - `where PATIENT_ID = ...` で UPDATE → 更新0件なら INSERT（患者1件1レコード）
  - `CONT1`〜`CONT4` は画面コントロールを `ContData.Build` で1テキストに直列化して格納
- **削除機能は無し。**

### 3-3. 検査（`EYE_KENSA` / `EYE_KENSA2`）

- **登録**: `EyeKensa.cs:16 Save()` / `EyeKensa2.cs:16 Save()`
  - キー: `PATIENT_ID` + `KENSA_ID` + `KENSA_DATE`（`EYE_KENSA2` は加えて `KENSA_SEQ`）
  - UPDATE → 更新0件なら INSERT
- **削除**: `EyeKensa.cs:291 Delete()` / `EyeKensa2.cs:229 Delete()`
  - `delete from ...` の**物理削除**。復旧不可。
  - UI側で「検査日が1週間以上前」の場合に追加確認ダイアログを出すのみで、削除自体は制限していない
    （`KensaTabPage.cs:283`, `KensaTabPage2.cs:355`, `FormKensa2.cs:294`）。

### 3-4. 手術（`EYE_OPE` / `EYE_OPE_RECORD` / `EYE_OPE_DOCTOR` / `EYE_OPE_PASS`）

- **基本情報 登録**: `MedicalLibrary/Agent/EyeOpe.cs:105 Save()`
  - `ID` 有りは UPDATE、無しは `ID = EYE_OPE_SEQ.nextval` で INSERT
  - 手術予約もこのテーブル（`OPE_DATE` / `OPE_TIME` / `OPE_KIND`）で管理
- **手術記録 / 医師記録 / パス 登録**: `EyeOpeRecord.cs:41` / `EyeOpeDoctor.cs:30` / `EyeOpePass.cs:29`
  - いずれも `EYE_OPE.ID` を主キーに UPDATE → 0件なら INSERT（基本情報の保存が先に必要）
- **削除**: `MedicalLibrary/Agent/EyeOpe.cs:309 Delete()`
  - `update EYE_OPE set STATUS = 0, DEL_STAFF, DEL_DATE, DEL_TIME where ID = ...` の**論理削除**
  - **注意**: `EYE_OPE_RECORD` / `EYE_OPE_DOCTOR` / `EYE_OPE_PASS` へはカスケードしない。
    ライブラリ側に各 `Delete()`（`STATUS = 0` 更新）は実装されているが、**EyeData からは呼ばれていない**ため子レコードは `STATUS` が残ったまま滞留する。
- 使用シーケンス: `EYE_OPE_SEQ`

### 3-5. 手術枠設定（`EYE_OPE_RSV`）

- **登録**: `MedicalLibrary/Agent/EyeOpeRsv.cs:43 Save()`
  - キー: `OPE_DATE` + `OPE_WAKU` + `OPE_KIND`、UPDATE → 0件なら INSERT
  - `RSV_KIND` = 1（診療）/ 2（休診）
- **削除**: `EyeOpeRsv.cs:185 Delete()` — `delete from EYE_OPE_RSV ...` の**物理削除**

---

## 4. 参照のみの箇所（移行後も残す機能）

### 4-1. 電子カルテ本体スキーマ（`DB.Db3` = `medb`）への参照

| 箇所 | 内容 | 参照先 |
|---|---|---|
| `FormPat.cs:104,120,152` | 患者基本情報の読込 | `PatBase.Load` → `M_PATIENT` |
| `FormOpeRsv.cs:67` | 手術予約画面の患者情報表示 | `PatBase.Load` → `M_PATIENT` |
| `FormPat.cs:1140` | 「カルテ取込」ボタン — 感染症・身長・体重の取込（画面に表示するのみ） | `InfectionData.GetInfectionData` / `BaseInfo.GetDict` |
| `FormPat.cs:1146` | 既往歴・感染症などの基礎情報表示 | `BaseInfo.GetDict` |
| `FormPat.cs:1776,1785` | 「家族」「禁忌」ボタン — `FormString1` で表示のみ（DBアクセス無し、読込済みの値を渡すだけ） | — |
| `FormExport.cs:558,792` | エクスポート時の患者氏名等の付与 | `PatBase.GetList` → `M_PATIENT` |
| `FormFindKensa.cs:70` / `FormFindOpeRecord.cs:127` / `FormFindSummary.cs:112` | 検索結果への患者情報付与（`SearchTask.PatDb` 経由の専用接続） | `M_PATIENT` |
| `InPrint1.cs:20` / `InPrint3.cs:21` | 入院患者一覧の取得 | `PatIn.GetListByDate` |

### 4-2. OPENスキーマ（`DB.Db2`）への参照

- `FormExport.cs`（CSVエクスポート、`EYE_*` を SELECT のみ）
- `FormExport.Schema.cs`（`ALL_TAB_COLUMNS` 等のディクショナリビュー参照。`DB.Db2.InitString` で別接続を張る）
- 各画面の履歴表示・検索（`EyeKensa.LoadBy*` / `EyeOpe.GetList*` / `EyeSummary.Find` など）

---

## 5. DB以外で電子カルテ側のデータ領域に書き込む処理

DBではないが、電子カルテ側の共有領域にファイルを書き込む処理があるため併記する。

| 箇所 | 内容 |
|---|---|
| `FormPat.cs:1522` / `FormPat.cs:1536` | `Pat.WritePatCSV()` → `C:\innokarte\Pat.csv` に患者情報を書き出し、PDFビューア / Grapa を起動 |
| `MedicalLibrary/Entity/PatBase.cs:753 DeletePatCSV()` | `Pat.csv` / `Pat2.csv` の削除（InnoKarte未起動時のみ、または強制） |
| `MainForm.cs:93` | `PatBase.ReadPatCSV()` — ログインユーザー／患者IDの取得（読込） |

`MainForm.cs:149,154` の `CanonRKF1.exe` / `NidekARK1.exe` 起動、`FormPat.cs:1528` の `InnoProgram.KarteShow` はプロセス起動のみでDB書き込みは無い。

---

## 6. 移行時の論点

1. **電子カルテ本体DBへの書き込みは解消済み（2026-07-27）。**
   唯一の書き込みだった「伝達」ボタン（`D_KARTE_MEMO`）を機能ごと削除したため、
   移行にあたり電子カルテ本体スキーマへの書き込み対応は不要になった。
   - 伝達情報の閲覧・入力は電子カルテ本体側で行う運用となる
   - 既存の伝達情報データ（`D_KARTE_MEMO`）を新カルテへ移行するかは EyeData 側とは無関係に判断する
   - 削除内容: `FormPat` の `InfoShareButton`（ボタン定義／`InfoShareButton_Click`／
     `PtShow` 内の `Memo.Load` によるボタン色分け）を除去。同じ行の残りのボタンは40px左詰め
2. 上記以外の改修対象は「参照先（`M_PATIENT` / `BaseInfo` / `InfectionData` / `PatIn`）の置き換え」に限られる
   （`docs/eyedata_migration_plan.md` の作業リスト参照）。
3. **2章 #1〜17（OPENスキーマ）はそのまま残す。** 移行作業の本体は `EYE_*` テーブルの移設先決定。
   現行は InnoKarte の Oracle 環境に同居しているため、カルテ撤去時に同じインスタンスが使えなくなる可能性がある。
   - 案A: 自院で用意する Oracle に `EYE_*` とシーケンス（`EYE_OPE_SEQ` / `EYE_INTERVIEW_SEQ`）を移設し、接続文字列（`DBConnectionString2`）だけ差し替える
   - 案B: 新カルテベンダーの提供DBに同居させる
   どちらでも #1〜17 のコードは無改修で動く（テーブル定義とシーケンスが同一であれば）。
   - 移設時に必要なもの: 9テーブルのDDL＋データ、2シーケンス（`EYE_OPE_SEQ` / `EYE_INTERVIEW_SEQ`、**現行の最終採番値を引き継ぐこと**）、
     参照用に電子カルテ側の患者情報へ到達する手段（現行のDBリンク `INNO.WORLD` 相当）
   - 案Aの場合、`Db3`（患者参照）と `Db2`（`EYE_*`）が別インスタンスになるため、DBリンク結合を使う参照箇所は
     2段階取得への切り替えが必要（`EyeOpe.GetListByKindDates` 系が未対応。`docs/eyedata_migration_plan.md` 参照）
4. **物理削除している3テーブル**（`EYE_KENSA` / `EYE_KENSA2` / `EYE_OPE_RSV`）は移行前後で差分検証がしづらい。
   移行リハーサル時は件数スナップショットを取ってから作業する。
5. **`EYE_OPE` 削除時に子テーブルへカスケードしない**問題（3-4参照）があるため、移行時の抽出条件は
   `EYE_OPE.STATUS = 1` を起点にし、`EYE_OPE_RECORD` 等を単独で抽出しないこと。
