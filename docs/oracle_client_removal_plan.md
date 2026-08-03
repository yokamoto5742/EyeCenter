# Oracle クライアント非依存化 計画

作成日: 2026-08-03 / 最終更新: 2026-08-03

電子カルテ端末に今後 Oracle クライアントを導入しない方針のため、EyeCenter（および同居する関連アプリ）を
**Oracle クライアントのインストール無しで動作する構成**へ移行する。データベースは Oracle のまま使用する前提。

結論として、移行先は **ODP.NET マネージド・ドライバ（`Oracle.ManagedDataAccess`）** 一択であり、
コード変更は `using` の付け替えが主体で小さい。工数の大半は検証と配布物の整理になる。

> **状況（2026-08-03）**: 19.x（`4.122.19.1`）でコード改修を完了し、**本番環境での疎通に成功**。
> 本計画最大の懸念だった「本番DB 11.2.0.1.0 に対して 19.x が未認定」という点は実務上クリアされた。
> 残るのは日本語ラウンドトリップを中心とした機能検証と後片付け。詳細は **6. 実施記録** を参照。

---

## 1. 現状の依存

現在は非管理ドライバ `Oracle.DataAccess`（ODP.NET Unmanaged）を使用しており、
`OraOps*.dll` などのネイティブDLL＝**Oracle クライアントのインストールが必須**である。
またレジストリ／`ORACLE_HOME`／`NLS_LANG`／`tnsnames.ora` といった端末側の環境にも依存する。

grep で洗い出した依存箇所は以下のとおり。

| 場所 | Oracle 依存の実体 |
|---|---|
| `MedicalLibrary/Utility/DB.cs` | `OracleConnection` / `OracleCommand` / `OracleDataReader` / `BindByName` / `InitialLONGFetchSize` / `Command.Cancel` |
| `MedicalLibrary/Entity/StdClass.cs`(39箇所), `Entity/KarteLog.cs`, `Entity/Dict.cs`, `Entity/LoginUser.cs`, `Boundary/LoginPrompt.cs` | `OracleDbType.{Decimal,Varchar2,Char,Date,NVarchar2}` / `reader.GetOracleValue()` |
| `InnoUketsukeLib` | 自前の `Utility/DB.cs` ほか計5ファイルで同様（`Entity/StdClass.cs`, `StdDbClass.cs`, `StdDbColumn.cs`, `SOAP.cs`） |
| `EyeCenter` 本体 | **Oracle の型を一切使っていない**（`EyeCenter.csproj:94-97` のコメントのとおり）。依存は csproj の参照・`App.config` の bindingRedirect・`Program.cs` の `ResolveOracleAssembly` のみ |
| `CanonRKF1` / `NidekARK1` | Oracle 参照なし（MedicalLibrary 経由）。再ビルドのみ |

使用している Oracle API は上記が全てで、**いずれもマネージド・ドライバに同名で存在する**。
`TransactionScope`（分散トランザクション）・`OracleBulkCopy`・UDT・`RefCursor`・OS認証（`/@`）は
いずれも未使用であることを確認済み。したがってコード側に技術的障壁は無い。

## 2. 移行先の選定

`Oracle.ManagedDataAccess`（100% マネージド）を採用する方針は変わらない。
`Oracle.ManagedDataAccess.dll` を exe と同じフォルダに置くだけで動作し（xcopy 配置）、
Oracle クライアント／`OraOps.dll`／`ORACLE_HOME`／レジストリ／`NLS_LANG` はすべて不要になる。

副次的な効果として、現在の「開発機 4.122 / 本番 2.112」というバージョン差に起因する
bindingRedirect 問題（`MedicalLibrary/docs/Oracle_DataAccess_Deploy_Troubleshooting.md` 参照）が根本的に消える。

### 2.1 バージョン選定（本番DB = 11.2.0.1.0 判明後の修正）

本番サーバーは **11.2.0.1.0**（11.2.0.4 **未満**）と判明した。
Oracle のクライアント／サーバー相互運用マトリクス（Doc ID 207303.1）では、
19c・18c・12.2 のクライアントはいずれも **サーバー 11.2.0.4 以上**が条件のため、
当初案の 19.x は **11.2.0.1 に対しては未認定**である。11.2.0.1 を正式サポートする最後のマネージド・ドライバは
**12.1.0.2（`Oracle.ManagedDataAccess.dll` 4.121.2.0）** となる。

ただし「未認定」は「動かない」とは別で、TTC プロトコル上 11.2.0.1 と 11.2.0.4 に差は無く、
**同院内の別アプリが既に `Oracle.ManagedDataAccess.dll` で同DBに接続できている実績がある**。
したがって選定は次の優先順で行う。

| 優先 | 採用するもの | 判断理由 |
|---|---|---|
| 1 | **別アプリが実際に使っている DLL と同一バージョン** | 同DB・同ネットワークでの動作実績が最強の根拠。後述のとおり同一フォルダに同居する以上、バージョンは揃えざるを得ない |
| 2 | 12.1.0.2（4.121.2.0） | 11.2.0.1 を正式サポートする唯一の選択肢。認定を重視する場合 |
| 3 | 19.x（4.122.19.x） | 実績はあるが 11.2.0.1 では未認定。Oracle へ問い合わせる事態になった場合に unsupported 扱いとなるリスクを許容できる場合のみ |

**→ 2026-08-03、3 の 19.x（`4.122.19.1`）を採用し、本番環境での疎通に成功した。**
未認定という位置づけは変わらないため、Oracle へ問い合わせる事態になった場合は
「サポート対象外の組み合わせ」と扱われる点だけ認識しておく。実運用上の障害が出た場合の退避先は
2 の 12.1.0.2（DLL 差し替えと csproj の `Version=` 修正のみで戻せる）。

**重要**: EyeCenter / CanonRKF1 / NidekARK1 / InnoUketsuke受付 は `C:\Shinseikai\EyeData` に同居するため、
`Oracle.ManagedDataAccess.dll` は **全アプリで 1 本・同一バージョン**にする必要がある。
別アプリが 19.x を使っているならこちらも 19.x、12.1 なら 12.1 に揃える。
揃えられない場合のみ bindingRedirect を書くことになり、現在の問題が形を変えて再発する。

なお開発機にある DLL は以下のとおりで、**どちらもそのままでは採用しない**（21c は 12.1 未満のサーバーを完全に切っているため特に不可）。

- `C:\oracle\odac32\odpm\odp.net\managed\common\Oracle.ManagedDataAccess.dll` → 4.122.**21**.1（21c, サーバー 12.1 以上が条件）
- `OneDrive\...\C#\EyeData\Oracle.ManagedDataAccess.dll` → 4.122.**19**.1（19c）

.NET Framework 4.8 はいずれのバージョンでもサポート範囲内（12.1.0.2 は .NET 4.0 以上、19.x は 4.6.2 以上）。

## 3. 作業手順

### Phase 0 — 事前確認

DBに対して1回問い合わせるだけで済む。

```sql
select * from v$version;                                    -- 済: 11.2.0.1.0
select parameter, value from nls_database_parameters
 where parameter in ('NLS_CHARACTERSET','NLS_NCHAR_CHARACTERSET');

-- 12c 以降のクライアントを使う場合の必須確認（リスク7）。'11G' が含まれているか
select username, password_versions from dba_users
 where username in ('MEDB','（実際に使用する接続ユーザー）');
```

併せて本番端末の `tnsnames.ora` / `sqlnet.ora` の内容を控える
（ホスト・ポート・サービス名、ネイティブ暗号化(ANO)や `SQLNET.AUTHENTICATION_SERVICES` の有無）。

### Phase 1 — DLL の配置

`C:\Shinseikai\EyeData` に `Oracle.ManagedDataAccess.dll` を1本だけ置き、
3プロジェクト（EyeCenter / MedicalLibrary / InnoUketsukeLib）すべてが同一ファイルを `HintPath` で参照する。
旧形式（非SDKスタイル）の csproj のため、NuGet 復元は本体ビルドに持ち込まない。
全プロジェクトが同一バージョンを参照していれば bindingRedirect は不要。

### Phase 2 — MedicalLibrary（本丸）

対象6ファイルの using を置換し、csproj の参照を差し替える。

```diff
-using Oracle.DataAccess.Client;
+using Oracle.ManagedDataAccess.Client;
```

- 対象: `Utility/DB.cs`, `Entity/StdClass.cs`, `Entity/KarteLog.cs`, `Entity/Dict.cs`, `Entity/LoginUser.cs`, `Boundary/LoginPrompt.cs`
- **各ファイルの既存エンコーディング（Shift-JIS / UTF-8 BOM）を必ず維持する。** `StdClass.cs` は Shift-JIS。
- `MedicalLibrary.csproj:60-63` の `Oracle.DataAccess`（11.2 クライアント配下への HintPath）を差し替える。

### Phase 3 — InnoUketsukeLib

同じ using 置換と、`InnoUketsukeLib.csproj:36-39`（`C:\app\Administrator\...` への HintPath）の差し替え。
EyeCenter からは実行時オプショナル扱いだが、読み込まれた時点でクライアント依存が復活するため必ず一緒に移行する。

### Phase 4 — EyeCenter 本体（削除が中心）

| ファイル | 作業 |
|---|---|
| `EyeCenter.csproj:98-101` | `Oracle.DataAccess` 参照をマネージドへ置換（コメント94-97も現状に合わせて更新） |
| `App.config` | `Oracle.DataAccess` の `<dependentAssembly>` bindingRedirect ブロックを**削除** |
| `Program.cs:17-20, 71-80` | `ResolveOracleAssembly` と `AssemblyResolve` 登録を**削除**（バージョン不一致問題自体が消えるため） |
| `EyeCenter.Tests/App.config` | 同じく bindingRedirect を削除 |
| `EyeData.exe.config.production` | 開発機用/本番用の config を分ける理由が無くなるため**廃止**（`deploy.ps1` のコピー処理も削除） |

### Phase 5 — 接続先の解決方法

現在の接続文字列（`MedicalLibrary/Utility/LibSettings.cs`、実体は `Setting.xml`）は
`Data Source=wgs_odbc_orcl` / `macs_open` / `inno_orcl` の **TNSエイリアス指定**で、`tnsnames.ora` に依存している。

- **採用（A案）: `tnsnames.ora` を exe と同じフォルダ（`C:\Shinseikai\EyeData`）に置く。**
  接続文字列も `Setting.xml` も変更不要で、エイリアス名のまま運用を継続できる。
  **開発機での実測（下記 6. 参照）で、この配置なら解決できることを確認済み**。
  逆に `tnsnames.ora` が無く `TNS_ADMIN` も未設定だと起動時に例外になる。

- C案: 接続文字列を完全記述子に書き換え、`tnsnames.ora` を不要にする。
  端末に置くファイルは減るが、`Setting.xml` の再配布が必要になる。

  ```
  User Id=medb;Password=xxx;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=xxx)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xxx)));
  ```

いずれの案でも Oracle クライアントのインストールは不要。

### Phase 6 — 検証

コード改修より、こちらが実質の工数になる。

1. 起動 → ログイン（`LoginPrompt`）
2. 患者台帳の読み書き（基本情報・問診・検査・手術・サマリの登録）
3. `FormExport` のスキーマ取得（LONG 列 = `InitialLONGFetchSize` の経路）
4. 検索のキャンセル（`SearchTask` → `Command.Cancel`）
5. DBリンク（`@INNO.WORLD`）越しの取得
6. コマンドタイムアウト（`DbCommandTimeout`）
7. **日本語のラウンドトリップ**（下記リスク1）— 既存データの読み取り結果比較と、書き込み→読み戻しの一致確認
8. `EyeCenter.Tests` の実行（本体を x86 Debug でビルドしてから `dotnet test`）

### Phase 7 — 配布

- 配布物から `Oracle.DataAccess.dll` を削除し、`Oracle.ManagedDataAccess.dll` を追加。
- `deploy.ps1` / `deploy-production.ps1` を更新。
- `MedicalLibrary/docs/Oracle_DataAccess_Deploy_Troubleshooting.md` は役目を終えるため、廃止済みである旨を追記。
- **CanonRKF1 / NidekARK1 / InnoUketsuke受付は同じフォルダに同居するため、全て再ビルドして同時に入れ替える。**
  片方だけ差し替えると、旧 `Oracle.DataAccess` を要求する側が起動できなくなる。

## 4. リスクと対策

1. **文字コード（最重要）**
   本番DBは SJIS 系（`docs/charset_migration_note.md` で確認済み）。
   マネージド・ドライバは `NLS_LANG` に非依存で自前に変換を行うため、これまで `NLS_LANG` 経由で
   暗黙に変換されていた文字（半角カナ・㈱等の機種依存文字・波ダッシュ）で挙動が変わる可能性がある。
   → Phase 6-7 のラウンドトリップ試験を必ず実施する。

2. **`GetOracleValue(i).ToString() != "null"` による NULL 判定**（`MedicalLibrary/Entity/StdClass.cs:492`）
   ドライバ実装依存の文字列比較であり、移行時に最も壊れやすい箇所。
   → 移行前に `reader.IsDBNull(i)` ベースへ直しておくのが安全。

3. ~~**DBサーバが 11.2.0.4 未満（＝実際に 11.2.0.1.0 だった）**~~ → **解消**（2026-08-03）
   19.x マネージドは 11.2.0.1 に対して未認定だが、**本番環境で実際に接続できることを確認済み**。
   認定外である事実は残るため、退避策だけ記録しておく:
   (a) 12.1.0.2（4.121.2.0）へ落とす — 11.2.0.1 を正式サポートする最後のマネージド・ドライバ。
       DLL 差し替えと csproj の `Version=` 修正のみ、
   (b) Instant Client Basic Lite を xcopy 配置して非管理ドライバを継続（"インストール"は不要だがネイティブDLLは必要）、
   (c) DB 側にパッチを適用して 11.2.0.4 以上にする。

4. **`sqlnet.ora` の特殊設定**
   ネイティブ暗号化（ANO）などを使用している場合、マネージド側の対応可否を個別に確認する必要がある。

5. **x86 縛り**
   32bit 固定の理由だった「ネイティブ Oracle クライアントが 32bit」という制約は外れるが、
   `Interop.Excel` などの都合があるため x86 のまま据え置きを推奨（変更点を増やさない）。

6. **ネットワーク要件**
   端末からDBへ TCP 1521 の直結が引き続き必要。ポリシー上これが許容されない場合は、
   本計画ではなく「中間APIサーバーを設けて端末からはDB接続しない」という別アーキテクチャの検討になる。

7. ~~**`ORA-28040: 一致する認証プロトコルがありません`**~~ → **顕在化せず**（2026-08-03）
   12c 以降のクライアントは 11G 以上のパスワード・ベリファイアを要求するため、
   10G ベリファイアしか持たないユーザーだとログインが失敗する懸念があったが、本番接続は成功した。
   今後 DB 側でユーザーを作り直した場合は再発しうるので、その際は `dba_users.password_versions` に
   `11G` があるか確認すること。無ければ `SQLNET.ALLOWED_LOGON_VERSION=11`（サーバー `sqlnet.ora`）にした上で
   同じパスワードで `alter user ... identified by ...` を実行しベリファイアを再生成する。

## 5. 未確認事項

本番接続が成功したことで、着手前に確定させるべき事項は解消した。残りは運用判断のみ。

| # | 事項 | 状態 |
|---|---|---|
| 1 | 本番 Oracle サーバーのバージョン | **`11.2.0.1.0`**（2026-08-03 確認） |
| 2 | 19.x マネージドで本番DBに接続できるか | **接続成功**（2026-08-03 確認）— これが最大の懸念だった |
| 3 | 接続ユーザーの `password_versions` に `11G` が含まれるか | 接続成功のため事実上クリア（リスク7） |
| 4 | 電子カルテ端末から DB への TCP 1521 直結 | 接続成功のため許容されている |
| 5 | `tnsnames.ora` / `sqlnet.ora` の特殊設定 | 接続成功のため特殊設定は無いか、マネージドで問題にならない範囲 |
| 6 | 本番DBの `NLS_CHARACTERSET` | **未確認**（リスク1のラウンドトリップ検証と併せて確認する） |
| 7 | 既存端末の Oracle クライアントを残すのか撤去するのか | **運用判断待ち**。撤去すると `ORACLE_HOME` 配下の `tnsnames.ora` が消えるため、先に `C:\Shinseikai\EyeData` へコピーしておくこと |

---

## 6. 実施記録（2026-08-03）

19.x（`Oracle.ManagedDataAccess` 4.122.19.1）でコード改修を実施し、**本番環境での疎通に成功した**。
これにより「11.2.0.1 では 19.x が未認定」という本計画最大の懸念は実務上クリアされ、
Oracle クライアントに依存しない構成が成立することが確認できた。

### 実施したこと

| Phase | 内容 |
|---|---|
| 1 | `C:\Shinseikai\EyeData\Oracle.ManagedDataAccess.dll`（4.122.19.1, MSIL）を配置。3プロジェクトが同一ファイルを `HintPath` 参照 |
| 2 | MedicalLibrary 8ファイルの `using` 置換＋`MedicalLibrary.csproj` の参照差し替え |
| 3 | InnoUketsukeLib 5ファイルの `using` 置換＋`InnoUketsukeLib.csproj` の参照差し替え |
| 4 | EyeCenter: csproj 参照差し替え／`App.config`・`EyeCenter.Tests/App.config` の bindingRedirect 削除／`Program.cs` の `ResolveOracleAssembly` 削除／`EyeData.exe.config.production` 削除 |
| 7 | `deploy.ps1`・`deploy-production.ps1` から本番用 config の受け渡しを削除。CanonRKF1 / NidekARK1 も再ビルド |

`using` の置換はバイト単位で行い、Shift-JIS / UTF-8(BOM) の混在エンコーディングと改行コードを保持した。

### 検証結果

**本番環境**

- **19.x マネージド・ドライバで本番DB（11.2.0.1.0）への接続に成功**（Oracle クライアント非依存で動作することを確認）

**開発機**

- ビルド: MedicalLibrary / InnoUketsukeLib / EyeCenter / CanonRKF1 / NidekARK1 すべて成功
- `dotnet test`: **81 合格 / 0 失敗**（移行前は 11 失敗 — `Oracle.DataAccess` 2.112 がテストホストで読めなかったため。移行で解消）
- `tools\Test-OracleManaged.ps1` でローカル Oracle 23ai Free に接続成功。日本語ラウンドトリップも一致
- EyeData.exe を起動し患者台帳の表示まで到達

### 判明したこと: `tnsnames.ora` は exe と同じフォルダに必要

`TNS_ADMIN` 未設定・exe フォルダに `tnsnames.ora` 無しの状態で起動すると、DB接続で例外ダイアログになる。
`tnsnames.ora` を exe と同じフォルダに置くと正常に起動する。
→ **本番配布時に `C:\Shinseikai\EyeData\tnsnames.ora` を必ず配置すること**（Phase 5 のA案）。

### 残作業

疎通は取れたので、残るのは「データが正しく読み書きできるか」の機能検証と後片付け。

1. **Phase 6 の機能検証** — **未実施**。特に優先度が高い順に:
   1. **日本語のラウンドトリップ**（リスク1）。本番DBは SJIS 系のため、半角カナ・㈱等の機種依存文字・波ダッシュで
      既存データの読み取り結果比較と、書き込み→読み戻しの一致確認を行う。
      併せて本番の `NLS_CHARACTERSET` を控える（未確認事項6）
   2. NULL 判定（下記2）が絡む箇所の実データ確認
   3. `FormExport` のスキーマ取得（LONG 列 = `InitialLONGFetchSize` の経路）
   4. 検索のキャンセル（`SearchTask` → `Command.Cancel`）
   5. DBリンク（`@INNO.WORLD`）越しの取得
   6. コマンドタイムアウト（`DbCommandTimeout`）
2. リスク2（`GetOracleValue(i).ToString() != "null"`, `MedicalLibrary/Entity/StdClass.cs`）の `IsDBNull` 化 — **未着手**。
   ドライバ実装依存の文字列比較であり、移行後に最も静かに壊れやすい箇所
3. **本番端末への `tnsnames.ora` 配置の恒久化** — Oracle クライアントを撤去する前に
   `%ORACLE_HOME%\network\admin\tnsnames.ora` を `C:\Shinseikai\EyeData` へコピーしておく
4. 旧 `Oracle.DataAccess.dll` の撤去（配布フォルダ・本番端末の双方）
5. `MedicalLibrary/docs/Oracle_DataAccess_Deploy_Troubleshooting.md` に廃止済みである旨を追記 — **未着手**
