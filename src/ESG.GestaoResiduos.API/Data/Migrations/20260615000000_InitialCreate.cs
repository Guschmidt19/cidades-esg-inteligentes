using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

#nullable disable

namespace ESG.GestaoResiduos.API.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GERADOR_RESIDUO",
            columns: table => new
            {
                ID_GERADOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                TIPO = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                DOCUMENTO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                DATA_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GERADOR_RESIDUO", x => x.ID_GERADOR);
            });

        migrationBuilder.CreateTable(
            name: "PONTO_COLETA",
            columns: table => new
            {
                ID_PONTO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                LOCALIZACAO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                TIPO_RESIDUO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                CAPACIDADE_MAX_KG = table.Column<decimal>(type: "NUMBER(10,2)", precision: 10, scale: 2, nullable: false),
                QTDE_ATUAL_KG = table.Column<decimal>(type: "NUMBER(10,2)", precision: 10, scale: 2, nullable: false),
                STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PONTO_COLETA", x => x.ID_PONTO);
            });

        migrationBuilder.CreateTable(
            name: "USUARIO",
            columns: table => new
            {
                ID_USUARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                USERNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                SENHA_HASH = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                ROLE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
            });

        migrationBuilder.CreateTable(
            name: "DESCARTE",
            columns: table => new
            {
                ID_DESCARTE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                ID_PONTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                ID_GERADOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                DATA_DESCARTE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                TIPO_RESIDUO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                PESO_KG = table.Column<decimal>(type: "NUMBER(10,2)", precision: 10, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DESCARTE", x => x.ID_DESCARTE);
                table.ForeignKey(
                    name: "FK_DESCARTE_GERADOR_RESIDUO_ID_GERADOR",
                    column: x => x.ID_GERADOR,
                    principalTable: "GERADOR_RESIDUO",
                    principalColumn: "ID_GERADOR",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DESCARTE_PONTO_COLETA_ID_PONTO",
                    column: x => x.ID_PONTO,
                    principalTable: "PONTO_COLETA",
                    principalColumn: "ID_PONTO",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ALERTA_COLETA",
            columns: table => new
            {
                ID_ALERTA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                ID_PONTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                DATA_ALERTA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                PERCENTUAL_OCUPACAO = table.Column<decimal>(type: "NUMBER(5,2)", precision: 5, scale: 2, nullable: false),
                STATUS_ALERTA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                MENSAGEM = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ALERTA_COLETA", x => x.ID_ALERTA);
                table.ForeignKey(
                    name: "FK_ALERTA_COLETA_PONTO_COLETA_ID_PONTO",
                    column: x => x.ID_PONTO,
                    principalTable: "PONTO_COLETA",
                    principalColumn: "ID_PONTO",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RELATORIO_COMPLIANCE",
            columns: table => new
            {
                ID_RELATORIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                    .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                ID_PONTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                MES_REFERENCIA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                ANO_REFERENCIA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                TOTAL_KG_COLETADO = table.Column<decimal>(type: "NUMBER(12,2)", precision: 12, scale: 2, nullable: false),
                QTD_DESCARTES = table.Column<int>(type: "NUMBER(10)", nullable: false),
                CONFORME = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                DATA_GERACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RELATORIO_COMPLIANCE", x => x.ID_RELATORIO);
                table.ForeignKey(
                    name: "FK_RELATORIO_COMPLIANCE_PONTO_COLETA_ID_PONTO",
                    column: x => x.ID_PONTO,
                    principalTable: "PONTO_COLETA",
                    principalColumn: "ID_PONTO",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GERADOR_RESIDUO_DOCUMENTO",
            table: "GERADOR_RESIDUO",
            column: "DOCUMENTO",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DESCARTE_ID_GERADOR",
            table: "DESCARTE",
            column: "ID_GERADOR");

        migrationBuilder.CreateIndex(
            name: "IX_DESCARTE_ID_PONTO_DATA_DESCARTE",
            table: "DESCARTE",
            columns: new[] { "ID_PONTO", "DATA_DESCARTE" });

        migrationBuilder.CreateIndex(
            name: "IX_ALERTA_COLETA_ID_PONTO_STATUS_ALERTA",
            table: "ALERTA_COLETA",
            columns: new[] { "ID_PONTO", "STATUS_ALERTA" });

        migrationBuilder.CreateIndex(
            name: "IX_RELATORIO_COMPLIANCE_ID_PONTO_MES_REFERENCIA_ANO_REFERENCIA",
            table: "RELATORIO_COMPLIANCE",
            columns: new[] { "ID_PONTO", "MES_REFERENCIA", "ANO_REFERENCIA" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_USUARIO_USERNAME",
            table: "USUARIO",
            column: "USERNAME",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ALERTA_COLETA");
        migrationBuilder.DropTable(name: "DESCARTE");
        migrationBuilder.DropTable(name: "RELATORIO_COMPLIANCE");
        migrationBuilder.DropTable(name: "USUARIO");
        migrationBuilder.DropTable(name: "GERADOR_RESIDUO");
        migrationBuilder.DropTable(name: "PONTO_COLETA");
    }
}
