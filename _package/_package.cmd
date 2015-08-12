del /q *.nupkg
..\.nuget\nuget.exe pack ..\src\Neo4jClient.Extension\Neo4jClient.Extension.csproj -IncludeReferencedProjects -Prop Configuration=%1
..\.nuget\nuget.exe pack ..\src\Neo4jClient.Extension.Attributes\Neo4jClient.Extension.Attributes.csproj -IncludeReferencedProjects -Prop Configuration=%1