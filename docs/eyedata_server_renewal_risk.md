# 次期サーバー更新時の Oracle 起因リスク検証

作成日: 2026-09-14

## 1. 背景

ある開発者から「このアプリケーションは現在の Windows サーバーでは動いているが、次期更新でサーバーが新しくなると Oracle が原因で動かなくなる可能性がある」との指摘があった。
その妥当性を検証した記録である。

前提（ユーザー確認済み）:

- 以前は各端末に Oracle クライアントをインストールする実装だったが、現在はそうではない
- 次期更新では **Windows サーバー・Oracle DB サーバーの両方が入れ替わる**。時期は数年後で、Oracle のバージョンは未定
- **電子カルテ本体も別製品に変わる**
- 本番用 `tnsnames.ora` は準備済み

## 2. 結論

| 論点 | 判定 | 起きた場合の対処 |
|---|---|---|
| ① Oracle クライアントが無いと動かない | **妥当ではない** | 不要 |
| ② 新 DB がドライバ（19.x）の相互運用範囲外になる | **DB バージョン次第** | DLL 差し替え＋全アセンブリ再ビルド |
| ③ DB 文字コードが AL32UTF8 になる | **可能性が高い** | DB 側の列定義拡張（DMU） |
| ④ DB 側設定（パスワード形式・DB リンク等） | 移行作業次第 | DB 側で対応 |
| ⑤ **電子カルテ製品の変更** | **ほぼ確実に影響あり（最大の論点）** | 連携部分の再設計・改修 |

「Oracle クライアントに依存しているから新サーバーで動かない」という意味での指摘は当たらない。
一方で、**新 DB に無改修で繋がる保証は無く**、さらに電子カルテ製品の変更は Oracle のバージョンより影響が大きい。
つまり「次期更新で何らかの改修が必要になる」という点では指摘は結果的に正しいが、原因の中心は Oracle クライアントではなく **電子カルテとの連携部分** である。

## 3. 検証: Oracle クライアント依存は無い（論点①）

2026-08-03 のコミット `45e79d4` で ODP.NET マネージド・ドライバへ移行済み
（経緯は `docs/eyedata_oracle_client_removal_plan.md`）。

本番配置フォルダ `C:\Shinseikai\EyeData` の実物をアセンブリ参照で確認した結果:

| ファイル | Oracle 関連の参照 |
|---|---|
| `EyeData.exe` | なし（MedicalLibrary 経由） |
| `MedicalLibrary.dll` | `Oracle.ManagedDataAccess 4.122.19.1` |
| `InnoUketsukeLib.dll` | `Oracle.ManagedDataAccess 4.122.19.1` |
| `CanonRKF1.exe` / `NidekARK1.exe` | なし（MedicalLibrary 経由） |
| `Oracle.ManagedDataAccess.dll` | FileVersion 4.122.19.1、100% マネージド（MSIL） |

- 旧 `Oracle.DataAccess`（ネイティブ DLL＝クライアントのインストールが必須）を参照するファイルは存在しない
- 本番 DB（11.2.0.1.0）への接続に成功済み
- 新しい Windows Server には .NET Framework 4.8 系が標準搭載されており、x86 アプリも WOW64 で動作する。OS 更新だけで Oracle 起因の不具合は起きない

### tnsnames.ora（対応済み）

接続文字列は `Data Source=wgs_odbc_orcl` / `inno_open` / `inno_orcl` の TNS エイリアス指定で、`tnsnames.ora` に依存する。
マネージド・ドライバは exe と同じフォルダ（または `TNS_ADMIN`）に `tnsnames.ora` が無いと起動時に例外になる（開発機で実測済み）。

- `deploy-production.ps1` のコピー対象は `*.exe, *.exe.config, *.dll, *.ini, *.xml` で、**`.ora` は含まれない**。新サーバー構築時は手動配置を忘れないこと
- 本番用 `tnsnames.ora` は準備済み。新 DB のホスト名・サービス名に合わせて書き換える

### 補足: 同居アプリ

隣接リポジトリの `Agree` は `OleDbConnection` + `Provider=OraOLEDB.Oracle` を使っており、**Oracle クライアントのインストールが必須**。
同じサーバーで運用する場合、指摘はこちらには当てはまる。

## 4. DB サーバー入れ替え時の論点

### 4.1 ドライバの相互運用範囲（論点②）

- 開発機のテスト DB は **Oracle 26ai Free** で、本番と同じ `4.122.19.1` で EyeCenter が end-to-end に動作している。現時点の最新 Oracle になら現行 DLL のまま繋がる見込みが高い
- ただし数年後に 26ai より新しいリリースになった場合、19.x クライアントが相互運用範囲（Doc ID 207303.1）から外れる可能性がある。Oracle は過去にも新クライアントで古いサーバーを切っている（21c クライアントは 12.1 未満のサーバー非対応）
- 対処:
  1. `C:\Shinseikai\EyeData\Oracle.ManagedDataAccess.dll` を新 DB に対応する版へ差し替える
  2. EyeCenter / MedicalLibrary / InnoUketsukeLib / CanonRKF1 / NidekARK1 を **同時に再ビルド** する。csproj の参照が `Version=4.122.19.1` の強い名前のため、DLL を差し替えるだけでは読み込めない
  3. 使用 API は `OracleConnection` / `OracleCommand` / `OracleDbType` / `BindByName` / `Cancel` 等の基本機能のみで、コード改修はほぼ不要の見込み
- **事前に最新ドライバへ上げておくことはできない。** 新しいドライバは現行本番 DB（11.2.0.1）をサポートしないため、DB 切り替えと同時に差し替える

### 4.2 文字コード（論点③）

新規構築の Oracle は既定が AL32UTF8 で、日本語 1 文字が SJIS の 2 バイトから 3 バイトになる。
詳細は `docs/charset_migration_note.md` に調査済み。要点:

- `EYE_OPE.IN_OUT`（「あやめ」）、`EYE_OPE.IN_TERM`（「１０日間」）が桁あふれする（テスト DB で ORA-12899 を再現済み）
- `VARCHAR2(4000)` の列（`EYE_KENSA2.CONT` / `EYE_OPE_PASS.CONT` / `EYE_OPE_RECORD.CONT`）は既存データが切り捨てられる恐れがある
- ドライバではなく **DB 側の列定義** の問題。移行ツール DMU で対応可能

### 4.3 DB 側設定（論点④）

アプリ改修ではなく DB 側の作業:

- 接続ユーザーのパスワード・ベリファイア（古い形式のままだと ORA-01017 / ORA-28040）
- DB リンク `@INNO.WORLD` の再作成（下記 5 の論点と連動）
- `tnsnames.ora` のホスト名・サービス名の更新
- ネイティブ暗号化・TLS を必須にする設定の有無

## 5. 電子カルテ製品の変更（論点⑤・最大の論点）

電子カルテ本体が別製品に変わるため、**電子カルテ側のテーブル構造やファイル連携を前提にしている部分は、Oracle のバージョンに関係なく動かなくなる可能性が高い。**
コード上で確認できた電子カルテ依存箇所は以下のとおり（影響範囲の精査は未実施）。

| 依存箇所 | 内容 |
|---|---|
| `DBConnectionString3`（`medb` / `inno_orcl`、`DB.Db3`） | 電子カルテ DB（MEDB スキーマ）への直接接続。テスト DB で EyeCenter の動作に使っているのは `M_USR`, `M_PATIENT`, `D_UKETSUKE`, `D_BYOUMEI`, `D_KENSA_RESULT`, `M_DEPT` / `M_DR` / `M_SHINKU` 等 |
| DB リンク `@INNO.WORLD` | `MedicalLibrary/Utility/Env.cs`, `Entity/PatBase.cs`, `Entity/PatOut.cs` から OPEN スキーマ経由で電子カルテ DB を参照 |
| `pat.csv`（`C:\innokarte\pat.csv`） | 電子カルテから渡される 50 列固定の CSV。ログインユーザー・患者 ID をここから取得（`MedicalLibrary/Entity/LoginUser.cs`） |
| `InnoUketsukeLib`（`DBConnectionString1` / `macs` / `wgs_odbc_orcl`、`DB.Db1`） | 現行電子カルテの受付連携ライブラリ |
| MedicalLibrary 全体 | `M_PATIENT` / `M_USR` / `D_UKETSUKE` 等への参照が多数ある。ただし MedicalLibrary は他アプリとの共用ライブラリのため、EyeCenter が実際に通る範囲は別途洗い出しが必要 |

EyeCenter 自身のデータ（OPEN スキーマの `EYE_*` 9 テーブル、定義は `schema/`）は電子カルテ製品に依存しないが、**新 DB サーバーへのデータ移行** は必要で、4.2 の文字コード問題がここに効く。

## 6. 更新が決まった時点での確認事項

### DB について（ベンダーへ確認）

```sql
select banner from v$version;                                    -- 4.1 の判断
select parameter, value from nls_database_parameters
 where parameter in ('NLS_CHARACTERSET','MAX_STRING_SIZE');      -- 4.2 の判断
```

- Oracle のバージョン
- `NLS_CHARACTERSET` と `MAX_STRING_SIZE`
- ネイティブ暗号化・TLS の必須設定の有無
- 接続ユーザー（`open` 等）と DB リンクの作成方針
- ホスト名・サービス名
- EyeCenter 用 OPEN スキーマ（`EYE_*`）を新 DB サーバーに置けるか

### 電子カルテについて（新ベンダーへ確認）

- 患者マスタ・利用者マスタ・受付・病名・検査結果を **どの手段で参照できるか**（DB 直接参照の可否、ビュー提供、API、CSV 連携など）
- 電子カルテから外部アプリを起動する際の連携方式（現行の `pat.csv` に相当するもの）
- 受付連携（現行 `InnoUketsukeLib` 相当）の要否と方式

## 7. 関連ドキュメント

- `docs/eyedata_oracle_client_removal_plan.md` — Oracle クライアント非依存化の計画と実施記録
- `docs/charset_migration_note.md` — 文字コードと桁あふれの調査
- `schema/` — EyeCenter 用テーブル（`EYE_*`）の本番定義
- `tools/Test-OracleManaged.ps1` — クライアント不要の疎通確認スクリプト（新 DB での事前確認に使える）
