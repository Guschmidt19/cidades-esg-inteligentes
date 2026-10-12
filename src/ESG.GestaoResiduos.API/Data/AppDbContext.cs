using ESG.GestaoResiduos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Data;

/// <summary>
/// Contexto EF Core do sistema ESG de Gestão de Resíduos. Mapeia as entidades
/// para as tabelas Oracle definidas no projeto de Banco de Dados Avançado.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GeradorResiduo> Geradores => Set<GeradorResiduo>();
    public DbSet<PontoColeta> PontosColeta => Set<PontoColeta>();
    public DbSet<Descarte> Descartes => Set<Descarte>();
    public DbSet<AlertaColeta> Alertas => Set<AlertaColeta>();
    public DbSet<RelatorioCompliance> Relatorios => Set<RelatorioCompliance>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeradorResiduo>(e =>
        {
            e.ToTable("GERADOR_RESIDUO");
            e.HasKey(x => x.IdGerador);
            e.Property(x => x.IdGerador).HasColumnName("ID_GERADOR").ValueGeneratedOnAdd();
            e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(150).IsRequired();
            e.Property(x => x.Tipo).HasColumnName("TIPO").HasMaxLength(2)
                .HasConversion<string>().IsRequired();
            e.Property(x => x.Documento).HasColumnName("DOCUMENTO").HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(100).IsRequired();
            e.Property(x => x.DataCadastro).HasColumnName("DATA_CADASTRO").IsRequired();
            e.HasIndex(x => x.Documento).IsUnique();
        });

        modelBuilder.Entity<PontoColeta>(e =>
        {
            e.ToTable("PONTO_COLETA");
            e.HasKey(x => x.IdPonto);
            e.Property(x => x.IdPonto).HasColumnName("ID_PONTO").ValueGeneratedOnAdd();
            e.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
            e.Property(x => x.Localizacao).HasColumnName("LOCALIZACAO").HasMaxLength(200).IsRequired();
            e.Property(x => x.TipoResiduo).HasColumnName("TIPO_RESIDUO").HasMaxLength(50)
                .HasConversion<string>().IsRequired();
            e.Property(x => x.CapacidadeMaxKg).HasColumnName("CAPACIDADE_MAX_KG")
                .HasPrecision(10, 2).IsRequired();
            e.Property(x => x.QtdeAtualKg).HasColumnName("QTDE_ATUAL_KG")
                .HasPrecision(10, 2).IsRequired();
            e.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            e.Ignore(x => x.PercentualOcupacao);
        });

        modelBuilder.Entity<Descarte>(e =>
        {
            e.ToTable("DESCARTE");
            e.HasKey(x => x.IdDescarte);
            e.Property(x => x.IdDescarte).HasColumnName("ID_DESCARTE").ValueGeneratedOnAdd();
            e.Property(x => x.IdPonto).HasColumnName("ID_PONTO").IsRequired();
            e.Property(x => x.IdGerador).HasColumnName("ID_GERADOR").IsRequired();
            e.Property(x => x.DataDescarte).HasColumnName("DATA_DESCARTE").IsRequired();
            e.Property(x => x.TipoResiduo).HasColumnName("TIPO_RESIDUO").HasMaxLength(50)
                .HasConversion<string>().IsRequired();
            e.Property(x => x.PesoKg).HasColumnName("PESO_KG").HasPrecision(10, 2).IsRequired();

            e.HasOne(x => x.Ponto).WithMany(p => p.Descartes)
                .HasForeignKey(x => x.IdPonto).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Gerador).WithMany(g => g.Descartes)
                .HasForeignKey(x => x.IdGerador).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.IdPonto, x.DataDescarte });
        });

        modelBuilder.Entity<AlertaColeta>(e =>
        {
            e.ToTable("ALERTA_COLETA");
            e.HasKey(x => x.IdAlerta);
            e.Property(x => x.IdAlerta).HasColumnName("ID_ALERTA").ValueGeneratedOnAdd();
            e.Property(x => x.IdPonto).HasColumnName("ID_PONTO").IsRequired();
            e.Property(x => x.DataAlerta).HasColumnName("DATA_ALERTA").IsRequired();
            e.Property(x => x.PercentualOcupacao).HasColumnName("PERCENTUAL_OCUPACAO")
                .HasPrecision(5, 2).IsRequired();
            e.Property(x => x.StatusAlerta).HasColumnName("STATUS_ALERTA").HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            e.Property(x => x.Mensagem).HasColumnName("MENSAGEM").HasMaxLength(300);

            e.HasOne(x => x.Ponto).WithMany(p => p.Alertas)
                .HasForeignKey(x => x.IdPonto).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.IdPonto, x.StatusAlerta });
        });

        modelBuilder.Entity<RelatorioCompliance>(e =>
        {
            e.ToTable("RELATORIO_COMPLIANCE");
            e.HasKey(x => x.IdRelatorio);
            e.Property(x => x.IdRelatorio).HasColumnName("ID_RELATORIO").ValueGeneratedOnAdd();
            e.Property(x => x.IdPonto).HasColumnName("ID_PONTO").IsRequired();
            e.Property(x => x.MesReferencia).HasColumnName("MES_REFERENCIA").IsRequired();
            e.Property(x => x.AnoReferencia).HasColumnName("ANO_REFERENCIA").IsRequired();
            e.Property(x => x.TotalKgColetado).HasColumnName("TOTAL_KG_COLETADO")
                .HasPrecision(12, 2).IsRequired();
            e.Property(x => x.QtdDescartes).HasColumnName("QTD_DESCARTES").IsRequired();
            e.Property(x => x.Conforme).HasColumnName("CONFORME").HasMaxLength(1)
                .HasConversion(v => v ? "S" : "N", v => v == "S").IsRequired();
            e.Property(x => x.DataGeracao).HasColumnName("DATA_GERACAO").IsRequired();

            e.HasOne(x => x.Ponto).WithMany(p => p.Relatorios)
                .HasForeignKey(x => x.IdPonto).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.IdPonto, x.MesReferencia, x.AnoReferencia }).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("USUARIO");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("ID_USUARIO").ValueGeneratedOnAdd();
            e.Property(x => x.Username).HasColumnName("USERNAME").HasMaxLength(50).IsRequired();
            e.Property(x => x.SenhaHash).HasColumnName("SENHA_HASH").HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasColumnName("ROLE").HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
