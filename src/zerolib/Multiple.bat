@echo off
if not exist %1 goto usage
cls

:loop
@echo %1
for /l %%N in (1 1 1000) do (
  echo|set /p="%%N "
  %1 kjbible.txt > nul  || echo ERROR && exit /b
)
@echo ""

shift
if not "%~1"=="" goto loop

@echo All Good
goto :eof

:usage
@echo Pass the name of exec
exit /B 1