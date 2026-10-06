# Discipline Timeline

Aplicativo desktop Windows para planejamento pessoal, disciplina temporal e análise do histórico real de execução.

## Stack atual

- C# / .NET 10 LTS
- WPF
- CommunityToolkit.Mvvm
- Windows App SDK
- SQLite + Entity Framework Core 10
- Microsoft.Extensions.Hosting / DI / Logging
- TimeProvider
- xUnit v3
- GitHub Actions em Windows

## Arquitetura

- src/DisciplineTimeline.App: WPF, composição da aplicação e apresentação.
- src/DisciplineTimeline.Core: modelos, contratos e regras de domínio.
- src/DisciplineTimeline.Infrastructure: SQLite/EF Core e integrações.
- tests/DisciplineTimeline.Tests: testes unitários e integração SQLite.

A fonte funcional do projeto é docs/CONTEXTO_MESTRE.md.

## Regra arquitetural

A interface não deve decidir regras de atraso, perda, recuperação ou consistência. Regras dependentes de tempo recebem TimeProvider, permitindo testes determinísticos.

## Validação

O workflow .github/workflows/ci.yml executa em Windows:

1. restore;
2. build Release;
3. testes;
4. publish self-contained win-x64;
5. upload do artefato.

## Build local

Requisito: .NET 10 SDK em Windows.

Execute:

    scripts\build-windows.bat

Saída:

    artifacts\win-x64\
