#!/bin/bash
if [[ ${CWEBOT_TRACE+x} ]];
then 
	mono --trace=$CWEBOT_TRACE ./CWEBot.CLI/bin/Debug/CWEBot.exe "$@"
else
	mono --debug ./CWEBot.CLI/bin/Debug/CWEBot.exe "$@"
fi