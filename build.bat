@echo off
setlocal
pushd "%~dp0"
set PATH=%PATH%;%ProgramFiles(x86)%\Microsoft\ILMerge
msbuild tesvsnip.csproj /p:Configuration=Release /p:Platform="AnyCPU" /t:version_force,build,package
popd
endlocal