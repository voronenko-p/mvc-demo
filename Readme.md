
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


provided to web process, kind of 

HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\W3SVC

Environment (REG_MULTI_SZ)

COR_ENABLE_PROFILING=1
COR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
CORECLR_ENABLE_PROFILING=1
CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}


-------------------

Quoting  https://docs.lightstep.com/docs/c-quick-start-1


Each environment uses these installation components:

Native COM library that implements the Profiling API (a .dll file on Windows or .so on Linux) to intercept method calls
Managed library (Datadog.Trace.dll) that interact with your application to measure method execution time and extract data from method arguments
Several environment variables that enable the Profiling API and configure the .NET Auto-Installer


Install the .NET Tracer on the host using the MSI installer for Windows. Choose the platform that matches the OS architecture.
The native library is deployed into Program Files by default and registered as a COM library in the Windows Registry by the MSI installer.
Managed libraries are deployed into the Global Assembly Cache (GAC) by the MSI installer, where any .NET Framework application can access them.

Set these two environment variables before starting your application to enable automatic instrumentation (not necessary if running IIS)



The .NET runtime tries to load a profiler into any .NET process that is started while these environment variables are set. You should limit profiling only to the applications that need to be traced. 
Do not set these environment variables globally as this causes all .NET processes on the host to load the profiler.


```
 COR_ENABLE_PROFILING=1
 COR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
```
Note, that is a guid for native com library ( {846F5F1C-F9AE-4B07-969E-05C26BC060D8} )  if you have changed it - you need to change respectively.

To set environment variables for a Windows Service, use the multi-string key HKLM\System\CurrentControlSet\Services\{service name}\Environment in the Windows Registry.


Configuration for c library

```c
// Sets whether the profiler is enabled. Default is true.
// Setting this to false disabled the profiler entirely.
const WSTRING tracing_enabled = "DD_TRACE_ENABLED"_W;

// Sets whether debug mode is enabled. Default is false.
const WSTRING debug_enabled = "DD_TRACE_DEBUG"_W;

// Sets the paths to integration definition JSON files.
// Supports multiple values separated with semi-colons, for example:
// "C:\Program Files\Datadog .NET Tracer\integrations.json;D:\temp\test_integrations.json"
const WSTRING integrations_path = "DD_INTEGRATIONS"_W;

// Sets the path to the profiler's home directory, for example:
// "C:\Program Files\Datadog .NET Tracer\" or "/opt/datadog/"
const WSTRING profiler_home_path = "DD_DOTNET_TRACER_HOME"_W;

// Sets the filename of executables the profiler can attach to.
// If not defined (default), the profiler will attach to any process.
// Supports multiple values separated with semi-colons, for example:
// "MyApp.exe;dotnet.exe"
const WSTRING include_process_names = "DD_PROFILER_PROCESSES"_W;

// Sets the filename of executables the profiler cannot attach to.
// If not defined (default), the profiler will attach to any process.
// Supports multiple values separated with semi-colons, for example:
// "MyApp.exe;dotnet.exe"
const WSTRING exclude_process_names = "DD_PROFILER_EXCLUDE_PROCESSES"_W;

// Sets the Agent's host. Default is localhost.
const WSTRING agent_host = "DD_AGENT_HOST"_W;

// Sets the Agent's port. Default is 8126.
const WSTRING agent_port = "DD_TRACE_AGENT_PORT"_W;

// Sets the "env" tag for every span.
const WSTRING env = "DD_ENV"_W;

// Sets the default service name for every span.
// If not set, Tracer will try to determine service name automatically
// from application name (e.g. entry assembly or IIS application name).
const WSTRING service_name = "DD_SERVICE_NAME"_W;

// Sets a list of integrations to disable. All other integrations will remain
// enabled. If not set (default), all integrations are enabled. Supports
// multiple values separated with semi-colons, for example:
// "ElasticsearchNet;AspNetWebApi2"
const WSTRING disabled_integrations = "DD_DISABLED_INTEGRATIONS"_W;

// Sets the path for the profiler's log file.
// If not set, default is
// "%ProgramData%"\Datadog .NET Tracer\logs\dotnet-profiler.log" on Windows or
// "/var/log/datadog/dotnet/dotnet-profiler.log" on Linux.
const WSTRING log_path = "DD_TRACE_LOG_PATH"_W;

// Sets whether to disable all optimizations.
// Default is false on Windows.
// Default is true on Linux to work around a bug in the JIT compiler.
// https://github.com/dotnet/coreclr/issues/24676
// https://github.com/dotnet/coreclr/issues/12468
const WSTRING clr_disable_optimizations = "DD_CLR_DISABLE_OPTIMIZATIONS"_W;

// Sets whether to intercept method calls when the caller method is inside a
// domain-neutral assembly. This is dangerous because the integration assembly
// Datadog.Trace.ClrProfiler.Managed.dll must also be loaded domain-neutral,
// otherwise a sharing violation (HRESULT 0x80131401) may occur. This setting should only be
// enabled when there is only one AppDomain or, when hosting applications in IIS,
// the user can guarantee that all Application Pools on the system have at most
// one application.
// Default is false.
// https://github.com/DataDog/dd-trace-dotnet/pull/671
const WSTRING domain_neutral_instrumentation = "DD_TRACE_DOMAIN_NEUTRAL_INSTRUMENTATION"_W;

// Indicates whether the profiler is running in the context
// of Azure App Services
const WSTRING azure_app_services = "DD_AZURE_APP_SERVICES"_W;

// The app_pool_id in the context of azure app services
const WSTRING azure_app_services_app_pool_id = "APP_POOL_ID"_W;

// The DOTNET_CLI_TELEMETRY_PROFILE in the context of azure app services
const WSTRING azure_app_services_cli_telemetry_profile_value =
    "DOTNET_CLI_TELEMETRY_PROFILE"_W;

}  // namespace environment
}  // namespace trace
```