# language: pt

Funcionalidade: Saúde e rastreabilidade do deploy (Governança ESG)
  Para garantir que a aplicação está no ar e saber exatamente qual versão está rodando
  Como equipe de operação
  Quero consultar a saúde da API e as informações do ambiente

  @positivo @contrato
  Cenário: Health check informa que a API e o banco estão saudáveis
    Quando consulto o endpoint "/health"
    Então o status da resposta deve ser 200
    E o campo "status" da resposta deve ser "Healthy"
    E a resposta deve respeitar o contrato "health-response"

  @positivo @contrato
  Cenário: Info informa o ambiente e a versão em execução
    Quando consulto o endpoint "/api/info"
    Então o status da resposta deve ser 200
    E a resposta deve respeitar o contrato "info-response"