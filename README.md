# FlowDesk AI

FlowDesk AI is a reusable AI capability platform for business applications.

The goal is to provide application-agnostic AI services that other products can consume through HTTP APIs.

## Initial architecture

```
FlowDesk AI
├── FlowDesk.AI.Api            -> HTTP/API boundary
├── FlowDesk.AI.Application    -> Use cases and AI abstractions
├── FlowDesk.AI.Domain         -> Core domain contracts/models
└── FlowDesk.AI.Infrastructure -> External services, persistence, embeddings, LLM integrations
```

SpatialMaps will be the first client, but FlowDesk AI is intentionally not coupled to SpatialMaps.

## Secrets and credentials

No real credentials belong in Git.

Local development uses two layers:

- Docker Compose reads PostgreSQL credentials from a local `.env` file.
- ASP.NET Core secrets such as database connection strings and AI provider API keys use .NET User Secrets.

Create the local Docker environment:

```bash
copy .env.example .env
```

Edit `.env` and replace the placeholder password.

Set local ASP.NET secrets:

```bash
dotnet user-secrets set "ConnectionStrings:FlowDeskDb" "Host=localhost;Port=5432;Database=flowdesk;Username=flowdesk;Password=YOUR_PASSWORD" --project src/FlowDesk.AI.Api
dotnet user-secrets set "AI:ProviderApiKey" "YOUR_API_KEY" --project src/FlowDesk.AI.Api
```

Production credentials will be provided by the deployment environment or a dedicated secret store rather than committed to the repository.

## First target capability

Retrieval-Augmented Generation (RAG):

```
Application
    |
    | HTTP
    v
FlowDesk.AI.Api
    |
    v
Application layer
    |
    +----> vector retrieval
    |
    +----> LLM provider
    |
    v
structured AI response
```

## Development approach

Build the platform incrementally:

1. Backend foundation
2. PostgreSQL + pgvector
3. Document/data ingestion
4. Embedding generation
5. Vector retrieval
6. RAG orchestration
7. Natural-language query endpoint
8. Spatial query interpretation
9. SpatialMaps integration
10. Reusable AI capabilities for other applications

## Local development

Prerequisites:

- .NET 10 SDK
- Docker Desktop

Start PostgreSQL:

```bash
docker compose up -d
```

Build:

```bash
dotnet restore
dotnet build
```

Run the API:

```bash
dotnet run --project src/FlowDesk.AI.Api
```

The API currently contains only the platform foundation. AI/RAG functionality will be added next.
