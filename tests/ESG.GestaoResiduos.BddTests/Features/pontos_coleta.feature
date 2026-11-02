# language: pt

Funcionalidade: Consulta de pontos de coleta (Ambiental ESG)
  Para acompanhar a ocupação dos ecopontos da cidade
  Como cidadão ou gestor público
  Quero consultar os pontos de coleta de resíduos

  @positivo @contrato
  Cenário: Listar pontos de coleta retorna uma lista paginada
    Quando consulto a lista de pontos de coleta
    Então o status da resposta deve ser 200
    E a resposta deve ser paginada
    E a lista "items" não deve estar vazia
    E a resposta deve respeitar o contrato "pontos-lista"

  @positivo @contrato
  Cenário: Buscar um ponto de coleta existente retorna seus dados
    Quando consulto o ponto de coleta de id 1
    Então o status da resposta deve ser 200
    E o campo "nome" da resposta deve ser "Ecoponto Centro"
    E a resposta deve respeitar o contrato "ponto"

  @negativo @contrato
  Cenário: Buscar um ponto de coleta inexistente retorna não encontrado
    Quando consulto o ponto de coleta de id 9999
    Então o status da resposta deve ser 404
    E a resposta deve respeitar o contrato "problem-details"

  @positivo
  Cenário: Filtrar pontos de coleta por tipo de resíduo
    Quando consulto a lista de pontos filtrando pelo tipo "PLASTICO"
    Então o status da resposta deve ser 200
    E todos os itens da lista devem ter "tipoResiduo" igual a "PLASTICO"
