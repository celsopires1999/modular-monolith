### global.json

Por que precisamos do global.json?

Porque ele torna o comportamento do SDK previsível no projeto.

Sem global.json, o comando dotnet usa “o SDK mais novo instalado” na máquina. Isso pode causar diferenças entre:

- Sua máquina
- Container
- CI/CD
- Máquina de outro dev


Com global.json, você fixa a major/minor (no seu caso 8.0.421) e permite só patch compatível (latestPatch), evitando surpresas de build, restore, testes e tooling.

Resumo prático:

1. Não é obrigatório para funcionar.
2. É altamente recomendado para consistência e reprodutibilidade.
3. Ele não “força runtime da extensão”, só define o SDK usado pelo projeto/comandos dotnet naquele diretório.