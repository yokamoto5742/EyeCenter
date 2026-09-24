# コードレビュー 2026-09-24

## 対象

| リポジトリ | コミット | 内容 |
|---|---|---|
| EyeCenter | `4edfd43` レフケアのプログラムを統合 | `FormControl.cs`（`FormNidekARK1_Show` / `FormCanonRKF1_Show` 追加）、`MainForm.cs`（`Launcher.Start` → 上記呼び出し）、バージョン 1.3.0、ドキュメント |
| MedicalLibrary | `af2ad47` レフケラのプログラムを統合 | `Agent/CanonRKF1Form.cs`（`OnFormClosed` 追加、catch 節の `writer` null チェック） |

観点: 可読性・メンテナンス性、KISS。外部 EXE を同じプロセスに組み込んだことで **「フォームの例外 = EyeData 全体の停止」** になった点も合わせて確認した。

## 総評

EyeCenter 側の変更は既存の `FormFindKensa_Show()` と同じ形に揃っており、読みやすく差分も最小限。計画書（`docs/merge-plan-device-apps.md`）・CHANGELOG・バージョン更新も揃っている。

一方で、同じ形のメソッドが `FormControl` に 6 個並ぶことになったのと、`CanonRKF1Form` の後始末が 2 か所に分かれている点は、シンプルにできる余地がある。また `DataReceived` の中に、閉じた直後の受信で EyeData ごと落ちる経路がまだ 1 つ残っている。

| # | 重要度 | ファイル | 概要 |
|---|---|---|---|
| 1 | 高 | `CanonRKF1Form.cs` | `DataReceived` の `RsvBox.Invoke` とファイル書き込みが try の外にあり、閉じた直後の受信で EyeData ごと落ちうる |
| 2 | 中 | `CanonRKF1Form.cs` | 後始末が `CloseButton_Click` と `OnFormClosed` に重複している |
| 3 | 中 | `CanonRKF1Form.cs` | `writer` / `rsv` をフィールドにする必要がない（catch 節の `writer.Close()` も不要） |
| 4 | 中 | `FormControl.cs` | 「1 つだけ開いて前面化」の処理が 6 回コピーされている |
| 5 | 低 | `CanonRKF1Form.cs` | 接続失敗（ポート使用中・未選択）時に EyeData のスタックトレース付きエラーダイアログが出る |
| 6 | 低 | コミット | 無関係な `docs/dead_code_removal_guide.md`（Agree リポジトリ向け）が同じコミットに入っている |

---

## 1. [高] `DataReceived` の Invoke が try の外にある

`port_DataReceived` はスレッドプールで実行されるため、ここで出た例外は `Application.ThreadException` では拾えず、`AppDomain.UnhandledException`（ログ出力のみ）を経てプロセスが終了する。

```csharp
            catch (Exception ex)
            {
                ...
            }

            // ↓ ここから下は try の外
            AddReceivedDataDelegate add = new AddReceivedDataDelegate(AddReceivedData);
            RsvBox.Invoke(add, rsv);

            if (status == SerialStatus.Close)
            {
                writer = new StreamWriter(new FileStream(file, FileMode.Create), Encoding.Default);
```

- `OnFormClosed` でポートを閉じても、**閉じる直前にスレッドプールへ積まれた受信イベント**は後から実行される。そのときフォームは破棄済みなので `RsvBox.Invoke` が `ObjectDisposedException` / `InvalidOperationException` になる
- `ref.dat` の書き込み失敗（ファイルが他プロセスで開かれている等）も同じく未処理例外になる

外部 EXE だった頃は CanonRKF1.exe が落ちるだけだったが、今は EyeData（手術記録の入力中の画面を含む）が落ちる。手動テスト #8 はタイミング次第で通ってしまうため、コードで防いでおきたい。

**修正案（KISS: ハンドラ全体を 1 つの try に入れ、破棄済みなら何もしない）**

```csharp
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // スレッドプールで実行されるため、ここで例外を漏らすと EyeData ごと終了する
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                string rsv = ReadData();

                RsvBox.Invoke(new Action(() => RsvBox.Text += rsv));

                if (status == SerialStatus.Close)
                {
                    File.WriteAllText(file, RsvBox.Text, Encoding.Default);
                }
            }
            catch (Exception)
            {
                // 画面を閉じた直後の受信・ファイル書き込み失敗は無視する
            }
        }
```

（`ReadData()` は現在の try 内の受信処理を切り出したもの。受信エラー時にメッセージを `RsvBox` に出す現在の挙動を残すなら、`ReadData()` 内の catch で `ex.Message` を返す形にする）

## 2. [中] 後始末が 2 か所にある

```csharp
        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか？", ...) == DialogResult.Yes)
            {
                if (port != null && port.IsOpen) { port.Close(); }
                timer1.Stop();
                this.Dispose();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (port != null && port.IsOpen) { port.Close(); }
            timer1.Stop();
            base.OnFormClosed(e);
        }
```

`this.Dispose()` では `FormClosed` が発生しないため、同じ後始末を 2 回書く必要が生じている。`Close()` にすれば後始末は `OnFormClosed` の 1 か所で済む（モードレスフォームは `Close()` で破棄されるので、`FormControl` 側の `!Created` 判定もそのまま動く）。

```csharp
        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (port != null)
            {
                port.Dispose(); // Close も兼ねる。IsOpen の確認も不要
            }

            timer1.Stop();

            base.OnFormClosed(e);
        }
```

補足: `PortConnect()` で再接続するときも古い `SerialPort` を `Close()` するだけで `Dispose()` していない。`port.Dispose()` に揃えると「閉じる＝破棄」で統一でき、読み手が `IsOpen` の状態を追わなくて済む。

## 3. [中] `writer` / `rsv` はフィールドにしなくてよい

- `writer` は使う箇所すべてで「生成 → 書く → 閉じる」を 1 か所で完結させており、状態を持ち越していない。フィールドにしているせいで、catch 節の `writer.Close()`（今回 null チェックを追加した箇所）のような **意味のない後始末** が必要になっている。`writer` は常に閉じ済みなので、この `Close()` は削除してよい
- `File.WriteAllText(file, RsvBox.Text, Encoding.Default)` の 1 行で置き換えられる（`FileButton_Click` と `port_DataReceived` の 2 か所）。重複をなくすなら `void SaveFile()` にまとめる
- `rsv` も `port_DataReceived` の中でしか使わないのでローカル変数でよい。フィールドだとスレッドプールと UI スレッドで共有されているように見え、読み手が競合を心配することになる

```csharp
        void SaveFile()
        {
            File.WriteAllText(file, RsvBox.Text, Encoding.Default);
        }
```

同様に `AddReceivedDataDelegate` / `ClearDataDelegate` の独自デリゲート定義も `Action` で置き換えられる（.NET 4.8 なので利用可）。

## 4. [中] `FormControl` の「1 つだけ開いて前面化」が 6 回コピーされている

今回追加した 2 メソッドを含め、以下が同じ 12 行の処理になっている。

- `FormFindOpeRecord_Show` / `FormFindKensa_Show` / `FormFindSummary_Show` / `FormPrint_Show` / `FormNidekARK1_Show` / `FormCanonRKF1_Show`

今後も画面を追加するたびにコピーが増えるため、共通化する価値がある（6 か所で使うので「一箇所でしか使わない抽象化」には当たらない）。

```csharp
        /// <summary>
        /// フォームを 1 つだけ表示する。閉じられていれば作り直し、最小化されていれば元に戻す。
        /// </summary>
        static T ShowSingle<T>(T form) where T : Form, new()
        {
            if (form == null || !form.Created)
            {
                form = new T();
            }

            form.Show();
            form.Activate();

            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            return form;
        }

        public static void FormFindKensa_Show()   { F_FindKensa   = ShowSingle(F_FindKensa); }
        public static void FormNidekARK1_Show()   { F_NidekARK1   = ShowSingle(F_NidekARK1); }
        public static void FormCanonRKF1_Show()   { F_CanonRKF1   = ShowSingle(F_CanonRKF1); }
        // FormFindOpeRecord / FormFindSummary / FormPrint も同様
```

`FormOpeRsvList_Show`（`Show(ope_date)` を呼ぶ）と `FormOpeRsv_Show`（最大化する）は形が違うので対象外にする。

今回のコミットの範囲だけで直すなら、この共通化は別コミット（`♻️ refactor`）に分けるのがよい。

## 5. [低] 接続失敗時のエラー表示

`ConnectButton_Click` → `port.Open()` は、ポート未選択・使用中・存在しないポート名で例外になる。UI スレッドなので落ちはしないが、`Program.LogThreadException` によりスタックトレース全文のダイアログが出る（外部 EXE 時代は .NET 既定の未処理例外ダイアログだった）。利用者向けには次の程度で十分。

```csharp
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.PortConnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("接続できませんでした。\n" + ex.Message, "Canon RKF");
            }
        }
```

## 6. [低] コミットの粒度

`docs/dead_code_removal_guide.md`（Agree リポジトリでの作業をまとめた汎用ガイド、246 行）が「レフケアのプログラムを統合」のコミットに含まれている。内容が無関係なので、別コミット（`📝 docs`）に分けたほうが履歴を追いやすい。すでにコミット済みなので、今後の注意点として。

---

## 問題なしと判断した点

- `MainForm.cs`（Shift-JIS）の変更は ASCII のみで、エンコーディング・改行コードは維持されている
- `Launcher.Start` を使わなくなっても `using MedicalLibrary.Utility;` は `LibUtility` / `WinAPI` で引き続き必要
- 設定ファイル（`CanonRKF1.xml` / `NidekARK1.xml`）の探索先は組み込み前と同じ（計画書 2.4 のとおり）
- EyeData 終了時: `Application.Run` 終了で両画面のウィンドウも破棄される。このとき `FormClosed` は発生しないが、プロセス終了で OS がシリアルポートを解放するので問題はない
- `NidekARK1ListForm` はタイマー・スレッドを使わないため、組み込みによる影響はない

## 参考（今回の変更範囲外・`FormControl.cs` 内）

直すかどうかは別途判断でよいが、同じファイルを読むときに気になった点。

- `FormPat_Show(string pt_id, FormPat.Mode mode)` は引数 `mode` を使わず常に `FormPat.Mode.SHOW` を渡している。引数が誤解を招く
- `FormPat_Show` 系 4 メソッドで「`FormPat_List[0]` を取得、無ければ作る」の 10 行が重複している（`GetFormPat()` にまとめられる）
- `FormOpeRsv_Show` の `int i = 0; if (pt_id.Length > 0 && int.TryParse(pt_id, out i))` は `int.TryParse` だけで空文字も弾けるため、`Length > 0` と変数 `i` の宣言は不要
- `FormPat_Count` は `FormPat_List.Count(fp => !fp.IsDisposed)` の 1 行で書ける（`System.Linq` は using 済み）
