@echo off
@setlocal
set ERROR_CODE=0

if not "%1" == "" goto OkARG1
echo Error: Usage is Stanford-Classify trainFile testFile 
goto error

:OkARG1
if not "%2" == "" goto OkARGS
echo Error: Usage is Stanford-Classify trainFile testFile 
goto error

:OkARGS
if exist "%1" goto OkTRAINFILE
echo The training file %1 could not be found. >&2
goto error

:OkTRAINFILE
if exist "%2" goto OkTESTFILE
echo The test file %1 could not be found. >&2
goto error

:OkTESTFILE
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
@echo on
java -mx1800m -cp %STANFORD_CLASSIFIER_JAR% edu.stanford.nlp.classify.ColumnDataClassifier -trainFile %1 -testFile %2 -1.splitWordsRegexp "\s" -1.useSplitWords -displayedColumn 2  -printFeatures
@echo off
exit /B %ERROR_CODE%

:error
set ERROR_CODE=1

:end
@endlocal & set ERROR_CODE=%ERROR_CODE%
exit /B %ERROR_CODE%