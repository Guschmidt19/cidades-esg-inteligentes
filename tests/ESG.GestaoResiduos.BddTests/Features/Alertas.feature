# language: pt

Funcionalidade: Alertas de capacidade e atendimento de coletas (Ambiental ESG)
  Para que a equipe de coleta aja antes de um ecoponto transbordar
  Como operador autenticado do sistema
  Quero consultar os alertas e registrar a coleta realizada

  @positivo @contrato
  Cenário: Listar alertas retorna uma lista paginada
    Quando consulto a lista de alertas
    Então o status da resposta deve ser 200
    E a resposta deve ser paginada
    E a resposta deve respeitar o contrato "alertas-lista"

  @positivo
  Cenário: Filtrar alertas pendentes retorna somente alertas pendentes
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    E que o ponto de coleta 5 possui um alerta pendente
    Quando consulto os alertas com status "PENDENTE"
    Então o status da resposta deve ser 200
    E todos os itens da lista devem ter "statusAlerta" igual a "PENDENTE"

  @positivo @esg @contrato
  Cenário: Atender um alerta registra a coleta e libera a capacidade do ponto
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    E que o ponto de coleta 5 possui um alerta pendente
    Quando atendo o alerta pendente
    Então o status da resposta deve ser 200
    E o campo "statusAlerta" da resposta deve ser "ATENDIDO"
    E o campo "statusPonto" da resposta deve ser "ATIVO"
    E a quantidade atual do ponto deve ser zero
    E a resposta deve respeitar o contrato "atender-alerta-response"

  @negativo
  Cenário: Atender alerta sem autenticação é bloqueado
    Dado que não estou autenticado
    Quando atendo o alerta de id 1
    Então o status da resposta deve ser 401

  @negativo
  Cenário: Atender alerta inexistente retorna não encontrado
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    Quando atendo o alerta de id 999999
    Então o status da resposta deve ser 404
    E a resposta deve respeitar o contrato "problem-details"

  @negativo
  Cenário: Atender um alerta já atendido é recusado
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    E que o ponto de coleta 5 possui um alerta pendente
    Quando atendo o alerta pendente
    E atendo novamente o mesmo alerta
    Então o status da resposta deve ser 422
    E o campo "title" da resposta deve ser "Regra de negócio violada"
    E a resposta deve respeitar o contrato "problem-details"