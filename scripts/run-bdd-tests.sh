#!/usr/bin/env bash
# =============================================================================
#  Executa os testes BDD (Gherkin) + contrato (JSON Schema) da API ESG.
#
#  Uso:
#    bash scripts/run-bdd-tests.sh            # modo "docker" (padrão): sobe API + banco + testes
#    bash scripts/run-bdd-tests.sh local      # API já está no ar; roda com dotnet test local
#    API_BASE_URL=http://localhost:8080 bash scripts/run-bdd-tests.sh local
#
#  Requisitos: docker (modo docker) ou .NET SDK 8+ (modo local).
# =============================================================================
set -euo pipefail
cd "$(dirname "$0")/.."

MODO="${1:-docker}"
COMPOSE=(docker compose -f docker-compose.yml -f docker-compose.bdd.yml)

if [ "$MODO" = "docker" ]; then
  [ -f .env ] || { echo ">> Criando .env a partir de .env.example"; cp .env.example .env; }
  mkdir -p TestResults-BDD
  # Começa de um banco limpo para o resultado ser reprodutível
  "${COMPOSE[@]}" down -v >/dev/null 2>&1 || true
  set +e
  "${COMPOSE[@]}" up --build --abort-on-container-exit --exit-code-from bdd-tests
  CODIGO=$?
  set -e
  "${COMPOSE[@]}" down -v
  echo ">> Resultados (.trx) em: TestResults-BDD/   | código de saída: $CODIGO"
  exit "$CODIGO"
else
  export API_BASE_URL="${API_BASE_URL:-http://localhost:8080}"
  echo ">> Testando a API em $API_BASE_URL"
  dotnet test tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj \
    --logger "trx;LogFileName=resultado-bdd.trx" \
    --logger "console;verbosity=normal" \
    --results-directory ./TestResults-BDD
fi
