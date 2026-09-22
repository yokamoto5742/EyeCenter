# 検査結果検索 高速化 自動テスト手順

検査結果検索（`FormFindKensa`）の高速化（2026-09-22）について、変更前後で出力が変わらないことと、所要時間を確かめるための手順。

## 対象の変更

| 変更 | ファイル | 確認すること |
|---|---|---|
| 一覧作成の二重ループ解消 | `EyeCenter/FormFindKensa.cs` | 一覧の人数・CSV が変更前と同じ |
| `MakeTableData` の文字列処理の見直し | `EyeCenter/FormFindKensa.cs` | CSV が変更前と同じ（カンマを含む値・`<CR+LF>`・重複コードを含む） |
| 性別の色分け（`SexColor`）の削除 | `EyeCenter/FormFindKensa.cs` | 女性患者のカナ・氏名が赤字にならない（**意図した変更**） |
| Excel 出力の一括書き込み | `MedicalLibrary/Utility/TableData.cs` | Excel の値と型（数値/文字列）が変更前と同じ |
| 検索用接続の FetchSize 拡大 | `EyeCenter/SearchTask.cs`, `MedicalLibrary/Utility/DB.cs` | 検索結果の件数・内容が変更前と同じ |

## 前提

- ローカルのテスト DB（FREEPDB1、`OPEN/system`）が起動していること
- アプリは `C:\Shinseikai\EyeData\EyeData.exe` から起動する（`MedicalLibrary_Settings.xml` を exe と同じフォルダから読むため、`bin\x86\Debug` から直接起動すると「設定ファイルが存在しません」になる）
- MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
- MedicalLibrary の Debug|x86 ビルドは `C:\Shinseikai\EyeData\MedicalLibrary.dll` に直接出力される
- 自動操作中はマウス・キーボードに触れないこと（座標クリックと SendKeys で操作するため）

## 1. 共通テストの実行

```bash
dotnet test EyeCenter.Tests/EyeCenter.Tests.csproj
```

対象は `Barcode128` と `ExcelControl` だけで、今回の変更箇所は含まれない。回帰していないことの確認用。

## 2. テストデータの投入

ダミーデータは **100 件以内**に留める。付録 A の `ins.sql` で 90 件（患者 1001〜1003、視力 KENSA_ID=1 と眼圧 KENSA_ID=3、2026/01/05〜2026/04 の毎週、STAFF=9999）を入れる。

```bash
sqlplus -s -L OPEN/system@//localhost:1521/FREEPDB1 < ins.sql
```

- 患者 1001 の 2026/01/26 の視力には、次の確認用データを入れている: 値にカンマを含む行（`103R,-2.50,memo,x`）、`<CR+LF>` を含む行、項目コードの重複（`101R` が2回）、カンマのない行
- 視力と眼圧の検査日は、患者によって同じ日になる場合とずれる場合がある。そのため「同じ患者・同じ日の検査を1行にまとめる」処理の両方のケースを確認できる
- 実行済みなら再投入は不要（主キー重複でエラーになる）。件数は `select count(*) from EYE_KENSA where STAFF = 9999;` で確認する

## 3. 変更前（before）の出力を取る

変更をいったん退避して、変更前のコードでビルド・配置する。

```bash
cd /c/Users/yokam/source/repos/MedicalLibrary && git stash
cd /c/Users/yokam/source/repos/EyeCenter && git stash

MSB="/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
"$MSB" ../MedicalLibrary/MedicalLibrary.csproj -p:Configuration=Debug -p:Platform=x86 -v:m -nologo
"$MSB" EyeCenter.csproj -p:Configuration=Debug -p:Platform=x86 -v:m -nologo
cp bin/x86/Debug/EyeData.exe bin/x86/Debug/EyeData.pdb /c/Shinseikai/EyeData/

powershell -NoProfile -ExecutionPolicy Bypass -File kensa_export.ps1 -Tag before -OutDir C:\work\kensa_test
```

終わったら**必ず**退避を戻す。

```bash
cd /c/Users/yokam/source/repos/EyeCenter && git stash pop
cd /c/Users/yokam/source/repos/MedicalLibrary && git stash pop
```

> 変更をコミット済みの場合は、`git stash` の代わりに両リポジトリで変更前のコミットを `git checkout` し、終わったら元のブランチに戻す。

## 4. 変更後（after）の出力を取る

```bash
"$MSB" ../MedicalLibrary/MedicalLibrary.csproj -p:Configuration=Debug -p:Platform=x86 -v:m -nologo
"$MSB" EyeCenter.csproj -p:Configuration=Debug -p:Platform=x86 -v:m -nologo
cp bin/x86/Debug/EyeData.exe bin/x86/Debug/EyeData.pdb /c/Shinseikai/EyeData/

powershell -NoProfile -ExecutionPolicy Bypass -File kensa_export.ps1 -Tag after -OutDir C:\work\kensa_test
```

## 5. 結果の比較

```bash
cd /c/work/kensa_test
cmp kensa_before.csv kensa_after.csv && echo CSV一致
diff excel_before.txt excel_after.txt && echo Excel一致
cat time_before.txt time_after.txt
```

| 出力ファイル | 内容 | 合格条件 |
|---|---|---|
| `kensa_<Tag>.csv` | csv出力ボタンの結果（Shift-JIS） | before と after がバイト単位で一致 |
| `excel_<Tag>.txt` | Excel出力のシート全体を「型=値」で書き出したもの | before と after が一致（`String=0.5` と `Double=0.5` の違いも差分として出る） |
| `time_<Tag>.txt` | 人数表示・検索・CSV・Excel の所要時間（ミリ秒） | 参考値。90 件では差はほぼ出ない。速度は本番相当の件数で確かめる |

スクリプトは人数表示が「0 人」のとき中断する（項目のチェックに失敗しているため）。

### 目視で確認すること

- 一覧の女性患者のカナ・氏名が**赤字にならない**こと（色分けの削除は意図した変更）
- 一覧の「検査」列で、同じ日の視力と眼圧が1行にまとまっていること（例: 「視力, 眼圧」）

## 6. 後片付け

```sql
delete from EYE_KENSA where STAFF = 9999;
commit;
```

- `C:\Shinseikai\EyeData\EyeData.exe` を、実運用で使う版に戻す（2026-09-16 版は `EyeData.exe.bak-20260916` に退避済み）
- スクリプトが途中で失敗した場合、画面のない Excel（コマンドラインが `EXCEL.EXE /automation -Embedding`）が残ることがある。タスクマネージャーで終了させる

## 既知の制約

- 自動操作は、DPI 設定（1.27 倍で作成）や画面配置に依存する。うまく動かないときは、手順どおり手動で操作して CSV を保存し、比較だけ `cmp` で行う
- 検査項目のチェックは、一覧の下の空き領域をクリックしてフォーカスを移し、`1`→Space、`3`→Space で行っている（先頭が「1 」「3 」の項目＝視力・眼圧）。項目数が増えて空き領域がなくなった場合は動かない
- 付録 B のスクリプトは、中断前の試行でアプリ起動・検査結果検索画面の表示・CSV 保存までは動いた。項目のチェックと Excel の読み取り部分は修正後に**まだ通しで動かしていない**。初回は画面を見ながら実行すること
- レフケラ出力（KENSA_ID=18）は対象外（テスト DB にデータがない）。Excel の一括書き込みは検査結果の Excel 出力で確かめている

---

## 付録 A: ins.sql

```sql
set serveroutput on
declare
  d date := date '2026-01-05';
  c varchar2(3000);
  nl varchar2(2) := chr(13)||chr(10);
begin
  for k in 0..14 loop
    for p in 1001..1003 loop
      c := '101R,0.' || (mod(p+k,9)+1) || nl || '102R,1.' || mod(k,5) || nl || '101L,0.' || (mod(p*k,9)+1) || nl || '102L,1.0';
      if p = 1001 and k = 3 then
        c := c || nl || '103R,-2.50,memo,x' || nl || '104R,a<CR+LF>b' || nl || '101R,9.9' || nl || 'nocomma';
      end if;
      insert into EYE_KENSA (PATIENT_ID, KENSA_ID, KENSA_DATE, CONT, STAFF, SAVE_DATE, SAVE_TIME)
        values (p, 1, to_number(to_char(d + 7*k, 'YYYYMMDD')), c, 9999, 20260922, 120000);
      insert into EYE_KENSA (PATIENT_ID, KENSA_ID, KENSA_DATE, CONT, STAFF, SAVE_DATE, SAVE_TIME)
        values (p, 3, to_number(to_char(d + 7*k + mod(p,2), 'YYYYMMDD')), '301R,1' || mod(k+p,9) || nl || '301L,1' || mod(k,7) || nl || '304R,15.' || mod(p,10), 9999, 20260922, 120000);
    end loop;
  end loop;
  commit;
end;
/
select count(*) from EYE_KENSA where STAFF = 9999;
exit
```

## 付録 B: kensa_export.ps1

**UTF-8（BOM 付き）で保存すること。**BOM がないと、PowerShell 5.1 で日本語の文字列が化けて構文エラーになる。

処理の流れ: アプリ起動 → 患者画面を閉じる → 「検査検索」 → 視力・眼圧をチェック → 開始日を 2025 年に変更 → 検索 → csv出力 → Excel出力 → Excel のシート内容を書き出す → Excel とアプリを終了

```powershell
param([string]$Tag = "before", [string]$OutDir = "C:\work\kensa_test")
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class W {
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(int f, int x, int y, int d, int e);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr a, int x, int y, int cx, int cy, int f);
  [DllImport("user32.dll")] public static extern IntPtr FindWindowEx(IntPtr p, IntPtr c, string cls, string t);
  [DllImport("oleacc.dll")] public static extern int AccessibleObjectFromWindow(IntPtr h, int id, ref Guid g, [MarshalAs(UnmanagedType.IDispatch)] out object o);
  public static void Click(int x, int y) { SetCursorPos(x, y); mouse_event(2,0,0,0,0); mouse_event(4,0,0,0,0); }
  public static void Front(IntPtr h) { SetWindowPos(h, new IntPtr(-1), 0,0,0,0, 3); SetWindowPos(h, new IntPtr(-2), 0,0,0,0, 3); SetForegroundWindow(h); }
}
"@
[W]::SetProcessDPIAware() | Out-Null
$A = [System.Windows.Automation.AutomationElement]
$T = [System.Windows.Automation.TreeScope]
$All = [System.Windows.Automation.Condition]::TrueCondition
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$times = New-Object System.Collections.Generic.List[string]

function Find($root, $val, $sec = 15) {
  $cond = New-Object System.Windows.Automation.PropertyCondition($A::NameProperty, $val)
  for ($i = 0; $i -lt $sec * 5; $i++) {
    $e = $root.FindFirst($T::Subtree, $cond)
    if ($e) { return $e }
    Start-Sleep -Milliseconds 200
  }
  throw "not found: $val"
}
function ClickEl($e, $dx = $null) {
  $r = $e.Current.BoundingRectangle
  $x = if ($dx -ne $null) { [int]($r.Left + $dx) } else { [int]($r.Left + $r.Width / 2) }
  [W]::Click($x, [int]($r.Top + $r.Height / 2))
}
function Keys($s) { Start-Sleep -Milliseconds 700; [System.Windows.Forms.SendKeys]::SendWait($s) }
function CountText($form) {
  ($form.FindAll($T::Children, $All) | Where-Object { $_.Current.Name -like "人数*" } | Select-Object -First 1).Current.Name
}

$excelBefore = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })

$psi = New-Object System.Diagnostics.ProcessStartInfo "C:\Shinseikai\EyeData\EyeData.exe", "-u 519"
$psi.WorkingDirectory = "C:\Shinseikai\EyeData"
$psi.UseShellExecute = $false
$p = [System.Diagnostics.Process]::Start($psi)
$xlApp = $null
try {
  $c = New-Object System.Windows.Automation.PropertyCondition($A::ProcessIdProperty, $p.Id)

  # メイン画面（タイトル EyeCenter）を待ち、起動時に開く患者画面などは閉じる
  $main = $null
  for ($i = 0; $i -lt 100 -and -not $main; $i++) {
    Start-Sleep -Milliseconds 300
    foreach ($w in $A::RootElement.FindAll($T::Children, $c)) { if ($w.Current.Name -eq "EyeCenter") { $main = $w } }
  }
  if (-not $main) { throw "main window not found" }
  foreach ($w in $A::RootElement.FindAll($T::Children, $c)) {
    if ($w.Current.Name -ne "EyeCenter") { $w.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close() }
  }
  Start-Sleep -Seconds 1
  [W]::Front([IntPtr]$main.Current.NativeWindowHandle)
  ClickEl (Find $main "検査検索")

  # 検査結果検索画面（「レフケラcsv出力」ボタンを持つ窓）を待つ。開かなければ再クリック
  $form = $null
  for ($i = 0; $i -lt 60 -and -not $form; $i++) {
    Start-Sleep -Milliseconds 300
    if ($i % 15 -eq 14) { [W]::Front([IntPtr]$main.Current.NativeWindowHandle); ClickEl (Find $main "検査検索") }
    foreach ($w in $A::RootElement.FindAll($T::Children, $c)) {
      if ($w.FindFirst($T::Children, (New-Object System.Windows.Automation.PropertyCondition($A::NameProperty, "レフケラcsv出力")))) { $form = $w }
    }
  }
  if (-not $form) { throw "kensa form not found" }
  [W]::Front([IntPtr]$form.Current.NativeWindowHandle)
  Start-Sleep -Milliseconds 500

  # 視力（1）・眼圧（3）をチェック。項目は UIA で取れないためキーボードで操作する
  $list = $form.FindAll($T::Children, $All) | Where-Object { $_.Current.ClassName -like "*LISTBOX*" } | Select-Object -First 1
  $lr = $list.Current.BoundingRectangle
  [W]::Click([int]($lr.Left + 40), [int]($lr.Bottom - 10))
  Keys "1"; Keys " "; Keys "3"; Keys " "

  # 開始日の年を 2025 にする（終了日は既定の今日のまま）
  $dtps = @($form.FindAll($T::Children, $All) | Where-Object { $_.Current.ClassName -like "*SysDateTimePick32*" } | Sort-Object { $_.Current.BoundingRectangle.Left })
  ClickEl $dtps[0] 12
  Keys "2025"

  # 検索（人数表示が変わるまでを計測）
  $before = CountText $form
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  ClickEl (Find $form "検索")
  while ((CountText $form) -eq $before -and $sw.Elapsed.TotalSeconds -lt 600) { Start-Sleep -Milliseconds 100 }
  $count = CountText $form
  $times.Add("search_ms=" + $sw.ElapsedMilliseconds + " " + $count)
  "count: $count"
  if ($count -match "　0 人") { throw "検索結果が 0 件です（項目のチェックに失敗した可能性）" }

  # CSV（保存ファイルができるまでを計測）
  $csv = Join-Path $OutDir "kensa_$Tag.csv"
  if (Test-Path $csv) { Remove-Item $csv }
  ClickEl (Find $form "csv出力")
  Keys "{ENTER}"            # 列選択ダイアログ OK
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  Keys ($csv + "{ENTER}")   # 保存ダイアログ
  while (-not (Test-Path $csv) -and $sw.Elapsed.TotalSeconds -lt 600) { Start-Sleep -Milliseconds 100 }
  $times.Add("csv_ms=" + $sw.ElapsedMilliseconds)
  Keys "{ENTER}"            # 完了メッセージ
  Start-Sleep -Seconds 1

  # Excel（Excel の窓が表示される＝書き込み完了までを計測）
  [W]::Front([IntPtr]$form.Current.NativeWindowHandle)
  ClickEl (Find $form "Excel出力")
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  Keys "{ENTER}"            # 列選択ダイアログ OK
  $xl = $null
  while (-not $xl -and $sw.Elapsed.TotalSeconds -lt 600) {
    Start-Sleep -Milliseconds 100
    $xl = Get-Process EXCEL -ErrorAction SilentlyContinue | Where-Object { $excelBefore -notcontains $_.Id -and $_.MainWindowHandle -ne 0 } | Select-Object -First 1
  }
  if (-not $xl) { throw "excel not found" }
  $times.Add("excel_ms=" + $sw.ElapsedMilliseconds)
  Start-Sleep -Seconds 2

  # 開いた Excel からシート全体を「型=値」で書き出す
  $desk = [W]::FindWindowEx($xl.MainWindowHandle, [IntPtr]::Zero, "XLDESK", $null)
  $x7 = [W]::FindWindowEx($desk, [IntPtr]::Zero, "EXCEL7", $null)
  $g = New-Object Guid "00020400-0000-0000-C000-000000000046"
  $obj = $null
  [W]::AccessibleObjectFromWindow($x7, -16, [ref]$g, [ref]$obj) | Out-Null
  $xlApp = $obj.Application
  $v = $xlApp.ActiveWorkbook.ActiveSheet.UsedRange.Value2
  $lines = New-Object System.Collections.Generic.List[string]
  for ($r = 1; $r -le $v.GetLength(0); $r++) {
    $cells = @()
    for ($col = 1; $col -le $v.GetLength(1); $col++) {
      $val = $v[$r, $col]
      $type = if ($val -eq $null) { "null" } else { $val.GetType().Name }
      $cells += "$type=$val"
    }
    $lines.Add(($cells -join " | "))
  }
  [System.IO.File]::WriteAllLines((Join-Path $OutDir "excel_$Tag.txt"), $lines, [System.Text.Encoding]::UTF8)
  "excel rows: " + $v.GetLength(0) + " cols: " + $v.GetLength(1)
} finally {
  [System.IO.File]::WriteAllLines((Join-Path $OutDir "time_$Tag.txt"), $times, [System.Text.Encoding]::UTF8)
  if ($xlApp) {
    try { $xlApp.ActiveWorkbook.Close($false); $xlApp.Quit() } catch { }
  }
  Start-Sleep -Seconds 1
  if (-not $p.HasExited) { $p.Kill() }
}
```
