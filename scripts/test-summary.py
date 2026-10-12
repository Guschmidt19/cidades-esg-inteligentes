#!/usr/bin/env python3
"""Lê os arquivos .trx gerados pelo `dotnet test` e escreve um resumo em
Markdown (usado no "Summary" da execução do GitHub Actions)."""
import glob
import sys
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
arquivos = glob.glob(sys.argv[1] if len(sys.argv) > 1 else "TestResults/**/*.trx", recursive=True)

total = passou = falhou = 0
linhas = []
for arquivo in arquivos:
    raiz = ET.parse(arquivo).getroot()
    contadores = raiz.find(".//t:ResultSummary/t:Counters", NS)
    if contadores is not None:
        total += int(contadores.get("total", 0))
        passou += int(contadores.get("passed", 0))
        falhou += int(contadores.get("failed", 0))
    for r in raiz.findall(".//t:UnitTestResult", NS):
        icone = "✅" if r.get("outcome") == "Passed" else "❌"
        nome = r.get("testName", "").replace("ESG.GestaoResiduos.Tests.", "")
        linhas.append(f"| {icone} | `{nome}` | {r.get('duration', '')[:11]} |")

print("## 🧪 Resultado dos testes automatizados (xUnit)\n")
print(f"**Total:** {total} &nbsp;|&nbsp; **Passaram:** {passou} &nbsp;|&nbsp; **Falharam:** {falhou}\n")
print("| | Teste | Duração |")
print("|---|---|---|")
print("\n".join(sorted(linhas)))
