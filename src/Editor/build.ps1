param (
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('se', 'me')]
    [string]$EditorType = "se",

    [Parameter(Mandatory=$false)]
    [ValidateSet('x86', 'x64', 'AnyCPU')]
    [string]$Platform = "x64",  # Platform: x86, x64, or AnyCPU

    [Parameter(Mandatory=$false)]
    [string]$Version = "0.1.0.0"
)

# Set paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

if ($OutputPath -eq "") {
    $OutputPath = [Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop);
    if (-Not (Test-Path $OutputPath)) {
        Write-Error "OutputPath parameter not provided, and could not get Desktop path as default. Please provide OutputPath parameter."
        exit 1
    }
}

if ($EditorType -eq "se") {
    $OutputPath = "$OutputPath\Royale2D Sprite Editor"
    $ProjectFile = "SpriteEditor\SpriteEditor.csproj"
    $IncludeInBuildPaths = @(
        "SpriteEditor\sample_sprite_workspace",
        "SpriteEditor\readme_image.png",
        "SpriteEditor\readme.md"
    )
}
else {
    $OutputPath = "$OutputPath\Royale2D Map Editor"
    $ProjectFile = "MapEditor\MapEditor.csproj"
    $IncludeInBuildPaths = @(
        "MapEditor\sample_map_workspace",
        "MapEditor\readme_image.png",
        "MapEditor\readme.md"
    )
}

if (Test-Path $OutputPath) {
    Write-Error "Output folder already exists in the output path. Please move/rename/delete it from this path."
    exit 1
}

# Create output directory
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null

# Set configuration
$Configuration = "Release"

# Ensure dotnet CLI exists
if (-Not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error "The 'dotnet' command-line tool is not installed or not in PATH."
    exit 1
}

# Build command
Write-Output "Building project '$ProjectFile' for platform '$Platform' in Release mode..."
& dotnet build $ProjectFile `
    -c $Configuration `
    -p:Platform=$Platform `
    -p:FileVersion=$Version `
    -p:InformationalVersion=$Version `
    --output $OutputPath

# Check for build success
if ($LASTEXITCODE -eq 0) {
    Write-Output "Build succeeded! Output is located at '$OutputPath'."

    # Copy files/folders from each IncludeInBuildPaths path
    foreach ($path in $IncludeInBuildPaths) {
        if (Test-Path $path) {
            Write-Output "Copying '$path' to '$OutputPath'..."

            if (Test-Path $path -PathType Container) {
                # It's a folder → copy the whole folder into $OutputPath
                $dest = Join-Path $OutputPath (Split-Path $path -Leaf)
                Copy-Item -Path $path -Destination $dest -Recurse -Force
            } else {
                # It's a file → copy into $OutputPath
                Copy-Item -Path $path -Destination $OutputPath -Force
            }

            Write-Output "'$path' copied successfully."
        } else {
            Write-Output "Path '$path' does not exist. Skipping."
        }
    }
} else {
    Write-Error "Build failed! Check the project configuration and output above."
    exit $LASTEXITCODE
}
