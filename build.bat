@echo off
setlocal
pushd "%~dp0"
set ILMERGE_PATH=
for %%I in (ilmerge.exe) do @IF EXIST "%%~dp$PATH:I" set ILMERGE_PATH=%%~dp$PATH:I
IF NOT EXIST "%ILMERGE_PATH%"  set ILMERGE_PATH=%ProgramFiles%\Microsoft\ILMerge
IF NOT EXIST "%ILMERGE_PATH%"  set ILMERGE_PATH=%ProgramFiles(x86)%\Microsoft\ILMerge
IF NOT EXIST "%ILMERGE_PATH%" (
	Echo ILMerge.exe not found on PATH.  Build will not succeed. Aborting
	goto exit
)

for %%I in (git.cmd) do @IF EXIST "%%~dp$PATH:I" set GIT_PATH=%%~dp$PATH:I
IF NOT EXIST "%GIT_PATH%"  set ILMERGE_PATH=%ProgramFiles%\Git\cmd
IF NOT EXIST "%GIT_PATH%"  set ILMERGE_PATH=%ProgramFiles(x86)%\Git\cmd
IF NOT EXIST "%GIT_PATH%" (
	Echo Git.CMD not found on PATH.  Build will not succeed.  Aborting
	goto exit
)
set PATH=%PATH%;%ILMERGE_PATH%;%GIT_PATH%
if "%VCINSTALLDIR%" == "" (
	if exist "%VS110COMNTOOLS%\vsvars32.bat" (
		call "%VS110COMNTOOLS%\vsvars32.bat"
	) else (
		if exist "%VS100COMNTOOLS%\vsvars32.bat" (
			call "%VS100COMNTOOLS%\vsvars32.bat"
		) else (
			if exist "%VS90COMNTOOLS%\vsvars32.bat" (
				call "%VS90COMNTOOLS%\vsvars32.bat"
			) else (
				if exist "%VS80COMNTOOLS%\vsvars32.bat" (
					call "%VS80COMNTOOLS%\vsvars32.bat"
				)
			)
		)
	)
)

for %%I in (msbuild.exe) do @IF EXIST "%%~dp$PATH:I" set MSBUILD_PATH=%%~dp$PATH:I
IF NOT EXIST "%MSBUILD_PATH%" (
	Echo MSBuild.exe not found on PATH.  Build will not succeed.  Aborting
	goto exit
)

SET BUILD_OPTIONS=
for %%I in (rar.exe) do @IF EXIST "%%~dp$PATH:I" set RAR_PATH=%%~dp$PATH:I
IF NOT EXIST "%RAR_PATH%" set RAR_PATH=%ProgramFiles%\WinRAR\
IF NOT EXIST "%RAR_PATH%" set RAR_PATH=%ProgramFiles(x86)%\WinRAR\
IF EXIST "%RAR_PATH%" set BUILD_OPTIONS=%BUILD_OPTIONS%,package_rar

for %%I in (7za.exe) do @IF EXIST "%%~dp$PATH:I" set SZA_PATH=%%~dp$PATH:I
IF NOT EXIST "%SZA_PATH%" set SZA_PATH=%ProgramFiles%\WinRAR\
IF NOT EXIST "%SZA_PATH%" set SZA_PATH=%ProgramFiles(x86)%\WinRAR\
IF EXIST "%SZA_PATH%" set BUILD_OPTIONS=%BUILD_OPTIONS%,package_7z

msbuild tesvsnip.csproj /p:Configuration=Release /p:Platform="AnyCPU" /t:version_force,build,package%BUILD_OPTIONS%

:exit
popd
endlocal