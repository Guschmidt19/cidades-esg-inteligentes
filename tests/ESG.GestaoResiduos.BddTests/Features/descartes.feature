# language: pt

Funcionalidade: Registro de descartes de resíduos (Ambiental e Governança ESG)
  Para garantir a rastreabilidade do que é descartado em cada ecoponto
  Como operador autenticado do sistema
  Quero registrar descartes e ter as regras de compliance aplicadas automaticamente

  @positivo @contrato
  Cenário: Listar descartes retorna uma lista paginada
    Quando consulto a lista de descartes
    Então o status da resposta deve ser 200
    E a resposta deve ser paginada
    E a resposta deve respeitar o contrato "descartes-lista"

  @positivo @contrato
  Cenário: Operador autenticado registra um descarte válido
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    Quando registro um descarte de 5 kg de "PLASTICO" no ponto 1 pelo gerador 1
    Então o status da resposta deve ser 201
    E o campo "mensagem" da resposta deve conter "sucesso"
    E a resposta deve respeitar o contrato "descarte-resultado"

  @negativo
  Cenário: Registrar descarte sem autenticação é bloqueado
    Dado que não estou autenticado
    Quando registro um descarte de 5 kg de "PLASTICO" no ponto 1 pelo gerador 1
    Então o status da resposta deve ser 401

  @negativo
  Cenário: Descarte de tipo de resíduo incompatível com o ponto é recusado
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    Quando registro um descarte de 5 kg de "VIDRO" no ponto 1 pelo gerador 1
    Então o status da resposta deve ser 422
    E o campo "title" da resposta deve ser "Regra de negócio violada"
    E a resposta deve respeitar o contrato "problem-details"

  @negativo
  Cenário: Descarte em ponto de coleta inexistente retorna não encontrado
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    Quando registro um descarte de 5 kg de "PLASTICO" no ponto 9999 pelo gerador 1
    Então o status da resposta deve ser 404
    E a resposta deve respeitar o contrato "problem-details"

  @negativo
  Cenário: Descarte com peso zero é recusado por validação
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    Quando registro um descarte de 0 kg de "PLASTICO" no ponto 1 pelo gerador 1
    Então o status da resposta deve ser 400

  @positivo @esg
  Cenário: Descarte que ultrapassa 80% da capacidade gera alerta automático
    Dado que estou autenticado como "operador" com a senha "Operador@123"
    E que o ponto de coleta 6 não possui alerta pendente
    Quando registro um descarte que leva o ponto 6 a 85% da capacidade, de "ELETRONICO" pelo gerador 1
    Então o status da resposta deve ser 201
    E o campo "alertaGerado" da resposta deve ser "true"
    E existe um alerta pendente para o ponto 6