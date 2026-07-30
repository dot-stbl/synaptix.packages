$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = Resolve-Path (Join-Path $ScriptDir '..')
Push-Location $Root
try {
    & dotnet format synaptix.packages.slnx --verify-no-changes --severity hidden
    if ($LASTEXITCODE -ne 0) { throw "dotnet format exited $LASTEXITCODE" }
}
finally { Pop-Location }