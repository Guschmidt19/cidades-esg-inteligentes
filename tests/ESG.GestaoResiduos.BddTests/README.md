# Testes automatizados de API — BDD (Gherkin) + Contrato (JSON Schema)

Atividade **Testes Automatizados** (FIAP) aplicada ao projeto **Cidades ESG Inteligentes**
(API de Gestão de Resíduos e Reciclagem, ASP.NET Core .NET 8 + PostgreSQL).

| Item | Tecnologia |
|---|---|
| BDD / Gherkin (em português, `# language: pt`) | **Reqnroll 3.3.4** + xUnit 2.9 |
| Testes de API (status code + corpo JSON) | `HttpClient` + `System.Text.Json` |
| Testes de contrato | **JSON Schema (draft-07)** validado com **NJsonSchema 11.6** |
| Execução local / CI | `dotnet test`, Docker Compose, GitHub Actions |

Os testes são **caixa-preta**: chamam a API real por HTTP (a URL vem da variável de ambiente
`API_BASE_URL`, padrão `http://localhost:8080`), exatamente como um cliente faria.

## Estrutura

```
tests/ESG.GestaoResiduos.BddTests/
├── Features/            # cenários Gherkin (6 arquivos .feature, 27 cenários)
├── StepDefinitions/     # implementação dos passos (C#)
├── Schemas/             # 13 contratos JSON Schema
├── Support/             # ApiContext (HttpClient/JWT/validações) e configuração do xUnit
├── Dockerfile           # contêiner de execução dos testes
└── README.md
docker-compose.bdd.yml   # API + PostgreSQL + testes (na raiz do repositório)
scripts/run-bdd-tests.sh / run-bdd-tests.ps1
```

## Como executar

### Opção 1 — Docker (recomendada, um comando, banco sempre limpo)

Requisito: Docker Desktop / Docker Engine com Compose v2.

```bash
# Linux / macOS / Git Bash
bash scripts/run-bdd-tests.sh
```
```powershell
# Windows PowerShell
.\scripts\run-bdd-tests.ps1
```

O script cria o `.env` (se não existir), sobe **PostgreSQL + API + testes**, espera a API ficar
*healthy*, executa todos os cenários, grava o relatório `.trx` em `TestResults-BDD/`, derruba tudo
(`down -v`) e devolve o código de saída dos testes (0 = todos passaram).

Equivalente manual:
```bash
cp .env.example .env
docker compose -f docker-compose.yml -f docker-compose.bdd.yml up --build --abort-on-container-exit --exit-code-from bdd-tests
docker compose -f docker-compose.yml -f docker-compose.bdd.yml down -v
```

### Opção 2 — Local com `dotnet test` (API já em execução)

Requisitos: .NET SDK 8+ e Docker.

```bash
cp .env.example .env                 # PowerShell: Copy-Item .env.example .env
docker compose up -d --build         # sobe API + banco -> http://localhost:8080
docker compose ps                    # aguarde "healthy"

dotnet test tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj
# Outra URL:  API_BASE_URL=https://minha-api  dotnet test ...   (PowerShell: $env:API_BASE_URL="...")
```

> Dica: para um estado 100% reprodutível (ex.: `dashboard` espera 6 pontos do *seed*), rode
> `docker compose down -v` antes de subir o ambiente.

### Opção 3 — CI (GitHub Actions)

O workflow `.github/workflows/ci-cd.yml` possui o job **“2. Testes BDD (Gherkin) e contrato
(JSON Schema)”**: sobe API + PostgreSQL com Docker Compose, executa os testes, publica o resumo
na aba *Summary* e os artefatos `resultados-bdd` (`.trx`) e `docker-compose-bdd-logs`.
O deploy em *staging* só acontece se esse job passar.

## Cenários (27) e pilar ESG

| Feature | Pilar ESG | Cenários | Positivos | Negativos |
|---|---|---|---|---|
| `autenticacao.feature` | Governança | 3 | 1 | 2 |
| `pontos_coleta.feature` | Ambiental | 4 | 3 | 1 |
| `descartes.feature` | Ambiental / Governança | 7 | 3 | 4 |
| `Alertas.feature` | Ambiental | 6 | 3 | 3 |
| `dashboard_relatorios.feature` | Governança | 5 | 5 | 0 |
| `infraestrutura.feature` | Governança | 2 | 2 | 0 |
| **Total** | | **27** | **17** | **10** |

Tags: `@positivo`, `@negativo`, `@contrato` (valida JSON Schema), `@esg` (regra de negócio ESG).
Executar só um grupo: `dotnet test --filter "Category=negativo"`.

## Endpoints cobertos

`POST /api/auth/login` · `GET /api/pontos-coleta` · `GET /api/pontos-coleta/{id}` ·
`GET /api/descartes` · `POST /api/descartes` · `GET /api/alertas` · `PUT /api/alertas/{id}/atender` ·
`GET /api/dashboard` · `GET /api/relatorios-compliance` · `GET /api/relatorios-compliance/consolidado` ·
`GET /health` · `GET /api/info`.

## Como o teste de contrato funciona

Cada cenário marcado com *“E a resposta deve respeitar o contrato "x"”* carrega
`Schemas/x.json` e valida o corpo recebido. Se a API remover um campo obrigatório, mudar um tipo ou
um valor de *enum*, o teste falha listando o caminho do campo e a regra violada.
