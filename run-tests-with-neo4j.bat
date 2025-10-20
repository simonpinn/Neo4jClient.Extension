@echo off

echo 🚀 Starting Neo4j Community Edition with Docker...

REM Start Neo4j container
docker compose up -d neo4j

echo ⏳ Waiting for Neo4j to be ready...

REM Wait for Neo4j to be ready - simple approach for Windows
timeout /t 30 /nobreak > nul

echo ✅ Neo4j should be ready! Checking connection...

REM Test connection
docker compose exec -T neo4j cypher-shell -u neo4j -p testpassword "RETURN 1;" >nul 2>&1
if %errorlevel% neq 0 (
    echo ⏳ Neo4j still starting, waiting a bit longer...
    timeout /t 30 /nobreak > nul
)

echo 🧪 Running integration tests...

REM Build and run tests
dotnet build
if %errorlevel% neq 0 (
    echo ❌ Build failed
    pause
    exit /b 1
)

dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal

echo 🏁 Tests completed!
echo.
echo 📊 Neo4j Browser is available at: http://localhost:7474
echo 🔑 Login with username: neo4j, password: testpassword
echo.
echo To stop Neo4j: docker compose down
echo To view logs: docker compose logs neo4j
pause