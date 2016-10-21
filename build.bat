@echo off
echo -- Restoring references --
"D:\Tools\nuget.exe" restore
if %errorlevel% neq 0 ( exit %errorlevel% )
echo -- Building --
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" DataCenter_IRCConnector.sln /t:rebuild /p:Configuration=Release;Platform="Any CPU";DebugSymbols=false;DebugType=None /m
