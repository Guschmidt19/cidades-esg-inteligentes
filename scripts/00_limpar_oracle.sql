-- ============================================================
--  LIMPEZA DO SCHEMA ORACLE (FIAP)
--  Remove os objetos do projeto anterior (PL/SQL) para que o
--  EF Core possa recriar o esquema via Migrations sem conflito.
--  Execute UMA VEZ no seu schema antes do 'dotnet run'.
--  Os triggers e sequences são removidos junto com as tabelas.
-- ============================================================

BEGIN
  FOR t IN (
    SELECT table_name
      FROM user_tables
     WHERE table_name IN (
        'ALERTA_COLETA',
        'DESCARTE',
        'RELATORIO_COMPLIANCE',
        'PONTO_COLETA',
        'GERADOR_RESIDUO',
        'USUARIO',
        '__EFMigrationsHistory'
     )
  ) LOOP
    EXECUTE IMMEDIATE 'DROP TABLE "' || t.table_name || '" CASCADE CONSTRAINTS PURGE';
  END LOOP;
END;
/

-- (opcional) Conferir que não sobrou nenhuma das tabelas:
-- SELECT table_name FROM user_tables
--  WHERE table_name IN ('ALERTA_COLETA','DESCARTE','RELATORIO_COMPLIANCE',
--                       'PONTO_COLETA','GERADOR_RESIDUO','USUARIO','__EFMigrationsHistory');

COMMIT;
