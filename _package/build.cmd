msbuild ..\Neo4jClient.Extension.sln /p:configuration=debug  /t:clean,build
msbuild ..\Neo4jClient.Extension.sln /p:configuration=release /t:clean,build