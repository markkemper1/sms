REM dont remove this line
"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /nologo src/CouchN.sln /t:clean,build /p:Configuration=Release  %*