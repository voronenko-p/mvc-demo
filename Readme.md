
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"

or

call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64

depending on visualstudio you have


msbuild Samples.AspNetMvc4.sln  /t:Rebuild /p:outdir="c:\\outproject" /p:Configuration=Release /p:Platform="Any CPU"

Ensure hosting server has minimal dependencies installed

```
Dism /Online /Get-Features /Format:Table
Dism /Online /Enable-Feature /FeatureName:IIS-WebServer /All
Dism /Online /Enable-Feature /FeatureName:IIS-DefaultDocument /All
Dism /Online /Enable-Feature /FeatureName:IIS-ASPNET45 /All
```

Create portal for future deploy

```ps1
$name = "MVC4Demo"
$physicalPath = "C:\inetpub\wwwroot\" + $name

# Create Application Pool
try
{
 $poolCreated = Get-WebAppPoolState $name –errorvariable retvar
 Write-Host $name "Already Exists"
}
catch
{
 # Assume it doesn't exist. Create it.
 New-WebAppPool -Name $name
 Set-ItemProperty IIS:\AppPools\$name managedRuntimeVersion v4.5
}

# Create Folder for the website
if(!(Test-Path $physicalPath)) {
 md $physicalPath
}
else {
 # Remove-Item "$physicalPath\*" -recurse -Force
 Write-Host Consider clearing recursive $physicalPath
}
 
$site = Get-WebSite | where { $_.Name -eq $name }
if($site -eq $null)
{
 Write-Host "Creating site: $name $physicalPath"
# New-WebSite $name | Out-Null
 New-WebSite -Name $name -Port 80 -PhysicalPath $physicalPath
 New-WebApplication -Name $name   -Site $name  -PhysicalPath $physicalPath -ApplicationPool $name
}
```

Copy collected artifact to the created $physicalPath location


Getting recording from simulator, kind of

```
Curl -o temp.json http://{url:port}/download
```


--------------

PROFILER_GUID  {846F5F1C-F9AE-4B07-969E-05C26BC060D8}  corresponds to


C:\Program Files\Datadog\.NET Tracer\Datadog.Trace.ClrProfiler.Native.dll
