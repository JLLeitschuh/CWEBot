@echo off
nuget restore .\CWEBot.sln
msbuild .\CWEBot.sln /p:Configuration=Debug