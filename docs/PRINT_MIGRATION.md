# 一括印刷機能の電子カルテ移行 調査結果

対象: 一括印刷（`FormPrint`）の3帳票 — 入院患者一覧・検査予定一覧・ワークシート — を新電子カルテへ移行する際に変更が必要なコード箇所の調査。

作成日: 2026-07-06

関連: [MIGRATION_PLAN.md](MIGRATION_PLAN.md)（データ移行プラン）

---

## 1. 結論

- 電子カルテ（EMR）に依存するコードの大半は **EyeCenter 側ではなく MedicalLibrary 側**にある。
- EyeCenter 側の EMR 依存点は「眼科の診療科コード `"7"` のハードコード」のみだった（リファクタリングで1箇所に集約済み。§4）。
- 前回の移行時は MedicalLibrary 内の `#if INNO` 条件付きコンパイルで新旧システムを切り替えており、今回も同じパターンが踏襲できる（`MedicalLibrary.csproj` の x86 構成で `DefineConstants` に `INNO` を定義）。
- 描画ロジック（`FormPrint.cs`）と帳票レイアウト定義（`EyeCenter.xml` の `SumPrint1` / `SumPrint3` / `SumPrintEtc3` / `SumItem2`）は EMR 非依存で**変更不要**。

## 2. データの流れ

3帳票とも同じ構造:

```
患者リストを EMR から取得（PatIn / M_PATIENT）
  → 印刷内容（診療方針・検査予定等）を自前DBの EYE_SUMMARY から取得
  → FormPrint が EyeCenter.xml のレイアウト定義に従って描画
```

| 帳票 | ボタン | データ取得クラス | EMR 依存の入口 |
|---|---|---|---|
| 入院患者一覧 | `PatListButton` | `InPrint1.GetDict()` | `PatIn.GetListByDate()` |
| 検査予定一覧 | `KensaListButton` | `InPrint2.GetList()` | `EyeSummary.GetListByKensaDate()`（患者氏名の join） |
| ワークシート | `WorksheetButton` | `InPrint3.GetDict()` | `PatIn.GetListByDate()` |

## 3. 変更すべき箇所

### 3.1 MedicalLibrary 側（変更の本体）

| ファイル・箇所 | 内容 |
|---|---|
| `Entity/PatIn.cs` `GetListByDate()`（777行〜） | 入院患者一覧・ワークシートの患者リスト取得。EMR の `D_NYUIN` / `D_NYUIN_NOW` / `M_PATIENT` テーブルへ直接 SQL を発行しており、新カルテのスキーマに全面依存。日付が過去・当日・未来で3系統のクエリがある |
| 同 `GetRoomList()` / `GetDeptList()` / `GetDoctorList()` | 当日以外の日付指定時に病棟・病室・科・医師の履歴を取得。同様に EMR スキーマ依存 |
| 同 `PatInStatus` の判定 | 入院予定（`Yet`）の判定が `PROCESS in (10,18)` 等の INNO 固有コード値に依存。ワークシートは `Status == PatInStatus.Yet` の患者を除外している（`InPrint3.cs`、2019/11/01 視能訓練士依頼）ため、新カルテでの「入院予定」の表現方法の確認が必要 |
| 同 `OutTimeString` / `DoTimeString` | 時間帯コード値の変換（INNO: 1=朝/2=昼/3=夕/9=未定）。新カルテのコード体系に合わせる |
| `Agent/EyeSummary.cs` `GetListByKensaDate()`（193行〜） | 検査予定一覧の取得元。`EYE_SUMMARY` 自体は自前DB（`DB.Db2`）だが、**患者氏名を `M_PATIENT` + `Env.DB_LINK`（INNO.WORLD）への inner join で取得**している部分が EMR 依存。`LoadAll()` / `Find()` も同型の join を持つ |
| `Entity/PatBase.cs` `Load()`（496行） | 患者基本情報の取得（`M_PATIENT` を直接 select、`DB.Db3`）。ファイル内に `#if INNO` が計8箇所 |
| `Utility/DB.cs` `Db3` / `Utility/LibSettings.cs` `DBConnectionString3` / `Utility/Env.cs` `DB_LINK` | 新カルテDBへの接続文字列・DBリンク名の設定 |

### 3.2 EyeCenter 側

| ファイル | 内容 |
|---|---|
| `InPrintCommon.cs` `DeptCode` | 眼科の診療科コード `"7"`。新カルテでコード体系が変わる場合はこの1箇所を変更（§4 のリファクタリングで集約済み） |
| `FormPrint.cs` | 変更不要（印刷描画のみ。EMR 非依存） |
| `InPrint1.cs` / `InPrint3.cs` | 変更不要（EMR 依存部分は `PatIn` / `InPrintCommon.DeptCode` 経由） |
| `InPrint2.cs` | 変更不要（EMR 依存部分は `EyeSummary` 内に隠蔽されている） |
| `EyeCenter.xml`（`EyeDict` が読むレイアウト定義） | 変更不要（`SumPrint1` / `SumPrint3` / `SumPrintEtc3` / `SumItem2` は帳票の項目・幅の定義で EMR 非依存） |

## 4. 適用済みリファクタリング（2026-07-06）

`InPrint1.GetDict()` と `InPrint3.SetDict()` に全く同じサマリー値解析ロジック（Kind→Cont1/3/4 選択＋Code 照合、約40行）が重複していたため、以下を実施:

- 新規ファイル `InPrintCommon.cs`（UTF-8 BOM）を追加し、`GetSummaryValue(EyeSummary, DataRow)` として共通化
- 診療科コード `"7"` を `InPrintCommon.DeptCode` 定数に集約（`InPrint1.cs` / `InPrint3.cs` の計4箇所から参照）
- 移行時の変更箇所が2箇所→1箇所になる

`msbuild EyeCenter.sln /p:Configuration=Debug /p:Platform=x86` でビルド成功を確認済み。

**挙動差の注記:** 旧コードは `Kind` が 1/3/4 以外の行で直前行の `cont` を引きずるバグ的挙動があったが、共通化後は毎行リセットされる。レイアウト定義上 `Kind` は 1/3/4 のみのため実質同一挙動。

## 5. 未確定事項（移行方針に影響）

1. **新カルテのDBへ直接 SQL でアクセスできるか。** 現行は INNO の Oracle テーブル（`D_NYUIN` 等）を直接 select している。新カルテも Oracle だが（MIGRATION_PLAN.md 参照）、業務システムからの直接参照（読み取り用ビュー・DBリンク等）が許可されるかはベンダー確認が必要。API や CSV 連携のみの場合、`PatIn` の作りそのものが変わる。
2. **MedicalLibrary の切替方式。** 既存の `#if INNO` と同様に新カルテ用のコンパイルシンボルを追加する方針でよいか（変更最小なら前例踏襲が安全）。
3. **眼科の診療科コード。** 新カルテでも `"7"` か。変わる場合は `InPrintCommon.DeptCode` の1箇所を修正。
4. **EYE_SUMMARY（自前DB）の継続利用。** 3帳票の印刷内容はすべて `EYE_SUMMARY` 由来のため、サマリー自体を新カルテへ載せ替える計画なら `InPrint1/2/3` 全体の作り直しになる。
5. **入院予定の扱い。** ワークシートで除外している「入院予定」患者を新カルテのデータでどう判定するか（現行は `D_NYUIN.PROCESS in (10,18)`）。
