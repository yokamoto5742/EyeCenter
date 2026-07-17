# 本番機向けの配置フォルダ C:\Shinseikai\EyeData.production を作成するスクリプト
# 使い方: .\deploy-production.ps1 [-Configuration Debug]  （既定は Release）
# 出来上がったフォルダの中身を、そのまま本番機の C:\Shinseikai\EyeData へ上書きコピーする。
# 本番機での手作業（config のリネーム・Oracle.DataAccess.dll の削除）は不要:
#   - EyeData.exe.config は本番用（Oracle 11.2 / GAC 2.112系へのリダイレクト）に置き換え済み
#   - 開発機用 Oracle.DataAccess.dll (4.122) は除外済み（本番機に置くと起動しなくなるため）
param(
    [string]$Configuration = "Release"
)

$sourceDir = Join-Path $PSScriptRoot "bin\x86\$Configuration"
$destDir = "C:\Shinseikai\EyeData.production"

if (-not (Test-Path (Join-Path $sourceDir "EyeData.exe"))) {
    Write-Error "ビルド成果物が見つかりません: $sourceDir\EyeData.exe  先に msbuild EyeCenter.sln /p:Configuration=$Configuration /p:Platform=x86 を実行してください。"
    exit 1
}

$prodConfig = Join-Path $PSScriptRoot "EyeData.exe.config.production"
if (-not (Test-Path $prodConfig)) {
    Write-Error "本番機用の設定ファイルが見つかりません: $prodConfig"
    exit 1
}

if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir | Out-Null
}

$patterns = @("*.exe", "*.dll", "*.ini", "*.xml")
$exclude = @("Oracle.DataAccess.dll")
$files = Get-ChildItem -Path $sourceDir -File | Where-Object {
    $name = $_.Name
    (($patterns | Where-Object { $name -like $_ }).Count -gt 0) -and ($exclude -notcontains $name)
}

foreach ($file in $files) {
    Copy-Item -Path $file.FullName -Destination $destDir -Force
    Write-Host "コピー: $($file.Name)"
}

# 本番機用の設定ファイルを EyeData.exe.config として配置する
Copy-Item -Path $prodConfig -Destination (Join-Path $destDir "EyeData.exe.config") -Force
Write-Host "コピー: EyeData.exe.config.production -> EyeData.exe.config"

Write-Host "完了: $destDir に本番機用ファイルを配置しました。（$Configuration）"
Write-Host "この中身をそのまま本番機の C:\Shinseikai\EyeData へ上書きコピーしてください。"
Write-Host "※本番機に古い Oracle.DataAccess.dll が残っている場合は削除してください。"
