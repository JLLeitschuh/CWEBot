@echo off
@setlocal
set ERROR_CODE=0

if not "%1" == "" goto OkARG1
echo Error: Usage is StanfordClassifyWithModel modelFile testFile resultsFile 
goto error

:OkARG1
if not "%2" == "" goto OkARG2
echo Error: Usage is StanfordClassifyWithModel modelFile testFile resultsFile
goto error

:OkARG2
if not "%3" == "" goto OkARGS
echo Error: Usage is StanfordClassifyWithModel modelFile testFile resultsFile
goto error


:OkARGS
if exist "%1" goto OkMODELFILE
echo The model file %1 could not be found. >&2
goto error

:OkMODELFILE
set MODELFILE=%1
SHIFT
if exist "%1" goto OkTESTFILE
echo The test file %1 could not be found. >&2
goto error

:OkTESTFILE
set TESTFILE=%1
SHIFT

:OkRESULTSFILE
set RESULTSFILE=%1
SHIFT

@REM ==== VALIDATE ENVIRONMENT ====
if not "%JAVA_HOME%" == "" goto OkJHOME
echo Error: JAVA_HOME not found in your environment. >&2
echo Please set the JAVA_HOME variable in your environment to match the >&2
echo location of your Java installation. >&2
goto error

:OkJHOME
if exist "%JAVA_HOME%\bin\java.exe" goto OkJCMD
echo Error: The Java 8 binary %JAVA_HOME%\bin\java.exe was not found. >&2
echo Please ensure the JAVA_HOME variable in your environment is set to the >&2
echo location of your Java installation. >&2
goto error

:OkJCMD
if not "%STANFORD_CLASSIFIER_JAR%" == "" goto OkSCJ
echo Error: The environment variable STANFORD_CLASSIFIER_JAR is not set. >&2
echo You must set the STANFORD_CLASSIFIER_JAR variable in your environment to the location >&2
echo of the Stanford classifier .jar file. The Stanford CoreNLP .jar files and supporting files like models should be in the same directory as the classifer .jar. >&2
goto error


:OkSCJ
@echo Running the Stanford CoreNLP Natural Language Processing Toolkit 3.7.0 and Classifier: http://nlp.stanford.edu/software/.
@echo on
java -mx16000m -cp %STANFORD_CLASSIFIER_JAR% edu.stanford.nlp.classify.ColumnDataClassifier -testFile %TESTFILE% -1.splitWordsRegexp "\s+" -1.useAllSplitWordPairs -useQN -displayedColumn 2  -printFeatures -loadClassifier %MODELFILE% %1 %2 %3 %4 %5 %6 %7 %8 %9 > %RESULTSFILE%
@echo off
exit /B %ERROR_CODE%

:error
set ERROR_CODE=1

:end
@endlocal & set ERROR_CODE=%ERROR_CODE%
exit /B %ERROR_CODE%