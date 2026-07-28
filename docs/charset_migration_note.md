# データベースキャラクタセットと桁あふれに関する調査メモ

作成日: 2026-07-28

患者台帳の「基本情報」で病棟名「あやめ」が入力できない事象の調査から、新電子カルテ本番DBへの移行時に想定される対応までをまとめる。

## 1. 発端の事象

テスト環境で、患者台帳 手術タブ「基本情報」の **入外** に病棟名「あやめ」を登録しようとすると `ORA-12899` となり、「あや」の2文字までしか入らなかった。

## 2. 原因

コードでも辞書ファイルでもなく、**テストDBの列がバイトセマンティクスのままだったこと**が原因である。

- 入外の格納先は `OPEN.EYE_OPE.IN_OUT`
- 定義は `VARCHAR2(8 BYTE)`（`user_tab_columns.CHAR_USED = 'B'` で確認）
- 本番は SJIS 系キャラクタセットのため「あやめ」は 6 バイト → 収まる
- ローカルテストDBは AL32UTF8 のため「あやめ」は 9 バイト → 溢れる

辞書ファイル `C:\Shinseikai\EyeData\EyeData.xml` の `<InOut>` は「外来 / あやめ / わかば / さくら」で正しく、こちらを短縮してはいけない。`FormPat.cs:590` が病棟名を文字列一致で判定しているため、値を縮めると入院日 (`InDateTimePicker`) が入力できなくなる。

```csharp
InDateTimePicker.Enabled = InOutBox.Text.Equals("わかば") || InOutBox.Text.Equals("さくら") || InOutBox.Text.Equals("あやめ");
```

## 3. 実施した対応（テストDBのみ）

```sql
-- OPEN スキーマで実行
ALTER TABLE EYE_OPE MODIFY (IN_OUT  VARCHAR2(8 CHAR));
ALTER TABLE EYE_OPE MODIFY (IN_TERM VARCHAR2(10 CHAR));
```

| 列 | 変更前 | 変更後 |
|---|---|---|
| `IN_OUT` | `VARCHAR2(8 B)` | `VARCHAR2(8 C)` (data_length 32) |
| `IN_TERM` | `VARCHAR2(10 B)` | `VARCHAR2(10 C)` (data_length 40) |

CHAR セマンティクスにすることで、本番（8バイト＝全角4文字）で通る値は必ずテストでも通るようになる。本番と完全に同じ上限で試験したい場合は `VARCHAR2(4 CHAR)` / `VARCHAR2(5 CHAR)` とする。

検証として「あやめ」(9バイト) と「１０日間」(12バイト) の INSERT が成功することを確認したうえで `rollback` 済み。既存データへの影響はない。

### 辞書値の総当たり結果

「基本情報」パネルに値を供給する辞書テーブルの全候補値を UTF-8 バイト長で検査したところ、溢れるのは以下の2列のみだった。

| 辞書 | 列 | 溢れる値 | UTF-8 | SJIS |
|---|---|---|---|---|
| InOut | `IN_OUT VARCHAR2(8)` | あやめ / わかば / さくら | 9 | 6 |
| InTerm | `IN_TERM VARCHAR2(10)` | １０日間 / １１日間 | 12 | 8 |

`OPE_ROOM(4)` `IN_ROOM(8)` `ANES(20)` `DIAG(100)` `POST_DEAL(200)` は候補値がすべて収まっている。

### schema/ の扱い

`schema/schema.csv` は本番のダンプであるため**変更しない**。結果としてテストDBの上記2列だけが本番定義と乖離する状態になる。なお `FormExport.Schema.cs` のスキーマ出力は `DATA_LENGTH` のみで `CHAR_USED` やキャラクタセットを含まないため、この差はダンプの diff には現れない。

## 4. 現行の本番DBについて

**修正は不要。**

本番では「あやめ」がすでに運用されている（辞書ファイルは本番配布物そのものであり、コードも当該文字列を前提にしている）。つまり本番の `EYE_OPE.IN_OUT VARCHAR2(8)` は「あやめ」を保持できている。今回の `ORA-12899` は、ローカルテストDBだけが AL32UTF8 であることによるテスト環境固有の現象である。

念のための確認クエリ（参照のみ）:

```sql
SELECT * FROM NLS_DATABASE_PARAMETERS WHERE PARAMETER = 'NLS_CHARACTERSET';

SELECT DISTINCT IN_OUT, LENGTHB(IN_OUT) FROM OPEN.EYE_OPE;
```

`NLS_CHARACTERSET` が `JA16SJIS` / `JA16SJISTILDE` 系で「あやめ」の `LENGTHB` が 6 なら現状のままで問題ない。

## 5. 新電子カルテ本番DBへの移行時の対応

移行先のキャラクタセットによって対応が変わる。

### 5.1 SJIS のままの場合

**移行対応は不要。** 列定義をそのまま持っていけば「あやめ」も現在の全データも収まる。

### 5.2 AL32UTF8 の場合

日本語が1文字2バイト→3バイトになるため、`VARCHAR2(n)` の桁を見直す必要がある。

#### (1) 狭い列 — CHAR セマンティクス化または1.5倍に拡張

`schema/schema.csv` から、日本語が入る可能性のある32バイト以下の列を洗い出した結果:

| テーブル | 列 | 現定義 | SJIS | AL32UTF8 |
|---|---|---|---|---|
| EYE_OPE | `IN_OUT` | VARCHAR2(8) | 全角4字 | **全角2字** ← 「あやめ」不可 |
| EYE_OPE | `IN_TERM` | VARCHAR2(10) | 全角5字 | **全角3字** ← 「１０日間」不可 |
| EYE_OPE | `IN_ROOM` | VARCHAR2(8) | 全角4字 | 全角2字 |
| EYE_OPE | `OPE_ROOM` | VARCHAR2(4) | 全角2字 | 全角1字 |
| EYE_OPE | `ANES` | VARCHAR2(20) | 全角10字 | 全角6字 |
| EYE_OPE | `HEIGHT` / `WEIGHT` | VARCHAR2(10) | 全角5字 | 全角3字 |
| EYE_OPE_RSV | `OPE_WAKU` | VARCHAR2(9) | 全角4字 | 全角3字 |
| EYE_OPE_RSV | `COMT` | VARCHAR2(20) | 全角10字 | 全角6字 |

現行の辞書値で実際に溢れるのは `IN_OUT` と `IN_TERM` だけだが、他も余裕がほぼ無いため一括で `VARCHAR2(n CHAR)` にするのが定石。

#### (2) 広い列 — こちらの方が厄介

以下の列が `VARCHAR2(4000)` で、標準の VARCHAR2 上限ちょうどである。

- `EYE_KENSA2.CONT`
- `EYE_OPE_PASS.CONT`
- `EYE_OPE_RECORD.CONT`

`4000 CHAR` にすると最大12000バイトとなり上限を超えるため、`MAX_STRING_SIZE = EXTENDED`（32767バイト）にするか CLOB 化が必要になる。既存データも SJIS 4000バイト分が UTF-8 で最大6000バイトに膨らむため、**移行時に切り捨てが起きないか実データの `LENGTHB` 分布を事前確認すること。**

`EYE_SUMMARY.CONT1〜4`(3000)、`EYE_OPE_DOCTOR.PRE_CONT`/`DO_CONT`(3000)、`EYE_KENSA.CONT`(3000)、`EYE_INTERVIEW.CONT`(2000)、`EYE_OPE.PAST`(2000)、`EYE_SUMMARY.PASS`/`HIST`(2000) も同様の観点で確認が要る。

#### 移行ツール

Oracle の **DMU (Database Migration Assistant for Unicode)** が、この「桁溢れ検出」と「列拡張」を自動で行う。手作業より確実。

## 6. SJIS と AL32UTF8 のどちらがメジャーか

**新規構築なら AL32UTF8 が圧倒的多数。**

- Oracle は 12.2 以降、DBCA の既定キャラクタセットを AL32UTF8 にしており、新規DBは基本これになる
- Oracle 公式も全新規デプロイに Unicode (AL32UTF8) を推奨している。`JA16SJIS` / `JA16SJISTILDE` はサポートこそ継続しているが事実上レガシー扱い
- 絵文字・異体字・外字対応や他システム連携の要求から、Unicode 以外を選ぶ理由がほぼ無い

ただし**日本の医療システムで稼働中の既存電子カルテは SJIS が今も多い**。古い環境をそのまま更新してきた結果である。したがって「新電子カルテ」が一から構築されるなら AL32UTF8、既存DBを引き継ぐ形なら SJIS のまま、という見当になる。

ベンダーへの確認は次の1行で足りる。

```sql
SELECT * FROM NLS_DATABASE_PARAMETERS WHERE PARAMETER = 'NLS_CHARACTERSET';
```

## Geminiの解説

AL32UTF8 だった場合は、併せて `MAX_STRING_SIZE` の設定値（`STANDARD` か `EXTENDED` か）も確認しておくと 5.2(2) の判断ができる。

データベースの Unicode（AL32UTF8）移行において、`VARCHAR2(4000)` の列は最も障害（データ溢れエラーや切り捨て）が発生しやすい「罠」です。

ご提示の通り、`VARCHAR2(4000 CHAR)` と定義すると、UTF-8では最大12,000バイト（1文字最大3バイト計算）を消費する可能性が生じ、標準Oracleの `VARCHAR2` の上限である4,000バイトを超えて定義エラー（`ORA-01450` 等）や格納エラーが発生します。

この問題の全体像と具体的な調査手順、2つの対応策の比較を解説します。

---

## 1. なぜ問題になるのか？（背景）

* **SJIS（Shift_JIS / JA16SJIS）:** 全角文字（日本語）は **1文字 = 2バイト**。
* 4,000バイトの領域には、最大で日本語 **2,000文字** が入ります。


* **AL32UTF8（UTF-8）:** 日本語（ひらがな・漢字・カタカナ等）は **1文字 = 3バイト**。
* SJIS時代に「日本語2,000文字（4,000バイト）」ぴったりで入っていたデータを AL32UTF8 に移行すると、**6,000バイト** に膨れ上がります。


* **標準Oracleの制約:** `MAX_STRING_SIZE = STANDARD` の場合、`VARCHAR2` の物理上限は **4,000バイト** です。
* そのため、移行データが 4,000バイトを超えると **`ORA-12899`（値が大きすぎます）** で移送が失敗するか、強制移行ツール等を使うと**データの後ろが切り捨てられる**事故につながります。



---

## 2. 事前調査：実データの `LENGTHB` 分布確認 SQL

移行前に「実際に4,000バイトを超えて膨らむレコードがいくつ存在するか」を特定する必要があります。

以下のクエリで、現在の環境における最大バイト数や、UTF-8化後に4,000バイトを超えるリスクのあるレコード件数を把握します。

### ① 対象テーブルごとの最大長とリスク件数の抽出

```sql
SELECT 
    'EYE_KENSA2' AS TABLE_NAME,
    MAX(LENGTHB(CONT)) AS CURRENT_MAX_BYTES,
    MAX(LENGTH(CONT)) AS MAX_CHAR_COUNT,
    -- UTF-8化時の概算バイト数（日本語文字数を3バイト換算）
    MAX(LENGTHB(CONT) + LENGTH(CONT)) AS ESTIMATED_UTF8_BYTES,
    -- UTF-8化時に4000バイトを超える可能性がある件数
    COUNT(CASE WHEN (LENGTHB(CONT) + LENGTH(CONT)) > 4000 THEN 1 END) AS DANGER_ROW_COUNT
FROM EYE_KENSA2
UNION ALL
SELECT 
    'EYE_OPE_PASS',
    MAX(LENGTHB(CONT)),
    MAX(LENGTH(CONT)),
    MAX(LENGTHB(CONT) + LENGTH(CONT)),
    COUNT(CASE WHEN (LENGTHB(CONT) + LENGTH(CONT)) > 4000 THEN 1 END)
FROM EYE_OPE_PASS
UNION ALL
SELECT 
    'EYE_OPE_RECORD',
    MAX(LENGTHB(CONT)),
    MAX(LENGTH(CONT)),
    MAX(LENGTHB(CONT) + LENGTH(CONT)),
    COUNT(CASE WHEN (LENGTHB(CONT) + LENGTH(CONT)) > 4000 THEN 1 END)
FROM EYE_OPE_RECORD;

```

> **補足（UTF-8概算ロジック）:**
> SJISでの `LENGTHB` と `LENGTH`（文字数）の差分が「全角文字数」です。
> `全角文字数 × 1バイト` を元の `LENGTHB` に加算すると、UTF-8（1文字3バイト）にした際のおおよそのバイト数になります。

---

## 3. 2つの対応策と選び方

調査結果（`DANGER_ROW_COUNT`）に応じて、以下のいずれかの技術的アプローチをとります。

### アプローチ A: `MAX_STRING_SIZE = EXTENDED`（32767バイト拡張）

Oracle 12c から導入された機能で、`VARCHAR2` の物理上限を 4,000バイト から **32,767バイト** へ拡大します。

* **メリット:**
* 型が `VARCHAR2` のままなので、**アプリ側のSQL（`LIKE` 検索、文字列結合、`SUBSTR` 等）の修正が不要**。
* データモデルの変更（CLOB化）を行わずに済む。


* **デメリット / 留意点:**
* DB全体の初期化パラメータ変更（`UTL32K.SQL` の実行など、PDB/DBレベルでの移行作業）が必要。
* 一度 `EXTENDED` に変更すると、**`STANDARD`（4,000バイト）に戻すことは不可**。
* 4,000バイトを超える文字列データは内部的に Out-of-line LOB として保持されるため、インデックス追加などの制約が生じる場合がある。



### アプローチ B: 該当列の `CLOB` 化

該当の `CONT` 列（内容・記録等の自由記述欄）の型自体を `CLOB`（Character Large Object）に変更します。

* **メリット:**
* 文字数制限の心配が完全に不要になる（最大 4GB）。
* DB全体のパラメータ変更を行わず、特定のテーブル・列だけ局所的に対処できる。


* **デメリット / 留意点:**
* **アプリ側の改修が必要になる可能性が高い。**
* `CLOB` 型に対しては、標準的な文字列比較（`WHERE CONT = '...'`）や一部の文字列関数、連携プログラム（Pro*C、JDBC、ETLツール等）でハンドリングの変更が必要になる。



---

## 4. 推奨する進め方（フロー）

```
[事前調査 SQLの実行]
       │
       ├── DANGER_ROW_COUNT が 0 件 ＆ 今後も全角1,300文字（約4,000バイト）を超える運用がない
       │     └─► 対応: 定義を `VARCHAR2(4000 BYTE)` のまま維持（または安全のため要件を見直し）
       │
       └── 実際に 4,000 バイトを超えるデータが存在する（または今後の業務で膨らむ）
             │
             ├── アプリ側の CLOB 対応改修コストを避けたい / システム全体を最適化したい
             │     └─► 対応: 【アプローチ A】 `MAX_STRING_SIZE = EXTENDED` を採用
             │
             └─► アプリ側の改修が容易 / 特定のテーブルのみで完結させたい
                   └─► 対応: 【アプローチ B】 列を `CLOB` 型へ定義変更

```

まずは対象3テーブルの事前調査SQLを実行し、「実際に4,000バイト（UTF-8換算）を超えるレコードが何件存在するのか」を把握することから始めるのが最優先となります。
