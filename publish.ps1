param(
    [string]$OutputPath = 'D:\Game\Pokemon\3DSRNGTool'
)

$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot '3DSRNGTool\3DSRNGTool.csproj'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '未找到 dotnet SDK，请先安装 .NET 10 SDK。'
}

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "找不到项目文件：$projectPath"
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

$publishArguments = @(
    $projectPath
    '--configuration'
    'Release'
    '--runtime'
    'win-x64'
    '--self-contained'
    'false'
    '--output'
    $OutputPath
    '-p:PublishSingleFile=true'
)

Write-Host "开始发布到：$OutputPath"
& dotnet publish @publishArguments

if ($LASTEXITCODE -ne 0) {
    throw "发布失败，dotnet publish 返回代码：$LASTEXITCODE"
}

$executablePath = Join-Path $OutputPath '3DSRNGTool.exe'
if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
    throw "发布完成但未找到单文件程序：$executablePath"
}

$executable = Get-Item -LiteralPath $executablePath
Write-Host ("发布成功：{0} ({1:N2} MB)" -f $executable.FullName, ($executable.Length / 1MB))
