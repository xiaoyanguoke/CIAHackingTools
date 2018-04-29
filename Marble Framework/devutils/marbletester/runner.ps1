#Change this if you want to increase the number of passes
$maxLoop = 10000

Function Dump-Data($folder, $outText, $marbleText)
{
    New-Item ([string]$folder) -ItemType directory -Force

    $outFile = ([string]$folder) + "\out.txt"
    $marbledoth = ([string]$folder) + "\Marble.h"
    
    Copy-Item $outText $outFile
    Copy-Item $marbleText $marbledoth
}


#The Environment Variable ProgramFiles(x86) is unique to 64 bit, if we don't have it we assume 32 bit
$programFiles = ${env:ProgramFiles(x86)}
if (!$programFiles)
{
    $programFiles = $env:ProgramFiles
}

#Hard coded path, sorry if you want to prove it is there be my guest.
$pathToDevenv = $programFiles + "\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe"

if (Test-Path $pathToDevenv)
{
    Write-Host -NoNewline "Found Devenv at "
    Write-Host $pathToDevenv
}
else
{
    Write-Output "Failed to find Devenv."
    [Environment]::Exit(1)
}

$failureFolder = 'failures'
if (Test-Path $failureFolder)
{
    Remove-Item $failureFolder -Recurse -Force
}
New-Item $failureFolder -ItemType directory -Force

$pathToMarbleSln = 'marbletester\MarbleTester.sln'
$pathToMarbleH = 'marbletester\Shared\Marble.h'
$pathToMarbleTester = "marbletester\BIN\Release_Dynamic\Win32\MarbleTester.exe"
$outPath = 'out.txt'
$success = 1

$command =  $pathToMarbleSln + ' /build "Release_Dynamic|Win32" /out ' + $outPath

for($i = 0; $i -lt $maxLoop; $i++)
{
    if (Test-Path $outPath)
    {
        Remove-Item $outPath -Recurse -Force
    }

    Write-Host -NoNewline "*** Compiling Pass Number: "
    Write-Host $i
    #We modify the last write time to trick VS to build.
    $marbleFile = Get-ChildItem -Path $pathToMarbleH
    $marbleFile.LastWriteTime = Get-Date

    $p = Start-Process -FilePath $pathToDevenv -ArgumentList $command -Wait -PassThru
    
    if ($p.ExitCode -eq 0)
    {
        #We succeeded with the build now run the MarbleTester
        Write-Host "Build Succeeded, running MarbleTester"
        $p = Start-Process -FilePath $pathToMarbleTester -Wait -PassThru
        if ($p.ExitCode -eq 0)
        {
            Write-Host "MarbleTester returned success.  Test Passed"
        }
        else
        {
            Write-Host "Failed marble test!"
            $success = 0
            $failureSubfolder = $failureFolder + "\" + $i
            Dump-Data $failureSubfolder $outPath $pathToMarbleH
        }
    
    }
    else
    {
        Write-Host "Failed to compile!"
        $success = 0
        $failureSubfolder = $failureFolder + "\" + $i
        Dump-Data $failureSubfolder $outPath $pathToMarbleH
    }
}

if ($success -eq 1)
{
    Write-Host "All tests passed!"
    #[Environment]::Exit(0)
}
else
{
    Write-Host "One or more tests failed.  See log for more details."
    #[Environment]::Exit(1)
}