
param(
    [ValidateScript({Test-Path $_})]
    $nuspecFile = '.\src\webjobs.nuspec',

    [ValidateScript({Test-Path $_})]
    $nugetPath = '.\tools\NuGet.exe',
    
    [ValidateScript({Test-Path $_})]
    $nugetBasePath = '.\src',

    $outputFolder = '.\OutputRoot\NuGet',
    
    $nugetDevRepoPath = 'C:\temp\nuget\localrepo'
)
$argMsg = @"
build-nugetpkg.ps1 called with the following args:
    outputFolder: [{0}]
    nugetPath: [{1}]
    nuspecFile: [{2}]
"@

$argMsg -f $outputFolder, $nugetPath, $nuspecFile | Write-Verbose

if(!(Test-Path $outputFolder)){
    mkdir $outputFolder
}

# delete any .nupkg files that are in the output folder
Get-ChildItem $outputFolder -Filter *.nupkg | Remove-Item

# nuget pack .\webjobs.nuspec -BasePath .\src\ -OutputDirectory ..\OutputRoot\NuGet\ -NonInteractive
$nugetArgs = @()
$nugetArgs += 'pack'
$nugetArgs += $nuspecFile
$nugetArgs += '-OutputDIrectory'
$nugetArgs += $outputFolder
$nugetArgs += '-NonInteractive'

'Calling nuget.exe with the following args: [{0}]' -f ($nugetArgs -join ' ') | Write-Host

& $nugetPath $nugetArgs

if(Test-Path $nugetDevRepoPath){
    # copy nuget packages to the dev repo
    Get-ChildItem $outputFolder -Filter *.nupkg | Copy-Item -Destination $nugetDevRepoPath
}