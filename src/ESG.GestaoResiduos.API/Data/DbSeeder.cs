using ESG.GestaoResiduos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Data;

/// <summary>
/// Popula o banco com dados iniciais equivalentes aos INSERTs do script PL/SQL
/// original, além do usuário de autenticação. Idempotente: só insere se vazio.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Usuarios.CountAsync() == 0)
        {
            db.Usuarios.AddRange(
                new Usuario
                {
                    Username = "admin",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Administrador"
                },
                new Usuario
                {
                    Username = "operador",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Operador@123"),
                    Role = "Operador"
                });
        }

        if (await db.Geradores.CountAsync() == 0)
        {
            db.Geradores.AddRange(
                new GeradorResiduo { Nome = "Ana Paula Ferreira", Tipo = TipoGerador.PF, Documento = "123.456.789-00", Email = "ana.ferreira@email.com" },
                new GeradorResiduo { Nome = "Carlos Eduardo Lima", Tipo = TipoGerador.PF, Documento = "234.567.890-11", Email = "carlos.lima@email.com" },
                new GeradorResiduo { Nome = "Supermercado Verde Ltda", Tipo = TipoGerador.PJ, Documento = "12.345.678/0001-99", Email = "contato@superverde.com.br" },
                new GeradorResiduo { Nome = "Mariana Costa", Tipo = TipoGerador.PF, Documento = "345.678.901-22", Email = "mariana.costa@email.com" },
                new GeradorResiduo { Nome = "Indústria Recicla S.A.", Tipo = TipoGerador.PJ, Documento = "98.765.432/0001-55", Email = "esg@recicla.ind.br" },
                new GeradorResiduo { Nome = "Roberto Alves Santos", Tipo = TipoGerador.PF, Documento = "456.789.012-33", Email = "roberto.santos@email.com" },
                new GeradorResiduo { Nome = "Escola Municipal Vida Verde", Tipo = TipoGerador.PJ, Documento = "11.222.333/0001-44", Email = "secretaria@vidaverde.edu.br" });
        }

        if (await db.PontosColeta.CountAsync() == 0)
        {
            db.PontosColeta.AddRange(
                new PontoColeta { Nome = "Ecoponto Centro", Localizacao = "Av. Paulista, 100 – São Paulo/SP", TipoResiduo = TipoResiduo.PLASTICO, CapacidadeMaxKg = 500, QtdeAtualKg = 0 },
                new PontoColeta { Nome = "Ecoponto Zona Norte", Localizacao = "Rua das Palmeiras, 55 – São Paulo/SP", TipoResiduo = TipoResiduo.PAPEL, CapacidadeMaxKg = 300, QtdeAtualKg = 0 },
                new PontoColeta { Nome = "Ecoponto Jardim Europa", Localizacao = "Rua das Orquídeas, 200 – São Paulo/SP", TipoResiduo = TipoResiduo.VIDRO, CapacidadeMaxKg = 400, QtdeAtualKg = 0 },
                new PontoColeta { Nome = "Ecoponto Vila Madalena", Localizacao = "Rua Harmonia, 80 – São Paulo/SP", TipoResiduo = TipoResiduo.METAL, CapacidadeMaxKg = 350, QtdeAtualKg = 0 },
                new PontoColeta { Nome = "Ecoponto Lapa", Localizacao = "Rua Catão, 300 – São Paulo/SP", TipoResiduo = TipoResiduo.ORGANICO, CapacidadeMaxKg = 600, QtdeAtualKg = 0 },
                new PontoColeta { Nome = "Ecoponto Ipiranga", Localizacao = "Av. Nazaré, 450 – São Paulo/SP", TipoResiduo = TipoResiduo.ELETRONICO, CapacidadeMaxKg = 200, QtdeAtualKg = 0 });
        }

        await db.SaveChangesAsync();
    }
}
