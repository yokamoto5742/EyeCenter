# 本番機向けの配置フォルダ C:\Shinseikai\EyeData.production を作成するスクリプト
# 使い方: .\deploy-production.ps1 [-Configuration Debug]  （既定は Release）
# 出来上がったフォルダの中身を、そのまま本番機の C:\Shinseikai\EyeData へ上書きコピーする。
# ODP.NET マネージド・ドライバへ移行したため、開発機用と本番機用で config を分ける必要は無くなった。
# 本番機に旧 Oracle.DataAccess.dll が残っている場合は削除すること（もう使用しない）。
param(
    [string]$Configuration = "Release"
)

$sourceDir = Join-Path $PSScriptRoot "bin\x86\$Configuration"
$destDir = "C:\Shinseikai\EyeData.production"

if (-not (Test-Path (Join-Path $sourceDir "EyeData.exe"))) {
    Write-Error "ビルド成果物が見つかりません: $sourceDir\EyeData.exe  先に msbuild EyeCenter.sln /p:Configuration=$Configuration /p:Platform=x86 を実行してください。"
    exit 1
}

if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir | Out-Null
}

$patterns = @("*.exe", "*.exe.config", "*.dll", "*.ini", "*.xml")
$files = Get-ChildItem -Path $sourceDir -File | Where-Object {
    $name = $_.Name
    ($patterns | Where-Object { $name -like $_ }).Count -gt 0
}

foreach ($file in $files) {
    Copy-Item -Path $file.FullName -Destination $destDir -Force
    Write-Host "コピー: $($file.Name)"
}

Write-Host "完了: $destDir に本番機用ファイルを配置しました。（$Configuration）"
Write-Host "この中身をそのまま本番機の C:\Shinseikai\EyeData へ上書きコピーしてください。"
Write-Host "※本番機に古い Oracle.DataAccess.dll が残っている場合は削除してください。"
