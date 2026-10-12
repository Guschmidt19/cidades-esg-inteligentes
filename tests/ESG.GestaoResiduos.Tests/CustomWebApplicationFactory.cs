using ESG.GestaoResiduos.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ESG.GestaoResiduos.Tests;

/// <summary>
/// Fábrica de aplicação para testes de integração. Substitui o provedor real
/// (PostgreSQL/Oracle) por um banco InMemory, evitando dependência externa —
/// por isso os testes rodam no pipeline de CI sem precisar de banco.
/// Cada fábrica usa um banco InMemory exclusivo, isolando as classes de teste.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"EsgGestaoResiduosTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove o registro do DbContext configurado para o banco real.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            // Registra um banco InMemory exclusivo para os testes.
            // O InMemory não suporta transações: ignoramos o aviso para poder
            // testar o fluxo de registro de descarte (que usa transação).
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
                       .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Constrói o provedor e popula os dados de teste.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}
