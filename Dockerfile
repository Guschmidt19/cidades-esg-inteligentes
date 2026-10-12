# syntax=docker/dockerfile:1
# =============================================================================
#  Dockerfile multi-stage — Cidades ESG Inteligentes · API Gestão de Resíduos
#  (.NET 8 / ASP.NET Core)
#
#  Estágios:
#    build   -> restaura dependências e compila a solução (API + testes)
#    test    -> (opcional) roda os testes:  docker build --target test .
#    publish -> gera os binários otimizados da API
#    final   -> imagem enxuta de runtime, usuário não-root e healthcheck
# =============================================================================

# ---------- Stage 1: build ---------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Copia só os arquivos de projeto e restaura: esta camada fica em cache e
#    só é refeita quando alguma dependência (.csproj) muda.
COPY ESG.GestaoResiduos.sln ./
COPY src/ESG.GestaoResiduos.API/ESG.GestaoResiduos.API.csproj src/ESG.GestaoResiduos.API/
COPY tests/ESG.GestaoResiduos.Tests/ESG.GestaoResiduos.Tests.csproj tests/ESG.GestaoResiduos.Tests/
RUN dotnet restore ESG.GestaoResiduos.sln

# 2) Copia o código-fonte e compila em Release.
COPY src/ src/
COPY tests/ tests/
RUN dotnet build ESG.GestaoResiduos.sln -c Release --no-restore

# ---------- Stage 2: test (opcional) ----------------------------------------
FROM build AS test
RUN dotnet test ESG.GestaoResiduos.sln -c Release --no-build

# ---------- Stage 3: publish -------------------------------------------------
FROM build AS publish
RUN dotnet publish src/ESG.GestaoResiduos.API/ESG.GestaoResiduos.API.csproj \
    -c Release --no-build -o /app/publish /p:UseAppHost=false

# ---------- Stage 4: runtime (imagem final) ----------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Versão injetada pelo pipeline (ex.: sha-1a2b3c4). Aparece em /api/info.
ARG APP_VERSION=local

LABEL org.opencontainers.image.title="cidades-esg-inteligentes-api" \
      org.opencontainers.image.description="API ESG de Gestão de Resíduos e Reciclagem (.NET 8)" \
      org.opencontainers.image.version="${APP_VERSION}" \
      org.opencontainers.image.authors="Gustavo Ferreira Schmidt - RM 562688"

# curl é usado apenas pelo HEALTHCHECK do container.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    APP_VERSION=${APP_VERSION}

EXPOSE 8080

# Segurança: a aplicação roda com o usuário sem privilégios "app"
# (já existe nas imagens oficiais do .NET 8).
USER app

# O Docker marca o container como "healthy" só quando a API responde.
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -fsS "http://localhost:${PORT:-8080}/health/live" || exit 1

ENTRYPOINT ["dotnet", "ESG.GestaoResiduos.API.dll"]
