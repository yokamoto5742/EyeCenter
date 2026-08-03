# EyeData のビルド成果物を C:\Shinseikai\EyeData へ配置するスクリプト
# 使い方: .\deploy.ps1 [-Configuration Debug]  （既定は Release）
# 注意: 追加コピーのみ。配置先の Excel テンプレートや外部 EXE（CanonRKF1.exe 等）は削除しない。
param(
    [string]$Configuration = "Release"
)

$sourceDir = Join-Path $PSScriptRoot "bin\x86\$Configuration"
$destDir = "C:\Shinseikai\EyeData"

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

Write-Host "完了: $($files.Count) ファイルを $destDir へ配置しました。（$Configuration）"
