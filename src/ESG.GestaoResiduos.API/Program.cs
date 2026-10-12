using System.Text;
using ESG.GestaoResiduos.API.Common.Middleware;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Security;
using ESG.GestaoResiduos.API.Services;
using ESG.GestaoResiduos.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// Porta dinâmica (DevOps): plataformas como o Render informam a porta pela
// variável PORT. Sem ela, vale ASPNETCORE_HTTP_PORTS (8080 no container).
// ----------------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Versão da aplicação (o pipeline injeta o SHA do commit em APP_VERSION).
var appVersion = builder.Configuration["APP_VERSION"] ?? "local";
var startedAtUtc = DateTime.UtcNow;

// ----------------------------------------------------------------------------
// Configurações (JWT) — em staging/produção a chave vem de Jwt__SecretKey
// ----------------------------------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

// ----------------------------------------------------------------------------
// Banco de dados — provider escolhido por variável de ambiente (Database__Provider)
//   PostgreSQL (padrão)  -> Docker Compose, staging e produção
//   Oracle               -> banco da FIAP (projeto original, com Migrations)
// Nos testes o DbContext é substituído por InMemory.
// ----------------------------------------------------------------------------
var dbProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";
var usaOracle = dbProvider.Equals("Oracle", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usaOracle)
        options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"));
    else
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ----------------------------------------------------------------------------
// Health checks — usados pelo Docker Compose, pelo pipeline (smoke test) e pelo Render
// ----------------------------------------------------------------------------
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// ----------------------------------------------------------------------------
// Injeção de dependências (Services / MVVM)
// ----------------------------------------------------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDescarteService, DescarteService>();
builder.Services.AddScoped<IPontoColetaService, PontoColetaService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ----------------------------------------------------------------------------
// Autenticação / Autorização JWT Bearer
// ----------------------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ----------------------------------------------------------------------------
// Controllers + serialização de enums como string
// ----------------------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"Cidades ESG Inteligentes – Gestão de Resíduos API [{builder.Environment.EnvironmentName}]",
        Version = appVersion,
        Description = "API RESTful (.NET 8) do tema ESG de Gestão de Resíduos e Reciclagem. "
                    + "Containerizada com Docker e entregue por pipeline CI/CD (GitHub Actions) "
                    + "nos ambientes de staging e produção."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido em /api/auth/login. Ex.: Bearer {token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

// ----------------------------------------------------------------------------
// Pipeline HTTP
// ----------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger habilitado em todos os ambientes para facilitar testes/avaliação.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ESG - Gestão de Resíduos API");
    c.RoutePrefix = "swagger"; // disponível em /swagger
    c.DocumentTitle = $"ESG API – {app.Environment.EnvironmentName}";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Acessar a raiz redireciona para a documentação Swagger.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// /health      -> verifica a aplicação E a conexão com o banco (200 Healthy / 503 Unhealthy)
// /health/live -> só verifica se o processo está de pé (liveness)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            ambiente = app.Environment.EnvironmentName,
            versao = appVersion,
            checks = report.Entries.Select(e => new
            {
                nome = e.Key,
                status = e.Value.Status.ToString(),
                duracaoMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1)
            })
        });
    }
});
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Informações do deploy: prova em qual ambiente/versão a API está rodando.
app.MapGet("/api/info", () => Results.Ok(new
    {
        aplicacao = "Cidades ESG Inteligentes – Gestão de Resíduos API",
        ambiente = app.Environment.EnvironmentName,
        versao = appVersion,
        banco = dbProvider,
        iniciadoEmUtc = startedAtUtc,
        horaServidorUtc = DateTime.UtcNow
    }))
    .WithTags("Infra")
    .WithSummary("Ambiente, versão (commit) e banco em uso pela API.")
    .AllowAnonymous();

// ----------------------------------------------------------------------------
// Criação do schema + seed (ignorado no ambiente de testes, que usa InMemory)
// ----------------------------------------------------------------------------
if (!app.Environment.IsEnvironment("Testing"))
{
    await InicializarBancoAsync(app, usaOracle);
}

app.Run();

// Tenta preparar o banco algumas vezes: em containers o banco pode demorar a
// aceitar conexões, e bancos serverless na nuvem podem estar "acordando".
static async Task InicializarBancoAsync(WebApplication app, bool usaOracle)
{
    const int maxTentativas = 10;
    for (var tentativa = 1; ; tentativa++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (usaOracle)
                await db.Database.MigrateAsync();      // migrations do projeto original (Oracle)
            else
                await db.Database.EnsureCreatedAsync(); // cria o schema a partir do modelo (PostgreSQL)

            await DbSeeder.SeedAsync(db);
            app.Logger.LogInformation("Banco de dados pronto na tentativa {Tentativa}.", tentativa);
            return;
        }
        catch (Exception ex) when (tentativa < maxTentativas)
        {
            app.Logger.LogWarning(ex,
                "Banco indisponível (tentativa {Tentativa}/{Max}). Nova tentativa em 5s...",
                tentativa, maxTentativas);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

/// <summary>Tornado público/parcial para permitir testes de integração com WebApplicationFactory.</summary>
public partial class Program { }
