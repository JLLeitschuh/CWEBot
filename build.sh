#!/bin/bash
nuget restore ./CWEBot.sln
xbuild ./CWEBot.sln /p:Configuration=Debug
