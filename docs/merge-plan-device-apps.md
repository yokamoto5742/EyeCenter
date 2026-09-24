# NidekARK1.exe / CanonRKF1.exe の EyeData.exe 組み込み計画（2026-09-24）

> **ステータス: 実装済み（2026-09-24）** — コード変更（手順1〜3）とビルド検証まで完了。本番環境での手動テスト（5章）と配備・後片付け（手順5・6）は未実施。

## 1. 目的と方針

外部 EXE として起動している `NidekARK1.exe` と `CanonRKF1.exe` を廃止し、EyeData.exe の中でそれぞれのフォームを開くようにする。

| 前提（2026-09-24 確認済み） | 内容 |
|---|---|
| 起動経路 | 両 EXE とも EyeData 以外から起動している端末はない（EyeData の MainForm のボタンからのみ） |
| 対象 | NidekARK1 と CanonRKF1 の両方 |

## 2. 調査結果

### 2.1 両 EXE の中身

`NidekARK1` / `CanonRKF1` リポジトリのコードは `Program.cs`（約40行）のみで、内容は同じ。

1. 二重起動時はウィンドウタイトル（`"Nidek ARK1"` / `"Canon RF-K1 通信プログラム"`）で既存ウィンドウを探して前面化する
2. それ以外は `MedicalLibrary.Agent.NidekARK1ListForm` / `CanonRKF1Form` を `Application.Run` する

`Form1.cs`、`App.config`（supportedRuntime のみ）、`Properties/`（テンプレートのまま）に移植すべきものはない。

### 2.2 フォーム本体（MedicalLibrary）

| フォーム | 動作 | 組み込み時の問題 |
|---|---|---|
| `NidekARK1ListForm` | 日付を選び `c:\demo\rkt\txt` のファイル一覧を表示・変換する。タイマーやスレッドは使わない | なし |
| `CanonRKF1Form` | シリアルポートで機器からデータを受信し `ref.dat` に書く。`DataReceived`（スレッドプールで実行）と 10 秒間隔のタイマーを使う | **あり（2.3）** |

### 2.3 CanonRKF1Form の問題（組み込むと影響が大きくなる）

1. **× ボタンで閉じるとシリアルポートが開いたまま残る。** ポートを閉じるのは「終了」ボタン（`CloseButton_Click`）だけ。
   - 現在: 閉じるとプロセスごと終了するので、OS がポートを解放する
   - 組み込み後: EyeData を終了するまでポートが開いたまま残る。そのため
     - フォームを開き直して「接続」すると、ポートが使用中で `port.Open()` が例外になる
     - 閉じた後に機器からデータが届くと、`DataReceived` が破棄済みの `RsvBox.Invoke` を呼んで例外になる。この例外はスレッドプール上で起きるので、**EyeData ごと落ちる**
2. **`DataReceived` の catch 節で `writer.Close()` を呼んでいる。** 一度もファイルを書いていない状態（`writer == null`）で受信エラーが起きると `NullReferenceException` になり、これもスレッドプール上なので EyeData ごと落ちる。既存の不具合だが、今は CanonRKF1.exe が落ちるだけで済んでいる。

### 2.4 設定ファイルの読み込み

`NidekARK1.xml` / `CanonRKF1.xml` は `AppFile.FilePath` で探し、カレントディレクトリ → `Env.SHIN_HOME` … の順に見つかったものを使う。両ファイルとも EyeData.exe と同じ `C:\Shinseikai\EyeData` に置かれている。
現在も `Launcher.Start` で起動した EXE は EyeData のカレントディレクトリを引き継いでいる。また `NidekARK1.Settings` は EyeData（`FormKensa2` / `KensaTabPage2`）がすでに同じプロセス内で読んでいる。そのため、読み込まれるファイルは組み込み前と変わらない。

## 3. 変更内容

### 3.1 EyeCenter

| ファイル | 変更 |
|---|---|
| `FormControl.cs`（UTF-8 BOM） | `F_NidekARK1` / `F_CanonRKF1` フィールドと `FormNidekARK1_Show()` / `FormCanonRKF1_Show()` を追加する。`FormFindKensa_Show()` と同じく、既に開いていれば前面化し、閉じられていれば作り直す |
| `MainForm.cs`（**Shift-JIS**） | `CanonButton_Click` / `NidekButton_Click` の `Launcher.Start(...)` を上記メソッドの呼び出しに変える。変更行は ASCII のみなので、エンコーディングを保ったまま編集する |
| `CLAUDE.md` / `README.md` | 「外部機器 EXE を起動する」記述を更新する |
| `deploy.ps1` | コメントの「外部 EXE（CanonRKF1.exe 等）」の例を更新する |
| `docs/CHANGELOG.md` | 変更を記録する |

```csharp
// FormControl.cs（追加イメージ）
static NidekARK1ListForm F_NidekARK1;
static CanonRKF1Form F_CanonRKF1;

public static void FormNidekARK1_Show()
{
    if (F_NidekARK1 == null || !F_NidekARK1.Created)
    {
        F_NidekARK1 = new NidekARK1ListForm();
    }

    F_NidekARK1.Show();
    F_NidekARK1.Activate();

    if (F_NidekARK1.WindowState == FormWindowState.Minimized)
    {
        F_NidekARK1.WindowState = FormWindowState.Normal;
    }
}
// FormCanonRKF1_Show() も同じ形
```

### 3.2 MedicalLibrary（2.3 の対策。6章の回答次第）

| ファイル | 変更 |
|---|---|
| `Agent/CanonRKF1Form.cs` | `OnFormClosed` をオーバーライドし、ポートを閉じてタイマーを止める（× で閉じたときも「終了」ボタンと同じ後始末をする）。Designer は編集しない |
| `Agent/CanonRKF1Form.cs` | catch 節の `writer.Close()` を null のときは呼ばないようにする |
| `docs/CHANGELOG.md` | 変更を記録する |

### 3.3 変更しないもの

- `NidekARK1ListForm` / `NidekARK1` / `NidekARK1Settings`
- `Utility/Launcher.cs`（`Launcher.Start` は `Grapa/GraphicPavilion.exe` の起動に引き続き使う）

## 4. 手順

1. MedicalLibrary を修正（3.2）→ 検証: `msbuild MedicalLibrary.csproj /p:Configuration=Debug /p:Platform=x86` が成功する
2. EyeCenter を修正（3.1）→ 検証: `msbuild EyeCenter.sln /p:Configuration=Debug /p:Platform=x86` が成功し、`file MainForm.cs` が Shift-JIS のまま、`git diff` に文字化けがない
3. ドキュメントを更新する（CLAUDE.md / README.md / deploy.ps1 / CHANGELOG.md × 2）
4. 本番環境で手動テストする（5章）
5. 配備: MedicalLibrary.dll → EyeData.exe の順に配置する。新しい DLL は旧 EyeData / 旧 EXE とも互換がある（公開 API を変えない）ので、この順で途中の状態でも動く
6. 後片付け（6章の回答次第）: `C:\Shinseikai\EyeData` の `NidekARK1.exe` / `CanonRKF1.exe` を削除し、両リポジトリをアーカイブする

## 5. 手動テスト

| # | 操作 | 期待結果 |
|---|---|---|
| 1 | 「Nidek ARK」ボタンを押す | Nidek ARK1 画面が開く |
| 2 | もう一度押す | 2つ目は開かず、既存の画面が前面に出る |
| 3 | 画面を最小化してからボタンを押す | 元の大きさで前面に出る |
| 4 | 日付を選んで一覧表示し、変換する | 従来通り `ref.dat` が出力される |
| 5 | × で閉じてからボタンを押す | 新しい画面が開く |
| 6 | 「Canon RKF」ボタンを押し、接続して機器から受信する | 従来通り受信内容が表示され、`ref.dat` が出力される |
| 7 | × で閉じ、再度開いて「接続」する | エラーにならず接続できる（ポートが解放されている） |
| 8 | 接続中に × で閉じ、機器から送信する | EyeData が落ちない |
| 9 | 「終了」ボタンで閉じる | 確認ダイアログの後に閉じる（従来通り） |
| 10 | Nidek / Canon 画面を開いたまま EyeData を終了する | 両画面も閉じ、プロセスが残らない |
| 11 | 検査画面の Nidek 取り込み（`ConvertSaveLast`） | 従来通り動く |

## 6. 決定事項（2026-09-24 確認済み）

1. CanonRKF1Form の問題（2.3）は、1・2 とも MedicalLibrary を修正して対処する
2. 後片付け（手順6: 配置先の旧 EXE 削除、両リポジトリの扱い）はユーザーが手動で行う
3. EyeData のバージョンを 1.2.4 → 1.3.0 に上げる
