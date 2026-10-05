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

Real credentials stay in the local root `.env` file and are never committed to Git.

The repository contains only `.env.example` as a safe configuration template.

```
FlowDeskAi/
├── .env             # local secrets, ignored by Git
├── .env.example     # safe template, committed
└── docker-compose.yml
```

The API loads the root `.env` automatically, including when the application is started from Visual Studio, by traversing parent directories.

## Local development

Prerequisites:

- .NET 10 SDK
- Docker Desktop

Create the local environment file:

```bash
copy .env.example .env
```

Edit `.env` and replace `POSTGRES_PASSWORD` with your local password.

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

## Current RAG foundation

PostgreSQL with pgvector is now wired into the Infrastructure layer.

The next stages are:

1. Knowledge/document model
2. Embedding generation
3. Vector persistence
4. Similarity retrieval
5. RAG orchestration
6. LLM provider integration
7. Natural-language query API
8. Spatial query interpretation
9. SpatialMaps integration
10. Reusable AI capabilities for other applications
