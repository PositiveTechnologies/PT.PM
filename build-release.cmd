dotnet build Sources/PT.PM.sln -c Release

dotnet publish Sources/PT.PM.Cli/PT.PM.Cli.csproj -c Release -o ../../bin/Cli

dotnet publish Sources/PT.PM.PatternEditor/PT.PM.PatternEditor.csproj -c Release -o ../../bin/Gui