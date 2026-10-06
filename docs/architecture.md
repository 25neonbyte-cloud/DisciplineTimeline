# Arquitetura inicial

## Objetivo

Manter o núcleo de regras independente da interface e preservar histórico.

## Camadas iniciais

- **Models**: estruturas persistidas e contratos de domínio.
- **Services**: persistência SQLite e, nas próximas etapas, regras temporais.
- **ViewModels**: estado de apresentação.
- **Views/WPF**: interação com o usuário.

## Persistência

SQLite local em:

`%LOCALAPPDATA%\DisciplineTimeline\discipline-timeline.db`

O banco possui controle explícito de versão por `SchemaInfo`.

## Decisões relevantes

- `OriginalPlannedDate` e `CurrentPlannedDate` são campos distintos.
- flags históricas não dependem apenas do estado atual.
- categorias são dados persistidos, não enum fechado.
- recuperação referencia a tarefa original por `RecoveredFromTaskId`.
- recorrência terá ocorrências independentes; não será implementada como sobrescrita do mesmo registro.

## Próximas responsabilidades de domínio

1. CRUD de tarefas.
2. motor temporal de atraso/perda.
3. reagendamento com preservação histórica.
4. recuperação e bônus.
5. política específica por categoria.
6. métricas.
