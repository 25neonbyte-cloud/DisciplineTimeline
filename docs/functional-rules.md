# Regras funcionais resumidas

Este arquivo é um índice operacional. A fonte de verdade completa permanece em `CONTEXTO_MESTRE.md`.

## Pontuação

- cada tarefa vale 1 ponto;
- prioridade não altera pontuação;
- bônus não eleva progresso acima de 100%.

## Datas

O sistema nunca deve sobrescrever a data planejada original.

## Atraso

- somente horário de início: não existe atraso por horário no mesmo dia;
- início + fim: após o fim, a tarefa recebe flag histórica de atraso;
- sem horário: deve ser concluída dentro do dia para consistência temporal.

## Perda

Tarefa não concluída no dia planejado permanece perdida naquele dia.

## Recuperação

- Miscelânia: pode ser recuperada posteriormente como bônus separado;
- Exercícios: perda não pode ser reposta.

## Reagendamento

Reagendamento muda a data operacional, preserva a original e incrementa histórico de alteração.

## Cancelamento

Cancelamento é distinto de perda, conclusão e reagendamento.
