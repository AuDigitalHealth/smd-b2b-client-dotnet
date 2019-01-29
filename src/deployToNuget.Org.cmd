del *.nupkg

nuget restore

msbuild SMD.sln /p:Configuration=Release

NuGet.exe pack SMD/SMD.csproj -Properties Configuration=Release

pause

forfiles /m *.nupkg /c "cmd /c NuGet.exe push @FILE -Source https://www.nuget.org/api/v2/package"
