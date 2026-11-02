<#
 Executa os testes BDD (Gherkin) + contrato (JSON Schema) da API ESG (Windows / PowerShell).

 Uso:
   .\scripts\run-bdd-tests.ps1            # modo docker (padrão): sobe API + banco + testes
   .\scripts\run-bdd-tests.ps1 local      # API já no ar (http://localhost:8080); roda dotnet test
#>
param([ValidateSet("docker","local")][string]$Modo = "docker")
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

if ($Modo -eq "docker") {
    if (-not (Test-Path .env)) { Copy-Item .env.example .env; Write-Host ">> .env criado a partir de .env.example" }
    New-Item -ItemType Directory -Force TestResults-BDD | Out-Null
    $compose = @("-f","docker-compose.yml","-f","docker-compose.bdd.yml")
    docker compose @compose down -v 2>$null
    docker compose @compose up --build --abort-on-container-exit --exit-code-from bdd-tests
    $codigo = $LASTEXITCODE
    docker compose @compose down -v
    Write-Host ">> Resultados (.trx) em: TestResults-BDD\  | código de saída: $codigo"
    exit $codigo
}
else {
    if (-not $env:API_BASE_URL) { $env:API_BASE_URL = "http://localhost:8080" }
    Write-Host ">> Testando a API em $env:API_BASE_URL"
    dotnet test tests/ESG.GestaoResiduos.BddTests/ESG.GestaoResiduos.BddTests.csproj `
        --logger "trx;LogFileName=resultado-bdd.trx" --logger "console;verbosity=normal" `
        --results-directory ./TestResults-BDD
    exit $LASTEXITCODE
}
