# language: pt

Funcionalidade: Autenticação de usuários (Governança ESG)
  Para garantir rastreabilidade e segurança nas operações de coleta de resíduos
  Como operador do sistema de gestão de resíduos
  Quero me autenticar e receber um token JWT

  @positivo @contrato
  Cenário: Login com credenciais válidas retorna um token JWT
    Dado que informo o usuário "admin" e a senha "Admin@123"
    Quando envio a requisição de login
    Então o status da resposta deve ser 200
    E a resposta deve conter um token JWT
    E o perfil retornado deve ser "Administrador"
    E a resposta deve respeitar o contrato "login-response"

  @negativo
  Cenário: Login com senha incorreta é rejeitado
    Dado que informo o usuário "admin" e a senha "SenhaErrada"
    Quando envio a requisição de login
    Então o status da resposta deve ser 422
    E o campo "title" da resposta deve ser "Regra de negócio violada"
    E a resposta deve respeitar o contrato "problem-details"

  @negativo
  Cenário: Login sem usuário e sem senha é recusado por validação
    Dado que informo o usuário "" e a senha ""
    Quando envio a requisição de login
    Então o status da resposta deve ser 400