#!/bin/bash

echo "🚀 Starting Neo4j Community Edition with Docker..."

# Start Neo4j container
docker compose up -d neo4j

echo "⏳ Waiting for Neo4j to be ready..."

# Wait for Neo4j to be healthy
timeout=300  # 5 minutes timeout
elapsed=0
interval=5

while [ $elapsed -lt $timeout ]; do
    if docker compose exec -T neo4j cypher-shell -u neo4j -p testpassword "RETURN 1;" &> /dev/null; then
        echo "✅ Neo4j is ready!"
        break
    fi
    
    echo "⏳ Neo4j not ready yet, waiting... ($elapsed/$timeout seconds)"
    sleep $interval
    elapsed=$((elapsed + interval))
done

if [ $elapsed -ge $timeout ]; then
    echo "❌ Timeout waiting for Neo4j to be ready"
    docker compose logs neo4j
    exit 1
fi

echo "🧪 Running integration tests..."

# Build and run tests
dotnet build
dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal

echo "🏁 Tests completed!"
echo ""
echo "📊 Neo4j Browser is available at: http://localhost:7474"
echo "🔑 Login with username: neo4j, password: testpassword"
echo ""
echo "To stop Neo4j: docker compose down"
echo "To view logs: docker compose logs neo4j"