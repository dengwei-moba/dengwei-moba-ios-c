@echo off

::SET WORKDIR=%~dp0

xlsx2rof1.0.exe -i ./xlsx -o %WORKDIR%binary -cs %WORKDIR%code/DaoBiao.cs

cd %WORKDIR%code
for /f %%i in ('dir /a-d /b *.cs') do (	
	copy "%%i" "%WORKDIR%..\..\Assets\Script\DaoBiaoManager\"
)

cd ..\
cd %WORKDIR%binary
for /f %%i in ('dir /a-d /b *.bytes') do (	
	copy "%%i" "%WORKDIR%..\..\Assets\DaoBiao\BinaryData\"
)
pause