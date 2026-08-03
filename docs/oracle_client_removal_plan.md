# Oracle クライアント非依存化 計画

作成日: 2026-08-03

電子カルテ端末に今後 Oracle クライアントを導入しない方針のため、EyeCenter（および同居する関連アプリ）を
**Oracle クライアントのインストール無しで動作する構成**へ移行する。データベースは Oracle のまま使用する前提。

結論として、移行先は **ODP.NET マネージド・ドライバ（`Oracle.ManagedDataAccess`）** 一択であり、
コード変更は `using` の付け替えが主体で小さい。工数の大半は検証と配布物の整理になる。

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

**`Oracle.ManagedDataAccess` 19.x（例: 19.28）** を採用する。

- 100% マネージドのため、`Oracle.ManagedDataAccess.dll` を exe と同じフォルダに置くだけで動作する（xcopy 配置）。
  Oracle クライアント／`OraOps.dll`／`ORACLE_HOME`／レジストリ／`NLS_LANG` はすべて不要になる。
- **19.x を選ぶ理由**: 19.x は Oracle Database **11.2.0.4 以降**をサポートするが、21c 以降のドライバは
  サーバー 11.2 系を切っている。本番DBが 11.2 系である以上、19.x が安全圏。
- .NET Framework 4.8 はサポート範囲内（19.x の最低要件は .NET Framework 4.6.2）。

副次的な効果として、現在の「開発機 4.122 / 本番 2.112」というバージョン差に起因する
bindingRedirect 問題（`MedicalLibrary/docs/Oracle_DataAccess_Deploy_Troubleshooting.md` 参照）が根本的に消える。

## 3. 作業手順

### Phase 0 — 事前確認

DBに対して1回問い合わせるだけで済む。

```sql
select * from v$version;                                    -- 11.2.0.4 以上かどうか
select parameter, value from nls_database_parameters
 where parameter in ('NLS_CHARACTERSET','NLS_NCHAR_CHARACTERSET');
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

- **推奨（C案）: 接続文字列を完全記述子に書き換え、`tnsnames.ora` を不要にする。**
  コード変更ゼロで、端末に置くファイルも増えない。

  ```
  User Id=medb;Password=xxx;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=xxx)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xxx)));
  ```

- A案: `tnsnames.ora` を exe と同じフォルダ、または `TNS_ADMIN`（環境変数か `.config` の設定）で指定した場所に置く。
  既存の運用（エイリアス名のまま）を変えたくない場合はこちら。

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

3. **DBサーバが 11.2.0.4 未満だった場合**
   19.x マネージドは非対応。代替案は
   (a) Instant Client Basic Lite を xcopy 配置して非管理ドライバを継続（"インストール"は不要だがネイティブDLLは必要）、
   (b) DB 側にパッチを適用して 11.2.0.4 以上にする。

4. **`sqlnet.ora` の特殊設定**
   ネイティブ暗号化（ANO）などを使用している場合、マネージド側の対応可否を個別に確認する必要がある。

5. **x86 縛り**
   32bit 固定の理由だった「ネイティブ Oracle クライアントが 32bit」という制約は外れるが、
   `Interop.Excel` などの都合があるため x86 のまま据え置きを推奨（変更点を増やさない）。

6. **ネットワーク要件**
   端末からDBへ TCP 1521 の直結が引き続き必要。ポリシー上これが許容されない場合は、
   本計画ではなく「中間APIサーバーを設けて端末からはDB接続しない」という別アーキテクチャの検討になる。

## 5. 未確認事項（着手前に確定させる）

1. 本番 Oracle **サーバー**のバージョン（`11.2.0.x` の x）と `NLS_CHARACTERSET`
2. 電子カルテ端末から DB への **TCP 1521 直結**が引き続き許容されるか
3. 現在の `tnsnames.ora` の内容と `sqlnet.ora` の特殊設定の有無
4. 既存端末の Oracle クライアントを残すのか撤去するのか（移行期に新旧を共存させる必要があるか）
5. `Setting.xml`（接続文字列）を端末へ再配布できるか

1〜3 が判明すれば着手可能。コード改修は半日程度、残りは検証と配布物の整理。
