
echo "INFO: set NugetPush property to 1 (or anthing) to have nugets pushed. /p:NugetPush=1"
c:/windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe src/sms.sln \
 //nologo //t:clean,build //m //p:Configuration=Release $@
echo "INFO: set NugetPush property to 1 (or anthing) to have nugets pushed. /p:NugetPush=1"