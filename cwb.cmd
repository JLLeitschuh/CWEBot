@echo off
@setlocal
set ERROR_CODE=0

if /I "%1"=="train" goto train 
if /I "%1" =="classify" goto classify
.\CWEBot.CLI\bin\Debug\CWEBot.exe %*
goto end

:train
SHIFT
if not "%1" == "" goto OkTRAINARG1
echo Error: Usage is cwb train trainFile testFile modelFile 
goto error

:OkTRAINARG1
if not "%2" == "" goto OkTRAINARG2
echo Error: Usage is cwb train trainFile testFile modelFile
goto error

:OkTRAINARG2
if not "%3" == "" goto OkTRAINARGS
echo Error: Usage is cwb train trainFile testFile modelFile
goto error

:OkTRAINARGS
if exist "%1" goto OkTRAINFILE
echo The training file %1 could not be found. >&2
goto error

:OkTRAINFILE
set TRAINFILE=%1
SHIFT
if exist "%1" goto OkTRAINTESTFILE
echo The test file %1 could not be found. >&2
goto error

:OkTRAINTESTFILE
set TESTFILE=%1
SHIFT
set MODELFILE=%1
SHIFT

.\StanfordTrainClassifierModel %TRAINFILE% %TESTFILE% %MODELFILE% %1 %2 %3 %4 %5 %6 %9
goto end

:classify
SHIFT
if not "%1" == "" goto OkCLASSIFYARG1
echo Error: Usage is cwb classify modelFile testFile 
goto error

:OkCLASSIFYARG1
if not "%2" == "" goto OkCLASSIFYARGS
echo Error: Usage is cwb classify modelFile testFile 
goto error

:OkCLASSIFYARGS
if exist "%1" goto OkCLASSIFYMODELFILE
echo The model file %1 could not be found. >&2
goto error

:OkCLASSIFYMODELFILE
set MODELFILE=%1
SHIFT
if exist "%1" goto OkCLASSIFYTESTFILE
echo The test file %1 could not be found. >&2
goto error

:OkCLASSIFYTESTFILE
set TESTFILE=%1
SHIFT
.\StanfordClassifyWithModel %MODELFILE% %TESTFILE% %1 %2 %3 %4 %5 %6 %7 %8 %9

goto end

:error
set ERROR_CODE=1

:end
exit /B %ERROR_CODE%