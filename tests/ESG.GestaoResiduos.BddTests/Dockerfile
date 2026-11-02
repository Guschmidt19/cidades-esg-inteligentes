# syntax=docker/dockerfile:1
# =============================================================================
#  Dockerfile dos testes BDD (Gherkin/Reqnroll) + contrato (JSON Schema)
#  Cidades ESG Inteligentes · API Gestão de Resíduos
#
#  O contêiner compila o projeto de testes e executa todos os cenários contra
#  a API em execução (variável API_BASE_URL). Os resultados (.trx) são gravados
#  em /results (montado em ./TestResults-BDD pelo docker-compose.bdd.yml).
#
#  Build manual (a partir da RAIZ do repositório):
#    docker build -f tests/ESG.GestaoResiduos.BddTests/Dockerfile -t esg-bdd-tests .
#  Execução manual (API já no ar no host, porta 8080):
#    docker run --rm -e API_BASE_URL=http://host.docker.internal:8080 esg-bdd-tests
#  Forma recomendada: docker-compose.bdd.yml (sobe API + banco + testes).
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS tests
WORKDIR /src

# Restaura em uma camada própria (cache): só refaz se o .csproj mudar
COPY tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj tests/ESG.GestaoResiduos.BddTests/
RUN dotnet restore tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj

# Copia features, schemas e step definitions e compila
COPY tests/ESG.GestaoResiduos.BddTests/ tests/ESG.GestaoResiduos.BddTests/
RUN dotnet build tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj -c Release --no-restore

# URL da API sob teste (dentro do docker-compose, o serviço se chama "api")
ENV API_BASE_URL=http://api:8080
ENV DOTNET_NOLOGO=true

ENTRYPOINT ["dotnet", "test", "tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj", \
            "-c", "Release", "--no-build", \
            "--logger", "trx;LogFileName=resultado-bdd.trx", \
            "--logger", "console;verbosity=normal", \
            "--results-directory", "/results"]
