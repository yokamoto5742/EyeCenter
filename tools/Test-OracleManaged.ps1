# ODP.NET マネージド・ドライバ (Oracle.ManagedDataAccess) の疎通確認スクリプト。
# Oracle クライアントが入っていない端末でそのまま実行できる。
#
# 使い方:
#   .\Test-OracleManaged.ps1 -ConnectionString "User Id=medb;Password=xxx;Data Source=inno_orcl;" -TnsAdmin C:\Shinseikai\EyeData
#   .\Test-OracleManaged.ps1 -ConnectionString "User Id=medb;Password=xxx;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=xxx)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=xxx)));"
#
# 確認する内容:
#   1. マネージド DLL が読み込めるか（＝クライアント不要で動くか）
#   2. 11.2.0.1 のサーバーに接続できるか（19.x は未認定のため、ここが本番の可否を決める）
#   3. NLS_CHARACTERSET
#   4. password_versions に 11G があるか（無いと ORA-28040 になる）
#   5. 日本語のラウンドトリップ（半角カナ・機種依存文字・波ダッシュ）
param(
    [Parameter(Mandatory = $true)][string]$ConnectionString,
    [string]$TnsAdmin,
    [string]$Dll = "C:\Shinseikai\EyeData\Oracle.ManagedDataAccess.dll"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $Dll)) { Write-Error "DLL が見つかりません: $Dll"; exit 1 }
if ($TnsAdmin) { $env:TNS_ADMIN = $TnsAdmin }

$asm = [Reflection.Assembly]::LoadFrom($Dll)
Write-Host "1. ドライバ : $($asm.GetName().FullName)"
Write-Host "   TNS_ADMIN: $(if ($env:TNS_ADMIN) { $env:TNS_ADMIN } else { '(未設定 — exe と同じフォルダの tnsnames.ora を探す)' })"

$conn = New-Object Oracle.ManagedDataAccess.Client.OracleConnection($ConnectionString)

function Invoke-Scalar([string]$sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    try { $cmd.ExecuteScalar() } finally { $cmd.Dispose() }
}

function Invoke-Rows([string]$sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    try {
        while ($r.Read()) {
            $vals = @()
            for ($i = 0; $i -lt $r.FieldCount; $i++) { $vals += [string]$r.GetValue($i) }
            '   ' + ($vals -join '  |  ')
        }
    } finally { $r.Dispose(); $cmd.Dispose() }
}

try {
    $conn.Open()
    Write-Host "2. 接続     : OK  (ServerVersion=$($conn.ServerVersion))"

    Write-Host "3. バージョン/文字コード"
    Invoke-Rows "select banner from v`$version"
    Invoke-Rows "select parameter, value from nls_database_parameters where parameter in ('NLS_CHARACTERSET','NLS_NCHAR_CHARACTERSET')"

    Write-Host "4. password_versions"
    try {
        Invoke-Rows "select username, password_versions from dba_users where username = user"
    } catch {
        Write-Host "   取得できず（権限不足の可能性）: $($_.Exception.Message)"
    }

    Write-Host "5. 日本語ラウンドトリップ"
    $sample = "ｱｲｳ 株式会社 ㈱ 〜 － ①"
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "select :p, dump(:p, 1016) from dual"
    $cmd.BindByName = $true
    $p = $cmd.Parameters.Add("p", [Oracle.ManagedDataAccess.Client.OracleDbType]::Varchar2)
    $p.Value = $sample
    $r = $cmd.ExecuteReader()
    try {
        while ($r.Read()) {
            $got = [string]$r.GetValue(0)
            Write-Host "   送信: $sample"
            Write-Host "   受信: $got"
            Write-Host "   一致: $(if ($got -ceq $sample) { 'OK' } else { '*** 不一致 ***' })"
            Write-Host "   DUMP: $([string]$r.GetValue(1))"
        }
    } finally { $r.Dispose(); $cmd.Dispose() }

    Write-Host ""
    Write-Host "すべて完了。"
} catch {
    Write-Host ""
    Write-Host "失敗: $($_.Exception.GetType().FullName)"
    Write-Host $_.Exception.Message
    exit 1
} finally {
    if ($conn.State -ne 'Closed') { $conn.Close() }
    $conn.Dispose()
}
