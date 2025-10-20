# Docker Testing Setup for Neo4j Community Edition

This project includes a Docker Compose setup to run Neo4j Community Edition for testing the integration tests.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose (included with Docker Desktop)

## Quick Start

### Option 1: Automated Script (Recommended)

**Linux/macOS:**
```bash
./run-tests-with-neo4j.sh
```

**Windows:**
```cmd
run-tests-with-neo4j.bat
```

### Option 2: Manual Steps

1. **Start Neo4j:**
   ```bash
   docker compose up -d neo4j
   ```

2. **Wait for Neo4j to be ready** (about 30-60 seconds)

3. **Run the tests:**
   ```bash
   dotnet test --filter "FullyQualifiedName~Integration"
   ```

4. **Stop Neo4j when done:**
   ```bash
   docker compose down
   ```

## Neo4j Configuration

The Docker setup provides:

- **Neo4j Version**: 5.24 Community Edition (latest LTS)
- **HTTP Port**: 7474 (Web Browser)
- **Bolt Port**: 7687 (Driver Connection)
- **Username**: `neo4j`
- **Password**: `testpassword`
- **Web Browser**: http://localhost:7474

## Connection Details

The integration tests are configured to connect with:
- **URI**: `bolt://localhost:7687`
- **Username**: `neo4j` 
- **Password**: `testpassword`

You can override these in the App.config file:

```xml
<appSettings>
  <add key="Neo4jConnectionString" value="bolt://localhost:7687"/>
  <add key="Neo4jUsername" value="neo4j"/>
  <add key="Neo4jPassword" value="testpassword"/>
</appSettings>
```

## Troubleshooting

### Neo4j won't start
```bash
# Check Docker logs
docker compose logs neo4j

# Restart the container
docker compose restart neo4j
```

### Connection refused errors
- Make sure Neo4j container is running: `docker compose ps`
- Wait longer for Neo4j to fully start (can take 1-2 minutes)
- Check if ports 7474 and 7687 are available

### Tests still failing
- Verify Neo4j is responding:
  ```bash
  docker compose exec neo4j cypher-shell -u neo4j -p testpassword "RETURN 1;"
  ```
- Check the connection string in App.config matches your setup

### Clean Reset
If you need to start fresh:
```bash
docker compose down -v  # Removes volumes too
docker compose up -d neo4j
```

## Development Workflow

1. Start Neo4j: `docker compose up -d neo4j`
2. Develop and test your code
3. Run integration tests: `dotnet test --filter Integration`
4. Use Neo4j Browser at http://localhost:7474 to inspect data
5. Stop when done: `docker compose down`

## Performance Notes

The Neo4j container is configured with:
- **Initial heap**: 512MB
- **Max heap**: 2GB  
- **Page cache**: 1GB

You can adjust these in docker compose.yml if needed for your system.