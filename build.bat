@echo off
setlocal
pushd "%~dp0"

REM  ILMerge does not work correctly with 4.0 runtime
REM set ILMERGE_PATH=
REM for %%I in (ilmerge.exe) do @IF EXIST "%%~dp$PATH:I" set ILMERGE_PATH=%%~dp$PATH:I
REM IF NOT EXIST "%ILMERGE_PATH%"  set ILMERGE_PATH=%ProgramFiles%\Microsoft\ILMerge
REM IF NOT EXIST "%ILMERGE_PATH%"  set ILMERGE_PATH=%ProgramFiles(x86)%\Microsoft\ILMerge
REM IF NOT EXIST "%ILMERGE_PATH%" (
REM 	Echo ILMerge.exe not found on PATH.  Build will not succeed. Aborting
REM 	goto exit
REM )
REM IF NOT EXIST "%ILMERGE_PATH%\ILMerge.exe.config" (
REM 	Echo ILMerge.exe.config not in ILMerge Folder.  Build will not succeed with .NET 4.0
REM 	Echo   Create file with :
REM 	echo  <configuration><startup useLegacyV2RuntimeActivationPolicy="true"><requiredRuntime safemode="true" imageVersion="v4.0.30319" version="v4.0.30319"/></startup></configuration>
REM 	goto exit
REM )

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
REM IF EXIST "%RAR_PATH%" set BUILD_OPTIONS=%BUILD_OPTIONS%,package_rar

for %%I in (7za.exe) do @IF EXIST "%%~dp$PATH:I" set SZA_PATH=%%~dp$PATH:I
IF NOT EXIST "%SZA_PATH%" set SZA_PATH=%ProgramFiles%\WinRAR\
IF NOT EXIST "%SZA_PATH%" set SZA_PATH=%ProgramFiles(x86)%\WinRAR\
IF EXIST "%SZA_PATH%" set BUILD_OPTIONS=%BUILD_OPTIONS%,package_7z

REM 2 pass build since copy does not pickup source files generated or copied during build process
msbuild build.proj /p:Configuration=Release /p:Platform="AnyCPU" /t:version_force,build,afterbuild
msbuild build.proj /p:Configuration=Release /p:Platform="AnyCPU" /t:version_force,package%BUILD_OPTIONS%

:exit
popd
endlocal