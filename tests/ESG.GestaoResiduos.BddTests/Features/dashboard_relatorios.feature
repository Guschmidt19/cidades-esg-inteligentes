# language: pt

Funcionalidade: Indicadores ESG e relatórios de compliance (Governança ESG)
  Para acompanhar a conformidade ambiental da operação
  Como gestor público
  Quero consultar o dashboard e os relatórios de compliance

  @positivo @contrato
  Cenário: Dashboard retorna os indicadores consolidados
    Quando consulto o dashboard
    Então o status da resposta deve ser 200
    E o campo "totalPontos" da resposta deve ser "6"
    E a resposta deve respeitar o contrato "dashboard-response"

  @positivo @contrato
  Cenário: Listar relatórios de compliance retorna uma lista paginada
    Quando consulto a lista de relatórios de compliance
    Então o status da resposta deve ser 200
    E a resposta deve ser paginada
    E a resposta deve respeitar o contrato "relatorios-lista"

  @positivo
  Cenário: Relatórios de um ano sem dados retornam lista vazia
    Quando consulto os relatórios de compliance do ano 1900
    Então o status da resposta deve ser 200
    E a lista "items" deve estar vazia
    E o campo "totalItems" da resposta deve ser "0"

  @positivo @esg @contrato
  Cenário: Descarte registrado aparece no consolidado de compliance
    Dado que existe ao menos um descarte registrado
    Quando consulto o consolidado de compliance
    Então o status da resposta deve ser 200
    E a resposta deve ser uma lista não vazia
    E a resposta deve respeitar o contrato "relatorios-consolidado"

  @positivo
  Cenário: Consolidado de um ano sem dados retorna lista vazia
    Quando consulto o consolidado de compliance do ano 1900
    Então o status da resposta deve ser 200
    E a resposta deve ser uma lista vazia