param(
    [string]$configuration = "Release",
    [string]$packageVersion = $null,
    [string]$sourceUrlOverride = $null
)

. ".\build.common.ps1"

$solutionName = "Neo4jClient.Extension"
$sourceUrl = "https://github.com/simonpinn/Neo4jClient.Extension"

function init {
    # Initialization
    $global:rootFolder = Split-Path -parent $script:MyInvocation.MyCommand.Path
    $global:rootFolder = Join-Path $rootFolder .
    $global:packagesFolder = Join-Path $rootFolder packages
    $global:outputFolder = Join-Path $rootFolder _output
    $global:msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
	
	# Override via script parameter
	if($packageVersion){
        $env:PackageVersion = $packageVersion
    }
	
	# default if none specified
	if(!(Test-Path Env:\PackageVersion )){
        $env:PackageVersion = "0.1.2.3"
    }
	
	# Override via environment variable
	if(Test-Path Env:\sourceUrlOverride ){
        $sourceUrl = $env:sourceUrlOverride
    }
	
	# Override via script parameter
	if($sourceUrlOverride){
        $sourceUrl = $sourceUrlOverride
    }

    _WriteOut -ForegroundColor $ColorScheme.Banner "-= $solutionName Build =-"
    _WriteConfig "rootFolder" $rootFolder
	_WriteConfig "packageVersion" $env:PackageVersion
	_WriteConfig "configuration" $configuration
	_WriteConfig "sourceUrl" $sourceUrl
}

function restorePackages{
    _WriteOut -ForegroundColor $ColorScheme.Banner "nuget, gitlink restore"
    
    New-Item -Force -ItemType directory -Path $packagesFolder
    _DownloadNuget $packagesFolder
    nuget restore
    nuget install gitlink -SolutionDir "$rootFolder" -ExcludeVersion
}

function nugetPack{
    _WriteOut -ForegroundColor $ColorScheme.Banner "Nuget pack"
    
    New-Item -Force -ItemType directory -Path $outputFolder

    if(!(Test-Path Env:\nuget )){
        $env:nuget = nuget
    }

	nuget pack $rootFolder\src\Neo4jClient.Extension.Attributes\Neo4jClient.Extension.Attributes.csproj -o $outputFolder -IncludeReferencedProjects -p Configuration=$configuration -Version $env:PackageVersion
    nuget pack $rootFolder\src\Neo4jClient.Extension\Neo4jClient.Extension.csproj 			 -o $outputFolder -IncludeReferencedProjects -p Configuration=$configuration -Version $env:PackageVersion
}

function nugetPublish{

    if(Test-Path Env:\nugetapikey ){
        _WriteOut -ForegroundColor $ColorScheme.Banner "Nuget publish"
        &$env:nuget push .\_output\* $env:nugetapikey
    }
    else{
        _WriteOut -ForegroundColor Yellow "nugetapikey environment variable not detected. Skipping nuget publish"
    }
}

function buildSolution{

    _WriteOut -ForegroundColor $ColorScheme.Banner "Build Solution"
    & $msbuild "$rootFolder\$solutionName.sln" /p:Configuration=$configuration

    &"$rootFolder\packages\gitlink\lib\net45\GitLink.exe" $rootFolder -u $sourceUrl
	
	checkExitCode
}

function executeTests{

    Write-Host "Execute Tests"
    $nunitConsole = "$rootFolder\packages\NUnit.Runners.2.6.3\tools\nunit-console.exe"
    & $nunitConsole "$rootFolder\test\Neo4jClient.Extension.UnitTest\bin\$configuration\Neo4jClient.Extension.Test.dll"
	
    checkExitCode
}

init

restorePackages

buildSolution

executeTests

nugetPack

nugetPublish

Write-Host "Build $env:PackageVersion complete"