#!/usr/bin/env bash
# =============================================================================
#  Aguarda o Render terminar o deploy: consulta /api/info até que a versão
#  publicada seja igual à imagem recém-enviada pelo pipeline.
#
#  Uso: ./scripts/wait-for-version.sh <URL_BASE> <versao> [timeout_segundos]
# =============================================================================
set -euo pipefail

BASE_URL="${1:?Informe a URL base}"; BASE_URL="${BASE_URL%/}"
VERSAO="${2:?Informe a versão esperada (ex.: sha-1a2b3c4)}"
TIMEOUT="${3:-900}"

echo "⏳ Aguardando ${BASE_URL} servir a versão ${VERSAO} (timeout ${TIMEOUT}s)..."
INICIO=$(date +%s)
while true; do
  ATUAL=$(curl -fsS --max-time 15 "${BASE_URL}/api/info" 2>/dev/null | jq -r '.versao // empty' 2>/dev/null || true)
  DECORRIDO=$(( $(date +%s) - INICIO ))
  if [[ "$ATUAL" == "$VERSAO" ]]; then
    echo "✅ Deploy concluído: ${BASE_URL} está na versão ${ATUAL} (${DECORRIDO}s)."
    exit 0
  fi
  if (( DECORRIDO >= TIMEOUT )); then
    echo "❌ Timeout: a versão publicada ainda é '${ATUAL:-indisponível}'."
    exit 1
  fi
  echo "   ${DECORRIDO}s — versão atual: '${ATUAL:-indisponível}'. Nova verificação em 15s..."
  sleep 15
done
