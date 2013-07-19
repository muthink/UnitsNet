@echo off
setlocal
pushd %~dp0
cd ..

SET BUILD_BASE_DIR=%CD%\BuildForNuget
SET BUILD_DIR=%BUILD_BASE_DIR%\bin
SET PackageName=UnitsNet
SET NuGetExe=%CD%\Tools\NuGet\NuGet.exe

REM ECHO BUILD_DIR=%BUILD_DIR%

RMDIR /S /Q %BUILD_BASE_DIR%
MKDIR %BUILD_DIR%

copy .\%PackageName%.nuspec %BUILD_BASE_DIR%

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild /t:Rebuild /p:OutDir=%BUILD_DIR%\ /p:Configuration=Release .\Src\UnitsNet\UnitsNet.net35.csproj
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild /t:Rebuild /p:OutDir=%BUILD_DIR% /p:Configuration=Release .\Src\UnitsNet\UnitsNet.Pcl.csproj

%NuGetExe% pack %BUILD_BASE_DIR%\%PackageName%.nuspec -Verbose -OutputDirectory "%BUILD_BASE_DIR%" -BasePath "%BUILD_BASE_DIR%"

popd
pause