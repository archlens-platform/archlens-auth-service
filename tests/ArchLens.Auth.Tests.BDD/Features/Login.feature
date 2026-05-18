# language: pt-BR
Funcionalidade: Login de Usuário
  Como um usuário registrado
  Eu quero fazer login na plataforma
  Para obter um token de acesso

  Cenário: Login com credenciais válidas
    Dado que existe um usuário registrado com username "loginvalid" e senha "Login@123"
    Quando eu envio uma requisição de login com username "loginvalid" e senha "Login@123"
    Então a resposta deve ter status code 200
    E a resposta deve conter o campo "token"
    E a resposta deve conter o campo "username" com valor "loginvalid"

  Cenário: Login com senha incorreta
    Dado que existe um usuário registrado com username "loginerr" e senha "Login@123"
    Quando eu envio uma requisição de login com username "loginerr" e senha "SenhaErrada@1"
    Então a resposta deve ter status code 401

  Cenário: Login com usuário inexistente
    Quando eu envio uma requisição de login com username "naoexiste" e senha "Test@1234"
    Então a resposta deve ter status code 401

  Cenário: Login com conta bloqueada após múltiplas tentativas
    Dado que existe um usuário registrado com username "lockuser" e senha "Lock@1234"
    E que o usuário "lockuser" teve 5 tentativas de login falhas
    Quando eu envio uma requisição de login com username "lockuser" e senha "Lock@1234"
    Então a resposta deve ter status code 401
    E a resposta deve conter a mensagem "locked"

  Cenário: Login retorna role do usuário
    Dado que existe um usuário registrado com username "roleuser" e senha "Role@1234"
    Quando eu envio uma requisição de login com username "roleuser" e senha "Role@1234"
    Então a resposta deve ter status code 200
    E a resposta deve conter o campo "role" com valor "User"
