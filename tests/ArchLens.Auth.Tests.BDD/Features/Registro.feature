# language: pt-BR
Funcionalidade: Registro de Usuário
  Como um novo usuário
  Eu quero me registrar na plataforma
  Para poder acessar os recursos do sistema

  Cenário: Registro com dados válidos
    Dado que eu tenho os dados de registro válidos
      | Username  | Email              | Password  | LgpdConsent |
      | testuser  | test@archlens.com  | Test@1234 | true        |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 201
    E a resposta deve conter o campo "userId"
    E a resposta deve conter o campo "username" com valor "testuser"

  Cenário: Registro com username duplicado
    Dado que já existe um usuário registrado com username "existinguser"
    E que eu tenho os dados de registro válidos
      | Username      | Email                | Password  | LgpdConsent |
      | existinguser  | new@archlens.com     | Test@1234 | true        |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 400
    E a resposta deve conter a mensagem "already"

  Cenário: Registro com email duplicado
    Dado que já existe um usuário registrado com email "dup@archlens.com"
    E que eu tenho os dados de registro válidos
      | Username  | Email              | Password  | LgpdConsent |
      | newuser   | dup@archlens.com   | Test@1234 | true        |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 400

  Cenário: Registro sem consentimento LGPD
    Dado que eu tenho os dados de registro válidos
      | Username  | Email              | Password  | LgpdConsent |
      | testuser  | test@archlens.com  | Test@1234 | false       |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 400

  Cenário: Registro com senha fraca
    Dado que eu tenho os dados de registro válidos
      | Username  | Email              | Password | LgpdConsent |
      | testuser  | test@archlens.com  | 123      | true        |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 400

  Cenário: Registro com email inválido
    Dado que eu tenho os dados de registro válidos
      | Username  | Email        | Password  | LgpdConsent |
      | testuser  | invalid-mail | Test@1234 | true        |
    Quando eu envio uma requisição POST para "/auth/register"
    Então a resposta deve ter status code 400
