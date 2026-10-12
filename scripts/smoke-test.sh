#!/usr/bin/env bash
# =============================================================================
#  Smoke test da API — usado pelo pipeline após cada deploy
#
#  Uso:
#    ./scripts/smoke-test.sh <URL_BASE> [--escrita] [--versao <tag>]
#
#    --escrita   também testa o fluxo crítico (login + POST /api/descartes).
#                Usado no container de CI e em STAGING; em PRODUÇÃO o teste
#                é somente leitura, para não gravar dados falsos.
#    --versao    confere se /api/info está servindo exatamente essa versão.
#
#  Exemplo local:  ./scripts/smoke-test.sh http://localhost:8080 --escrita
#  Requisitos: bash, curl e jq.
# =============================================================================
set -euo pipefail

BASE_URL="${1:?Informe a URL base. Ex.: ./scripts/smoke-test.sh http://localhost:8080}"
BASE_URL="${BASE_URL%/}"
shift

ESCRITA=false
VERSAO_ESPERADA=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --escrita) ESCRITA=true; shift ;;
    --versao)  VERSAO_ESPERADA="${2:?--versao precisa de um valor}"; shift 2 ;;
    *) echo "Argumento desconhecido: $1"; exit 2 ;;
  esac
done

FALHAS=0
ok()   { echo "  ✅ $*"; }
erro() { echo "  ❌ $*"; FALHAS=$((FALHAS + 1)); }

# --- 1. Aguarda a API ficar saudável (container subindo / Render "acordando")
echo "⏳ Aguardando ${BASE_URL}/health ..."
for tentativa in $(seq 1 40); do
  if curl -fsS --max-time 10 "${BASE_URL}/health" >/dev/null 2>&1; then
    echo "  API respondeu na tentativa ${tentativa}."
    break
  fi
  if [[ "$tentativa" -eq 40 ]]; then
    echo "  ❌ A API não ficou saudável a tempo."
    curl -sS --max-time 10 "${BASE_URL}/health" || true
    exit 1
  fi
  sleep 5
done

# Faz a requisição e confere o status HTTP esperado
# uso: checar <descrição> <status esperado> <curl args...>
checar() {
  local descricao="$1" esperado="$2"; shift 2
  local status
  status=$(curl -sS -o /tmp/smoke_body.json -w '%{http_code}' --max-time 30 "$@" || echo "000")
  if [[ "$status" == "$esperado" ]]; then
    ok "${descricao} -> HTTP ${status}"
  else
    erro "${descricao} -> HTTP ${status} (esperado ${esperado})"
    head -c 500 /tmp/smoke_body.json 2>/dev/null || true; echo
  fi
}

echo "🔎 Smoke test em ${BASE_URL}"

# --- 2. Saúde e versão
checar "GET /health (API + banco)" 200 "${BASE_URL}/health"
echo "     $(jq -c . /tmp/smoke_body.json 2>/dev/null || cat /tmp/smoke_body.json)"

checar "GET /api/info" 200 "${BASE_URL}/api/info"
INFO=$(cat /tmp/smoke_body.json)
echo "     ambiente=$(jq -r '.ambiente' <<<"$INFO")  versao=$(jq -r '.versao' <<<"$INFO")  banco=$(jq -r '.banco' <<<"$INFO")"
if [[ -n "$VERSAO_ESPERADA" ]]; then
  VERSAO_ATUAL=$(jq -r '.versao' <<<"$INFO")
  if [[ "$VERSAO_ATUAL" == "$VERSAO_ESPERADA" ]]; then
    ok "Versão publicada confere (${VERSAO_ATUAL})"
  else
    erro "Versão publicada é ${VERSAO_ATUAL}, esperado ${VERSAO_ESPERADA}"
  fi
fi

# --- 3. Endpoints de negócio (somente leitura)
checar "GET /api/dashboard"                 200 "${BASE_URL}/api/dashboard"
checar "GET /api/pontos-coleta"             200 "${BASE_URL}/api/pontos-coleta?pageNumber=1&pageSize=5"
checar "GET /api/descartes"                 200 "${BASE_URL}/api/descartes?pageNumber=1&pageSize=5"
checar "GET /api/alertas"                   200 "${BASE_URL}/api/alertas?pageNumber=1&pageSize=5"
checar "GET /api/relatorios-compliance"     200 "${BASE_URL}/api/relatorios-compliance?pageNumber=1&pageSize=5"
checar "GET /swagger/v1/swagger.json"       200 "${BASE_URL}/swagger/v1/swagger.json"

# --- 4. Segurança: endpoint crítico sem token deve ser bloqueado
checar "POST /api/descartes sem token (bloqueado)" 401 \
  -X POST "${BASE_URL}/api/descartes" -H "Content-Type: application/json" -d '{}'

# --- 5. Fluxo crítico com JWT (apenas CI e staging)
if [[ "$ESCRITA" == "true" ]]; then
  checar "POST /api/auth/login" 200 \
    -X POST "${BASE_URL}/api/auth/login" -H "Content-Type: application/json" \
    -d '{"username":"operador","senha":"Operador@123"}'
  TOKEN=$(jq -r '.token // empty' /tmp/smoke_body.json 2>/dev/null || true)

  if [[ -n "$TOKEN" ]]; then
    checar "POST /api/descartes com JWT" 201 \
      -X POST "${BASE_URL}/api/descartes" \
      -H "Content-Type: application/json" -H "Authorization: Bearer ${TOKEN}" \
      -d '{"idPonto":5,"idGerador":1,"tipoResiduo":"ORGANICO","pesoKg":1.5}'
    echo "     $(jq -c '{mensagem: .mensagem, ocupacao: .percentualOcupacao, status: .statusPonto}' /tmp/smoke_body.json 2>/dev/null || true)"
  else
    erro "Login não retornou token"
  fi
fi

echo
if [[ "$FALHAS" -gt 0 ]]; then
  echo "💥 Smoke test FALHOU (${FALHAS} verificação(ões) com erro)."
  exit 1
fi
echo "🎉 Smoke test OK em ${BASE_URL}"
