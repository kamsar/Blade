@ECHO off

SET scriptRoot=%~dp0
SET msbuild=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

"%msbuild%" "%scriptroot%\..\Blade.sln" /p:Configuration=Release /m

"%scriptRoot%\..\.nuget\NuGet.exe" pack "%scriptRoot%\Blade.nuget\Blade.nuspec"

"%scriptRoot%\..\.nuget\NuGet.exe" pack "%scriptRoot%\Blade.Core.nuget\Blade.Core.nuspec" -Symbols

"%scriptRoot%\..\.nuget\NuGet.exe" pack "%scriptRoot%\Blade.Samples.nuget\Blade.Samples.nuspec"

PAUSE