@echo off
for /d /r . %%d in (artifacts,packages,TestResults,bin,obj,.vs)      do @if exist "%%d" rd /s /q "%%d"
for    /r . %%f in (*.bak,*.user,*.suo,coverage*.json,coverage*.xml) do @if exist "%%f" del "%%f"