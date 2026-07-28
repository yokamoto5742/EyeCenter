# EyeData データベーススキーマ定義

出力日時: 2026/07/28 10:43:22

対象: 本アプリケーションが読み書きする EYE_* 系テーブル（9テーブル）

> チェック制約は Oracle 11.2 のデータディクショナリから取得できないため含まれない。

## EYE_INTERVIEW

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | ID | NUMBER | NOT NULL |  |  |
| 2 | PATIENT_ID | NUMBER(9) | NOT NULL |  |  |
| 3 | IV_DATE | NUMBER(8) |  |  |  |
| 4 | CONT | VARCHAR2(2000) |  |  |  |
| 5 | STAFF | NUMBER(5) |  |  |  |
| 6 | SAVE_DATE | NUMBER(8) |  |  |  |
| 7 | SAVE_TIME | NUMBER(6) |  |  |  |
| 8 | STATUS | NUMBER(1) |  |  |  |
| 9 | PDF_SAVE | NUMBER(1) |  |  |  |
| 10 | PDF_DATE | NUMBER(8) |  |  |  |
| 11 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_INTERVIEW | ID |  |
| INDEX | PKEY_EYE_INTERVIEW | ID | UNIQUE |

## EYE_KENSA

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | PATIENT_ID | NUMBER(9) | NOT NULL |  |  |
| 2 | KENSA_ID | NUMBER(2) | NOT NULL |  |  |
| 3 | KENSA_DATE | NUMBER(8) | NOT NULL |  |  |
| 4 | CONT | VARCHAR2(3000) |  |  |  |
| 5 | STAFF | NUMBER(5) |  |  |  |
| 6 | SAVE_DATE | NUMBER(8) |  |  |  |
| 7 | SAVE_TIME | NUMBER(6) |  |  |  |
| 8 | PDF_SAVE | NUMBER(1) |  |  |  |
| 9 | PDF_DATE | NUMBER(8) |  |  |  |
| 10 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_KENSA | PATIENT_ID, KENSA_ID, KENSA_DATE |  |
| INDEX | PKEY_EYE_KENSA | PATIENT_ID, KENSA_ID, KENSA_DATE | UNIQUE |

## EYE_KENSA2

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | PATIENT_ID | NUMBER(9) | NOT NULL |  |  |
| 2 | KENSA_ID | NUMBER(2) | NOT NULL |  |  |
| 3 | KENSA_DATE | NUMBER(8) | NOT NULL |  |  |
| 4 | KENSA_SEQ | NUMBER(2) | NOT NULL |  |  |
| 5 | CONT | VARCHAR2(4000) |  |  |  |
| 6 | STAFF | NUMBER(5) |  |  |  |
| 7 | SAVE_DATE | NUMBER(8) |  |  |  |
| 8 | SAVE_TIME | NUMBER(6) |  |  |  |
| 9 | PDF_SAVE | NUMBER(1) |  |  |  |
| 10 | PDF_DATE | NUMBER(8) |  |  |  |
| 11 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_KENSA2 | PATIENT_ID, KENSA_ID, KENSA_DATE, KENSA_SEQ |  |
| INDEX | PKEY_EYE_KENSA2 | PATIENT_ID, KENSA_ID, KENSA_DATE, KENSA_SEQ | UNIQUE |

## EYE_OPE

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | ID | NUMBER | NOT NULL |  |  |
| 2 | PATIENT_ID | NUMBER(9) | NOT NULL |  |  |
| 3 | OPE_DATE | NUMBER(8) | NOT NULL |  |  |
| 4 | OPE_TIME | NUMBER(4) |  |  |  |
| 5 | OPE_KIND | NUMBER(2) |  |  |  |
| 6 | OPE_ROOM | VARCHAR2(4) |  |  |  |
| 7 | OPE_NAME | VARCHAR2(100) |  |  |  |
| 8 | DOCTOR | VARCHAR2(50) |  |  |  |
| 9 | PLAN_TIME | NUMBER(4) |  |  |  |
| 10 | ANES | VARCHAR2(20) |  |  |  |
| 11 | DIAG | VARCHAR2(100) |  |  |  |
| 12 | IN_OUT | VARCHAR2(8) |  |  |  |
| 13 | IN_ROOM | VARCHAR2(8) |  |  |  |
| 14 | IN_DATE | NUMBER(8) |  |  |  |
| 15 | IN_TIME | NUMBER(4) |  |  |  |
| 16 | IN_TERM | VARCHAR2(10) |  |  |  |
| 17 | EYE_R | NUMBER(1) |  |  |  |
| 18 | EYE_L | NUMBER(1) |  |  |  |
| 19 | HEIGHT | VARCHAR2(10) |  |  |  |
| 20 | WEIGHT | VARCHAR2(10) |  |  |  |
| 21 | INFECTION | VARCHAR2(100) |  |  |  |
| 22 | POST_DEAL | VARCHAR2(200) |  |  |  |
| 23 | PAST | VARCHAR2(2000) |  |  |  |
| 24 | COMT | VARCHAR2(200) |  |  |  |
| 25 | ALL_CHECK | NUMBER(1) |  |  |  |
| 26 | EXPLAIN | NUMBER(1) |  |  |  |
| 27 | EYE_DROP | NUMBER(1) |  |  |  |
| 28 | AGREE | NUMBER(1) |  |  |  |
| 29 | PRE_CHECK | NUMBER(1) |  |  |  |
| 30 | STAFF | NUMBER(5) |  |  |  |
| 31 | SAVE_DATE | NUMBER(8) |  |  |  |
| 32 | SAVE_TIME | NUMBER(6) |  |  |  |
| 33 | STATUS | NUMBER(1) |  |  |  |
| 34 | DEL_STAFF | NUMBER(5) |  |  |  |
| 35 | DEL_DATE | NUMBER(8) |  |  |  |
| 36 | DEL_TIME | NUMBER(6) |  |  |  |
| 37 | SHORT_OPE3 | NUMBER(1) |  |  |  |
| 38 | EARLIER_OK | NUMBER(1) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_OPE | ID |  |
| INDEX | PKEY_EYE_OPE | ID | UNIQUE |

## EYE_OPE_DOCTOR

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | ID | NUMBER | NOT NULL |  |  |
| 2 | PRE_CONT | VARCHAR2(3000) |  |  |  |
| 3 | DO_CONT | VARCHAR2(3000) |  |  |  |
| 4 | STAFF | NUMBER(5) |  |  |  |
| 5 | SAVE_DATE | NUMBER(8) |  |  |  |
| 6 | SAVE_TIME | NUMBER(6) |  |  |  |
| 7 | STATUS | NUMBER(1) |  |  |  |
| 8 | PDF_SAVE | NUMBER(1) |  |  |  |
| 9 | PDF_DATE | NUMBER(8) |  |  |  |
| 10 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_OPE_DOCTOR | ID |  |
| INDEX | PKEY_EYE_OPE_DOCTOR | ID | UNIQUE |

## EYE_OPE_PASS

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | ID | NUMBER | NOT NULL |  |  |
| 2 | CONT | VARCHAR2(4000) |  |  |  |
| 3 | STAFF | NUMBER(5) |  |  |  |
| 4 | SAVE_DATE | NUMBER(8) |  |  |  |
| 5 | SAVE_TIME | NUMBER(6) |  |  |  |
| 6 | STATUS | NUMBER(1) |  |  |  |
| 7 | PDF_SAVE | NUMBER(1) |  |  |  |
| 8 | PDF_DATE | NUMBER(8) |  |  |  |
| 9 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_OPE_PASS | ID |  |
| INDEX | PKEY_EYE_OPE_PASS | ID | UNIQUE |

## EYE_OPE_RECORD

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | ID | NUMBER | NOT NULL |  |  |
| 2 | CONT | VARCHAR2(4000) |  |  |  |
| 3 | STAFF | NUMBER(5) |  |  |  |
| 4 | SAVE_DATE | NUMBER(8) |  |  |  |
| 5 | SAVE_TIME | NUMBER(6) |  |  |  |
| 6 | STATUS | NUMBER(1) |  |  |  |
| 7 | PDF_SAVE | NUMBER(1) |  |  |  |
| 8 | PDF_DATE | NUMBER(8) |  |  |  |
| 9 | PDF_TIME | NUMBER(6) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_OPE_RECORD | ID |  |
| INDEX | PKEY_EYE_OPE_RECORD | ID | UNIQUE |

## EYE_OPE_RSV

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | OPE_DATE | NUMBER(8) | NOT NULL |  |  |
| 2 | OPE_WAKU | VARCHAR2(9) | NOT NULL |  |  |
| 3 | OPE_KIND | NUMBER(2) | NOT NULL |  |  |
| 4 | RSV_KIND | NUMBER(1) |  |  |  |
| 5 | COMT | VARCHAR2(20) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_OPE_RSV | OPE_DATE, OPE_WAKU, OPE_KIND |  |
| INDEX | PKEY_EYE_OPE_RSV | OPE_DATE, OPE_WAKU, OPE_KIND | UNIQUE |

## EYE_SUMMARY

所有者: OPEN

| # | 列名 | 型 | NULL | 既定値 | コメント |
|---|---|---|---|---|---|
| 1 | PATIENT_ID | NUMBER(9) | NOT NULL |  |  |
| 2 | DIAG | VARCHAR2(200) |  |  |  |
| 3 | KIND1 | VARCHAR2(60) |  |  |  |
| 4 | KIND2 | VARCHAR2(60) |  |  |  |
| 5 | KIND3 | VARCHAR2(60) |  |  |  |
| 6 | PLAN | VARCHAR2(400) |  |  |  |
| 7 | PASS | VARCHAR2(2000) |  |  |  |
| 8 | CONT1 | VARCHAR2(3000) |  |  |  |
| 9 | CONT2 | VARCHAR2(3000) |  |  |  |
| 10 | CONT3 | VARCHAR2(3000) |  |  |  |
| 11 | CONT4 | VARCHAR2(3000) |  |  |  |
| 12 | STAFF | NUMBER(5) |  |  |  |
| 13 | SAVE_DATE | NUMBER(8) |  |  |  |
| 14 | SAVE_TIME | NUMBER(6) |  |  |  |
| 15 | HIST | VARCHAR2(2000) |  |  |  |

| 種別 | 名称 | 列 | 備考 |
|---|---|---|---|
| PRIMARY KEY | PKEY_EYE_SUMMARY | PATIENT_ID |  |
| INDEX | PKEY_EYE_SUMMARY | PATIENT_ID | UNIQUE |

## シーケンス

| 所有者 | 名称 | 最小 | 最大 | 増分 | 次の採番 | キャッシュ |
|---|---|---|---|---|---|---|
| OPEN | EYE_INTERVIEW_SEQ | 1 | 999999999999999999999999999 | 1 | 653002 | 20 |
| OPEN | EYE_OPE_SEQ | 1 | 999999999999999999999999999 | 1 | 129573 | 20 |

