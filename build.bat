REM dont remove this line
echo "INFO: set NugetPush property to 1 (or anthing) to have nugets pushed. /p:NugetPush=1
"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /nologo src/sms.sln /t:clean,build /p:Configuration=Release  %*
echo "INFO: set NugetPush property to 1 (or anthing) to have nugets pushed. /p:NugetPush=1