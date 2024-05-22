del *.nupkg

msbuild SMD.sln /p:Configuration=Release

REM Use dotnet for packaging now
REM NuGet.exe pack SMD/SMD.csproj -Properties Configuration=Release
dotnet pack .\SMD\SMD.csproj -c Release -o .

pause

forfiles /m *.nupkg /c "cmd /c NuGet.exe push @FILE -Source https://www.nuget.org/api/v2/package"
