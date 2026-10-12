# Guia de configuração do deploy — GitHub + Neon + Render

Este guia leva o projeto do seu computador até os dois ambientes na nuvem
(**staging** e **produção**), com o pipeline do GitHub Actions fazendo o deploy sozinho.
Tudo é gratuito e você pode entrar em todas as plataformas com a sua conta do GitHub.

```
 Seu PC ──git push──▶ GitHub (código + Actions + GHCR)
                          │  1. build + testes
                          │  2. imagem Docker ──▶ ghcr.io/SEU_USUARIO/cidades-esg-inteligentes
                          │  3. deploy hook ───▶ Render: cidades-esg-staging ──▶ Neon: esg_staging
                          └─ 4. deploy hook ───▶ Render: cidades-esg-producao ─▶ Neon: esg_production
```

> Ordem importa: o Render precisa que a imagem já exista no GHCR, e a imagem só existe
> depois da 1ª execução do pipeline. Por isso o roteiro é: **push → pipeline gera a imagem →
> cria os serviços no Render → configura o GitHub → roda o pipeline de novo.**

---

## Etapa 0 — Testar localmente (Docker Desktop)

```powershell
cd cidades-esg-inteligentes
Copy-Item .env.example .env
docker compose up -d --build
docker compose ps        # aguarde api e db como "healthy"
```
Abra http://localhost:8080/swagger e http://localhost:8080/api/info. 📸 *Print 07.*

---

## Etapa 1 — Repositório no GitHub

1. Em https://github.com/new crie o repositório **`cidades-esg-inteligentes`**, **Public**, sem README.
   (Público é necessário para usar *Required reviewers* de graça e simplifica o GHCR.)
2. No terminal, dentro da pasta do projeto:
   ```bash
   git init
   git add .
   git commit -m "feat: containerização e pipeline CI/CD"
   git branch -M main
   git remote add origin https://github.com/SEU_USUARIO/cidades-esg-inteligentes.git
   git push -u origin main
   ```
3. Aba **Actions**: o pipeline começa sozinho. Os jobs **1** e **2** devem ficar verdes;
   os jobs **3** e **4** aparecem como *skipped* (ainda não há URLs configuradas) — isso é esperado.
4. Torne a imagem pública: no seu perfil do GitHub → **Packages** →
   `cidades-esg-inteligentes` → **Package settings** → *Danger Zone* → **Change visibility → Public**.
   📸 *Print 04 (página do pacote).*

---

## Etapa 2 — Bancos PostgreSQL no Neon

1. Entre em https://neon.tech com o GitHub e crie o projeto **`cidades-esg`**
   (região **AWS US East 1 — N. Virginia**, perto do Render).
2. Menu **Databases → New Database**: crie **`esg_staging`** e **`esg_production`**.
3. Clique em **Connect**, escolha o banco, **desligue "Connection pooling"** e copie a string
   no formato **.NET**. Se só aparecer o formato URI, converta assim:
   ```
   postgresql://USUARIO:SENHA@HOST/esg_staging?sslmode=require
   ⟶ Host=HOST;Database=esg_staging;Username=USUARIO;Password=SENHA;SSL Mode=Require
   ```
   Guarde as duas strings (staging e production) — vão no Render no próximo passo.

> As tabelas são criadas automaticamente pela API na primeira inicialização.

---

## Etapa 3 — Serviços no Render (staging e produção)

Repita para cada ambiente, trocando os valores da tabela:

| Campo | Staging | Produção |
|---|---|---|
| Name | `cidades-esg-staging` | `cidades-esg-producao` |
| `ASPNETCORE_ENVIRONMENT` | `Staging` | `Production` |
| `ConnectionStrings__DefaultConnection` | string do `esg_staging` | string do `esg_production` |
| `Jwt__SecretKey` | uma chave aleatória (≥ 32 caracteres) | **outra** chave aleatória |

1. https://render.com → entre com o GitHub → **New + → Web Service**.
2. Escolha **Existing Image** e informe:
   `ghcr.io/SEU_USUARIO/cidades-esg-inteligentes:latest` (tudo minúsculo).
3. Region: **Virginia (US East)** · Instance Type: **Free**.
4. **Environment Variables** (Add Environment Variable):
   - `ASPNETCORE_ENVIRONMENT` = (tabela acima)
   - `Database__Provider` = `PostgreSQL`
   - `ConnectionStrings__DefaultConnection` = (tabela acima)
   - `Jwt__SecretKey` = (tabela acima)
5. **Advanced → Health Check Path:** `/health`.
6. **Deploy Web Service**. Aguarde o log mostrar `Banco de dados pronto` e abra
   `https://NOME.onrender.com/api/info` — deve aparecer `"ambiente": "Staging"` (ou `Production`).
7. Em **Settings → Deploy Hook**, copie a URL (`https://api.render.com/deploy/srv-...?key=...`).
   ⚠️ Ela é um segredo: não coloque no código.

> No plano Free o serviço "dorme" após 15 min sem uso e leva ~1 min para acordar.
> O pipeline já espera por isso (até 15 min).

---

## Etapa 4 — Configurar o GitHub (variables, secrets e environments)

**4.1 Variáveis do repositório** — *Settings → Secrets and variables → Actions → aba Variables → New repository variable*

| Nome | Valor (sem barra no final) |
|---|---|
| `STAGING_URL` | `https://cidades-esg-staging.onrender.com` |
| `PRODUCTION_URL` | `https://cidades-esg-producao.onrender.com` |

**4.2 Environments** — *Settings → Environments → New environment*

- **`staging`** → *Add environment secret* → `RENDER_DEPLOY_HOOK_URL` = deploy hook do serviço de staging.
- **`production`** → *Add environment secret* → `RENDER_DEPLOY_HOOK_URL` = deploy hook do serviço de produção.
  Marque **Required reviewers**, adicione o seu usuário e clique em **Save protection rules**
  (assim o deploy em produção espera sua aprovação — um portão de qualidade real).

---

## Etapa 5 — Rodar o pipeline completo

Aba **Actions → CI/CD - Cidades ESG Inteligentes → Run workflow** (ou faça qualquer commit na `main`).

1. Jobs 1 e 2 rodam como antes.
2. Job 3 faz o deploy em **staging**, espera a nova versão e roda o smoke test.
3. O pipeline para em **"Waiting for review"** → clique em **Review deployments → production → Approve and deploy**.
4. Job 4 faz o deploy em **produção** e roda o smoke test somente leitura.

---

## Etapa 6 — Prints para o README e para o PDF

Salve em `docs/prints/` com estes nomes (o README já aponta para eles):

| Arquivo | O que capturar |
|---|---|
| `01-pipeline-completo.png` | Tela da execução com os 4 jobs verdes (grafo do workflow) |
| `02-build-testes.png` | Aba *Summary* com a tabela de testes, ou o log do passo de testes |
| `03-docker-compose-ci.png` | Log do job 2 no passo "Smoke test no container" |
| `04-imagem-ghcr.png` | Página do pacote no GHCR com as tags `latest` e `sha-...` |
| `05-deploy-staging.png` | Log do job 3 ("Deploy concluído" + smoke test OK) |
| `06-deploy-producao.png` | Tela de aprovação e/ou log do job 4 |
| `07-docker-local.png` | Terminal com `docker compose ps` (healthy) + navegador no Swagger local |
| `08-staging.png` | Navegador em `STAGING_URL/api/info` e/ou Swagger mostrando **[Staging]** |
| `09-producao.png` | Navegador em `PRODUCTION_URL/api/info` e/ou Swagger mostrando **[Production]** |
| `10-render.png` | Dashboard do Render com os dois serviços *Live* |

---

## Problemas comuns

| Sintoma | Causa provável | Solução |
|---|---|---|
| Jobs 3 e 4 *skipped* | `STAGING_URL`/`PRODUCTION_URL` não criadas (ou criadas dentro do environment) | Crie como **repository variables** (Etapa 4.1) |
| `Secret RENDER_DEPLOY_HOOK_URL não configurado` | Secret criado no lugar errado | Ele fica **dentro de cada environment**, com esse nome exato |
| Render: `400 Bad Request` no deploy hook | A imagem do serviço não é `ghcr.io/<usuario>/cidades-esg-inteligentes` | Corrija a Image URL do serviço (tudo minúsculo) |
| Render não consegue baixar a imagem | Pacote do GHCR privado | Torne público (Etapa 1.4) ou adicione credencial (token `read:packages`) |
| `/health` retorna 503 | Connection string errada ou banco inexistente | Revise a variável no Render (formato .NET, `SSL Mode=Require`) |
| Timeout esperando a versão | Serviço Free "dormindo"/deploy lento | Rode de novo; confira os logs do Render |
| `docker compose up` reclama de variável | `.env` não criado | `Copy-Item .env.example .env` |
