param($scriptRoot)

$msBuild = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
$nuGet = "$scriptRoot..\Dependencies\NuGet.exe"
$solution = "$scriptRoot\..\Blade.sln"

& $nuGet restore $solution
& $msBuild $solution /p:Configuration=Release /t:Rebuild /m

$bladeAssembly = Get-Item "$scriptRoot\..\Source\Blade\bin\Release\Blade.dll" | Select-Object -ExpandProperty VersionInfo
$targetAssemblyVersion = $bladeAssembly.ProductVersion

& $nuGet pack "$scriptRoot\Blade.nuget\Blade.nuspec" -version $targetAssemblyVersion

& $nuGet pack "$scriptRoot\Blade.Samples.nuget\Blade.Samples.nuspec" -version $targetAssemblyVersion

& $nuGet pack "$scriptRoot\..\Source\Blade\Blade.csproj" -Symbols -Prop Configuration=Release