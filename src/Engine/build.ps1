param(
[string]
$version = '1.0',

[string] 
$baseOutputPath = '',

[string]
[ValidateSet('win-x86', 'win-x64', 'osx-x64', 'debian-x64')]
$os = 'win-x86',

[string]
[ValidateSet('false', 'true')]
$sc = 'true',

[string]
[ValidateSet('false', 'true')]
$includeAssets = 'true'
)

$ErrorActionPreference = "Stop"

if ($baseOutputPath -eq "") {
    $baseOutputPath = [Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop);
    if (-Not (Test-Path $baseOutputPath)) {
        Write-Error "baseOutputPath parameter not provided, and could not get Desktop path as default. Please provide OutputPath parameter."
        exit 1
    }
}


$scStr = If ($sc -eq 'true') {"_sc"} Else {""}
$osStr = $os.Replace('-','_')
$versionFileName = $version.Replace('.','_')
#$name = "Royale2D_V$versionFileName" + "_$osStr" + "$scStr"
$name = "Royale2D Engine"
$outputPath = "$baseOutputPath\$name"

if (Test-Path $outputPath) {
    throw "Build output path already exists, remove it first: $path"
}

$constant = "WINDOWS"
If (($os -eq 'debian-x64') -or ($os -eq 'osx-x64'))
{
    $constant = "LINUX"
}

# Build the game
dotnet publish .\Royale2D\Royale2D.csproj -o $outputPath -c Release -r $os --self-contained $sc -p:PublishSingleFile=True -p:DefineConstants="$constant"

If ($includeAssets -eq 'true')
{
    # Copy over assets folder, without levels
    Copy-Item -Path .\..\..\assets -Destination $outputPath -Force -Recurse
}