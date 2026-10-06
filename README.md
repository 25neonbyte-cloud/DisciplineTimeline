# Discipline Timeline

Aplicativo desktop Windows para planejamento pessoal, disciplina temporal e análise do histórico real de execução.

## Stack

- C#
- .NET 8
- WPF
- SQLite
- Windows 10/11 x64

## Estado atual

A branch `develop` contém a base inicial do MVP.

O documento funcional de referência está em:

`docs/CONTEXTO_MESTRE.md`

## Princípio central

O sistema deve manter separadas as dimensões de:

- planejamento original;
- execução real;
- consistência temporal;
- qualidade do planejamento.

Concluir uma tarefa não apaga atrasos, perdas ou reagendamentos anteriores.

## Build local

Requisito: .NET 8 SDK em Windows.

```bat
scripts\build-windows.bat
```

Saída esperada:

`artifacts\win-x64\`
