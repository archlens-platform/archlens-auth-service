# language: pt-BR
Funcionalidade: Dados Pessoais (LGPD)
  Como um usuário autenticado
  Eu quero acessar e excluir meus dados pessoais
  Para exercer meus direitos previstos na LGPD

  Cenário: Exportar dados pessoais com sucesso
    Dado que eu sou um usuário autenticado com role "User"
    E que o usuário autenticado existe no banco de dados
    Quando eu envio uma requisição GET para "/auth/me/data"
    Então a resposta deve ter status code 200
    E a resposta deve conter o campo "username"
    E a resposta deve conter o campo "email"

  Cenário: Exportar dados pessoais sem autenticação
    Dado que eu não estou autenticado
    Quando eu envio uma requisição GET para "/auth/me/data"
    Então a resposta deve ter status code 401

  Cenário: Excluir conta com sucesso
    Dado que eu sou um usuário autenticado com role "User"
    E que o usuário autenticado existe no banco de dados
    Quando eu envio uma requisição DELETE para "/auth/me"
    Então a resposta deve ter status code 204

  Cenário: Excluir conta sem autenticação
    Dado que eu não estou autenticado
    Quando eu envio uma requisição DELETE para "/auth/me"
    Então a resposta deve ter status code 401

  Cenário: Exportar dados de usuário inexistente
    Dado que eu sou um usuário autenticado com role "User"
    Quando eu envio uma requisição GET para "/auth/me/data"
    Então a resposta deve ter status code 404
