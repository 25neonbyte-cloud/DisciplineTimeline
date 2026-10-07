# Discipline Timeline — Documento Mestre de Contexto e Continuidade

## 1. Objetivo deste documento

Este documento deve permitir que um novo chat assuma o desenvolvimento do projeto **Discipline Timeline** sem depender de conversas anteriores.

A função do novo chat é **continuar a execução**, não rediscutir conceitos já fechados, salvo quando houver conflito técnico real, ambiguidade objetiva ou necessidade de decisão que ainda não esteja documentada.

O projeto será versionado em Git e executado/testado posteriormente em uma máquina Windows do usuário.

---

# 2. Contexto do projeto

O projeto nasceu da necessidade de transformar um planejamento pessoal de 20 dias em um sistema desktop persistente, dinâmico e interativo.

O usuário inicialmente precisava organizar o período de **06/10/2026 a 25/10/2026**, conciliando:

- cuidado integral do filho de 1 ano e 9 meses de segunda a sexta;
- retorno progressivo a uma rotina de acordar às 07:30;
- prática diária de Wim Hof;
- 30 minutos diários de exercícios em casa;
- finalização e comercialização do Wallpaper Scheduler;
- produção de propaganda para um sistema de gestão administrativa hospitalar;
- organização de rotina, compromissos e produtividade.

Durante a definição do sistema, o escopo deixou de ser apenas uma agenda de 20 dias e passou a ser um **organizador pessoal desktop permanente**, capaz de medir não apenas execução, mas também disciplina temporal e qualidade do planejamento.

O planejamento de 06/10 a 25/10 será apenas o primeiro ciclo registrado no sistema.

---

# 3. Princípio central do produto

O sistema não deve funcionar como um simples to-do list.

O princípio central é:

> **Concluir uma tarefa não apaga o fato de que ela atrasou, foi perdida, reagendada ou executada fora do planejamento original.**

O sistema deve preservar permanentemente a diferença entre:

1. o que foi planejado;
2. quando deveria ter sido executado;
3. quando foi realmente executado;
4. se foi alterado;
5. se atrasou;
6. se foi perdido;
7. se foi recuperado posteriormente.

O objetivo é impedir que a produtividade aparente mascare falta de disciplina ou planejamento inadequado.

---

# 4. Filosofia das métricas

O sistema deve medir pelo menos três dimensões independentes:

## 4.1. Execução

Responde:

> Quanto do que foi planejado foi efetivamente realizado?

Exemplo:

- 100 tarefas planejadas;
- 100 concluídas;
- entrega = 100%.

---

## 4.2. Consistência temporal

Responde:

> Quanto do que foi planejado foi executado no período correto?

Exemplo:

- 100 tarefas concluídas;
- apenas 52 concluídas no dia planejado;
- entrega = 100%;
- consistência temporal = 52%.

O sistema nunca deve confundir essas duas métricas.

---

## 4.3. Qualidade do planejamento

O sistema deve registrar alterações do planejamento original.

Exemplo:

- uma tarefa estava prevista para 08/10;
- antes do vencimento foi movida para 10/10;
- isso não deve apagar o planejamento original;
- deve existir um indicador de reagendamento/alteração.

Com isso será possível gerar futuramente feedbacks como:

> "Você concluiu suas tarefas, mas alterou o planejamento original com frequência. Talvez o problema esteja na concepção do plano e não na execução."

---

# 5. Regra fundamental de pontuação

Cada tarefa vale **1 ponto**.

Prioridade NÃO altera pontuação.

Uma obrigação de baixa consequência e uma obrigação de alta consequência continuam sendo obrigações.

A prioridade serve apenas para:

- organização;
- ordenação;
- sinalização visual;
- tomada de decisão.

Ela nunca deve valer mais pontos.

Essa decisão é intencional: o sistema deve incentivar disciplina e não criar mecanismos para o usuário justificar postergação ou compensações artificiais.

---

# 6. Estados e flags de tarefa

Estados e flags podem coexistir.

Uma tarefa pode, por exemplo, estar:

- concluída;
- com flag de atraso;
- com registro de reagendamento.

Estados/condições mínimas:

- Planejada
- Em andamento
- Concluída
- Atrasada
- Perdida
- Bônus recuperado
- Cancelada
- Reagendada

Importante: "Atrasada", "Reagendada" e outras condições podem ser flags históricas, não necessariamente estados exclusivos.

---

# 7. Regras temporais

## 7.1. Tarefa com horário apenas de início

Exemplo:

- início: 14:00;
- sem horário final.

Se for feita às 16:00 no mesmo dia:

- conta como feita no dia correto;
- não recebe flag de atraso apenas por ter começado depois;
- o horário de início funciona como referência/notificação, não como prazo de entrega.

---

## 7.2. Tarefa com horário de início e fim

Exemplo:

- início: 14:00;
- fim: 16:00.

Se não for concluída até 16:00:

- passa a ficar atrasada;
- mantém flag de atraso até ser concluída;
- mesmo que seja concluída no mesmo dia, a flag histórica de atraso permanece.

Exemplo:

- planejada 14:00–16:00;
- concluída 18:00;
- resultado:
  - concluída no dia correto;
  - com flag de atraso.

---

## 7.3. Tarefa sem horário

Tarefas sem horário específico:

- entram apenas na meta diária;
- não recebem penalidade por horário;
- precisam apenas ser concluídas dentro do dia planejado para contar como execução temporal correta.

---

# 8. Tarefa perdida e recuperação

Quando uma tarefa não é concluída no dia planejado:

- o registro daquele dia permanece como **perdido**;
- a tarefa não desaparece;
- a data planejada original nunca é sobrescrita.

Se a tarefa for concluída em outro dia:

- no dia original continua registrada como perdida;
- no dia atual entra como **Bônus recuperado**;
- no histórico geral consta que foi concluída posteriormente;
- a recuperação não corrige retroativamente a disciplina do dia perdido.

Exemplo:

Tarefa prevista para 08/10.

Não concluída em 08/10.

Concluída em 10/10.

Resultado:

### 08/10

- tarefa perdida;
- reduz consistência daquele dia.

### 10/10

- aparece como "Bônus recuperado";
- não altera a meta planejada original de 10/10;
- não transforma o dia em mais de 100%.

---

# 9. Regra de bônus

Bônus não deve inflar artificialmente a porcentagem do dia.

Exemplo:

- 5 tarefas planejadas para hoje;
- 5 concluídas;
- 3 tarefas antigas recuperadas.

Resultado:

- execução do dia = 100%;
- bônus recuperados = +3;
- nunca mostrar 160%.

Bônus deve aparecer como métrica separada.

---

# 10. Categorias iniciais

Inicialmente existirão apenas duas categorias:

## 10.1. Miscelânia

Categoria geral para tarefas comuns.

Regras:

- tarefas podem ser recuperadas posteriormente;
- tarefas perdidas podem virar "Bônus recuperado" em outro dia;
- seguem as regras normais de atraso, reagendamento e cancelamento.

---

## 10.2. Exercícios

Categoria com lógica antifraude própria.

Regra central:

> Exercício perdido não pode ser "reposto".

Se o usuário não fizer o exercício no dia:

- fica registrado como perdido;
- não pode ser convertido em recuperação no dia seguinte.

Se no dia seguinte fizer exercício extra:

- pode entrar como bônus;
- não corrige o dia anterior;
- não conta como reposição.

Motivo:

- disciplina;
- integridade da métrica;
- evitar incentivo a sobrecarga física ou comportamento potencialmente prejudicial.

A arquitetura deve permitir futuramente novas categorias com regras próprias.

---

# 11. Antifraude conceitual

O sistema deve evitar mecanismos que permitam ao usuário "trapacear contra si mesmo".

Exemplos:

- não permitir que tarefas recuperadas apaguem dias perdidos;
- não permitir que exercício extra repare treino perdido;
- não permitir que reagendamento apague a data original;
- não permitir que bônus elevem a execução diária acima de 100%;
- não permitir que prioridade distorça pontuação;
- preservar sempre o histórico real.

Esse conceito deve orientar toda decisão futura.

---

# 12. Reagendamento

Se uma tarefa for movida antes do vencimento:

- a alteração é permitida;
- a nova data passa a ser usada operacionalmente;
- a data original deve permanecer registrada;
- deve ser adicionada uma flag de alteração/reagendamento;
- o número total de alterações deve alimentar métricas.

O sistema deve futuramente conseguir informar:

- quantidade de tarefas reagendadas;
- número médio de reagendamentos;
- frequência de alterações;
- possíveis falhas na concepção do planejamento.

---

# 13. Cancelamento

Cancelamento deve ser diferente de não execução.

Uma tarefa cancelada:

- não deve ser tratada como simplesmente esquecida;
- precisa manter o registro de que existiu;
- deve ter estado/flag própria;
- futuramente poderá ter motivo de cancelamento.

Não confundir:

- cancelada;
- perdida;
- concluída;
- reagendada.

---

# 14. Criação de tarefas

Ao criar uma tarefa, o sistema deve oferecer explicitamente:

## Iniciar agora

- cria a tarefa no dia atual;
- horário atual pode ser usado como início;
- o usuário pode opcionalmente definir horário final;
- se houver prazo/fim, aplicam-se as regras normais de atraso.

## Definir data

Permite criar tarefa para:

- hoje;
- data futura;
- ciclo específico.

---

# 15. Tarefas recorrentes

A lógica recorrente já está conceitualmente definida pelo sistema:

- cada ocorrência diária é independente;
- uma ocorrência perdida não deve desaparecer;
- recorrências devem gerar registros próprios;
- categoria pode alterar comportamento;
- exercício, por exemplo, não pode ser recuperado como reposição.

Não criar uma única tarefa recorrente sobrescrevendo estados anteriores.

Cada ocorrência precisa preservar histórico individual.

---

# 16. Notificações

Para tarefas com horário, implementar:

1. aviso 5 minutos antes;
2. aviso no horário exato;
3. aviso quando entrar em atraso;
4. lembretes posteriores configuráveis enquanto permanecer pendente.

As notificações devem ser configuráveis futuramente por:

- tarefa;
- categoria;
- preferência global.

Para o MVP, o comportamento padrão pode ser aplicado globalmente.

---

# 17. Interface principal

O sistema é um aplicativo desktop Windows residente.

Não deve depender de:

- navegador;
- Notion;
- aba permanentemente aberta;
- conexão com internet.

A experiência desejada é semelhante a uma combinação de:

- Sticky Notes;
- timeline;
- agenda;
- dashboard de disciplina.

---

# 18. Layout principal

A interface deve ter:

## 18.1. Navegação

Pode ser lateral ou superior.

Deve permitir acesso a:

- Hoje;
- Timeline;
- Métricas;
- Configurações;
- ciclos/planejamentos.

---

## 18.2. Timeline

Representação visual:

- linha;
- nós correspondendo aos dias;
- hoje destacado;
- passado e futuro visualmente distintos;
- status do dia representado no nó;
- possibilidade de selecionar qualquer dia.

---

## 18.3. Área principal

O preenchimento principal da tela sempre mostra o dia selecionado.

Para "Hoje", exibir prioritariamente:

- data;
- progresso diário;
- tarefas;
- tarefas atrasadas;
- pendências antigas;
- bônus recuperados;
- indicadores rápidos.

---

# 19. Modo compacto

O aplicativo deve possuir um modo compacto semelhante a Sticky Notes.

Objetivo:

- permanecer acessível;
- ocupar pouco espaço;
- permitir interação rápida;
- evitar necessidade de dedicar um monitor ao sistema.

Exemplo conceitual:

```
┌──────────────────────────────────┐
│ TER 06 OUT            DIA 01/20 │
│ ━━━━━━━━━━━━━━━━━━━━━━━  60%     │
│                                  │
│ HOJE                             │
│ ✓ Wim Hof                        │
│ ○ Exercício                      │
│ ○ Wallpaper — Auditoria         │
│                                  │
│ ⚠ PENDÊNCIAS                     │
│ 1 tarefa anterior               │
│                                  │
│ + Nova tarefa       Timeline     │
└──────────────────────────────────┘
```

---

# 20. Always-on-top

Deve existir opção:

- fixar sobre outras janelas;
- liberar comportamento normal.

Não deve ser obrigatoriamente always-on-top.

---

# 21. Tray

Fechar a janela não deve necessariamente encerrar o aplicativo.

O aplicativo deve:

- permanecer residente na bandeja do sistema;
- continuar processando notificações;
- permitir abrir/ocultar rapidamente;
- permitir sair explicitamente pelo tray.

---

# 22. Inicialização com Windows

Deve existir configuração:

- iniciar automaticamente com Windows.

Preferencialmente:

- habilitada por padrão no MVP;
- controlável pela interface.

---

# 23. Persistência

Todos os dados são locais.

Banco escolhido:

**SQLite**

Persistir:

- tarefas;
- horários;
- estados;
- flags;
- data original;
- data atual;
- conclusão real;
- alterações;
- cancelamentos;
- bônus;
- categorias;
- métricas;
- configurações;
- ciclos.

Reiniciar o computador não pode perder estado.

---

# 24. Dados mínimos de uma tarefa

Uma tarefa deve preservar pelo menos:

- ID
- título
- descrição opcional
- categoria
- prioridade
- data de criação
- data planejada original
- data planejada atual
- horário de início planejado
- horário final planejado opcional
- data/hora real de início, se aplicável
- data/hora real de conclusão
- estado atual
- flag de atraso
- flag de reagendamento
- contador de reagendamentos
- flag de cancelamento
- origem da tarefa
- referência à tarefa original em caso de bônus recuperado
- ciclo/planejamento associado
- recorrência, se existir

Nunca sobrescrever dados históricos essenciais.

---

# 25. Métricas mínimas

## Por dia

- planejadas;
- concluídas no dia correto;
- perdidas;
- atrasadas;
- canceladas;
- reagendadas;
- bônus recuperados;
- execução diária em %;
- consistência diária.

---

## Por mês/ciclo

- total planejado;
- total concluído;
- taxa de entrega;
- taxa de consistência;
- tarefas perdidas;
- tarefas recuperadas;
- bônus;
- número de reagendamentos;
- média de alterações;
- dias 100%;
- dias parcialmente cumpridos;
- dias zerados.

Futuramente essas métricas podem alimentar feedback e gamificação.

---

# 26. Gamificação

Não é prioridade do MVP, mas a arquitetura deve permitir.

Já foi definido o termo:

**Bônus recuperado**

A gamificação futura pode utilizar:

- streaks;
- conquistas;
- níveis;
- badges;
- sequência de dias;
- metas;
- feedbacks;
- recompensas.

Nenhuma gamificação futura deve quebrar as regras antifraude.

---

# 27. Tecnologias escolhidas

Stack revisada e aprovada em 06/10/2026:

- C#
- .NET 10 LTS
- WPF
- CommunityToolkit.Mvvm
- Windows App SDK para APIs modernas do Windows quando necessário
- SQLite
- Entity Framework Core 10
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- TimeProvider para toda lógica dependente de relógio
- xUnit v3
- GitHub Actions em runner Windows para build/test/publish

Diretriz arquitetural:

- WPF permanece como camada de interface;
- regras de domínio ficam isoladas em projeto Core;
- persistência e integrações ficam em Infrastructure;
- a UI não deve conter regras temporais;
- DateTime.Now/DateTime.Today não devem ser espalhados pelo domínio: lógica temporal deve depender de TimeProvider;
- testes automatizados devem validar virada de dia, atraso, perda, recuperação e demais regras sensíveis ao tempo;
- Windows App SDK deve ser usado seletivamente, sem transformar o projeto em WinUI 3.

Motivos:

- integração adequada com Windows;
- tray;
- notificações;
- inicialização automática;
- always-on-top;
- aplicativo desktop real;
- armazenamento local;
- testabilidade determinística;
- CI em ambiente Windows real;
- possibilidade futura de expansão.

Evitar transformar o sistema em aplicativo web local.

---

# 28. Plataforma alvo

Inicial:

- Windows 10/11
- x64

Distribuição inicial:

- self-contained win-x64

O usuário executará e testará o projeto posteriormente na própria máquina.

---

# 29. Versionamento Git

Estrutura vigente:

```
DisciplineTimeline/
├─ src/
│  ├─ DisciplineTimeline.App/
│  ├─ DisciplineTimeline.Core/
│  └─ DisciplineTimeline.Infrastructure/
├─ tests/
│  └─ DisciplineTimeline.Tests/
├─ docs/
│  ├─ CONTEXTO_MESTRE.md
│  ├─ architecture.md
│  ├─ functional-rules.md
│  └─ roadmap.md
├─ scripts/
│  └─ build-windows.bat
├─ .github/
│  └─ workflows/
│     └─ ci.yml
├─ Directory.Packages.props
├─ global.json
├─ .gitignore
├─ README.md
└─ DisciplineTimeline.sln
```

Branches sugeridas:

```
main
develop
feature/timeline
feature/notifications
feature/metrics
feature/ui
```

Versões conceituais:

```
v0.1.0 — Core inicial
v0.2.0 — Timeline + métricas
v0.3.0 — Notificações + tray
v0.4.0 — Refinamento UI/UX
v0.5.0 — Gamificação inicial
```

A numeração pode ser adaptada conforme o estado real do repositório.

---

# 30. Planejamento inicial a ser pré-carregado

Primeiro ciclo:

**06/10/2026 → 25/10/2026**

Objetivos do ciclo:

- ajustar horário de sono;
- acordar progressivamente até 07:30;
- retornar ao Wim Hof;
- fazer 30 min de exercício diário;
- finalizar Wallpaper Scheduler;
- revisar UI/UX;
- transformar Wallpaper Scheduler em produto comercial;
- estruturar venda;
- produzir propaganda do Wallpaper;
- produzir propaganda do sistema hospitalar.

---

# 31. Progressão inicial de sono

```
06–08/10 → acordar 09:30 / dormir 01:30
09–11/10 → acordar 09:00 / dormir 01:00
12–14/10 → acordar 08:30 / dormir 00:30
15–17/10 → acordar 08:00 / dormir 00:00
18–21/10 → acordar 07:30 / dormir 23:30
22–25/10 → manter 07:30 / 23:30
```

A meta definitiva é:

- acordar 07:30;
- dormir aproximadamente 23:30.

---

# 32. Rotina familiar relevante

De segunda a sexta:

- filho de 1 ano e 9 meses fica integralmente com o usuário;
- criança costuma cochilar depois do almoço por até 2 horas;
- criança costuma dormir após 21:30;
- enquanto a criança está acordada, usuário consegue apenas pequenas demandas.

Sábado e domingo:

- a mãe fica mais com a criança;
- usuário possui maior disponibilidade de trabalho.

Capacidade estimada:

- dias úteis: aproximadamente 5 horas;
- fins de semana: aproximadamente 8 horas.

O sistema não precisa automatizar essa regra agora, mas esse contexto explica o planejamento inicial.

---

# 33. Rotina física inicial

Wim Hof:

- usuário já praticou anteriormente;
- retorno começa com respiração;
- exposição ao frio será adicionada futuramente.

Exercício:

- 30 minutos;
- em casa;
- sem equipamentos;
- calistenia;
- prancha;
- cadeirinha;
- exercícios corporais semelhantes.

Categoria: Exercícios.

Regra:

- não recuperável como reposição.

---

# 34. Agenda profissional inicial

## 06/10

Wallpaper:

- auditoria do estado atual;
- lista fechada do que falta.

## 07/10

Wallpaper:

- implementação funcional pendente.

## 08/10

Wallpaper:

- finalizar implementação;
- iniciar revisão UI/UX.

## 09/10

Wallpaper:

- UI/UX;
- correções.

## 10/10

Wallpaper:

- bateria completa de testes.

## 11/10

Wallpaper:

- correções;
- build candidata.

## 12/10

Wallpaper:

- teste final;
- congelar versão comercial.

## 13/10

Sistema hospitalar:

- conceito;
- público;
- mensagem da propaganda.

## 14/10

Sistema hospitalar:

- roteiro;
- storyboard;
- captura/seleção de material.

## 15/10

Sistema hospitalar:

- montagem bruta.

## 16/10

Sistema hospitalar:

- edição;
- motion;
- áudio;
- narração;
- acabamento.

## 17/10

Sistema hospitalar:

- revisão;
- render;
- finalização.

## 18/10

Wallpaper comercial:

- produto;
- preço;
- licença;
- entrega;
- suporte.

## 19/10

Wallpaper comercial:

- jornada completa de compra.

## 20/10

Wallpaper comercial:

- definir plataforma/site;
- arquitetura da página.

## 21/10

Wallpaper comercial:

- construir página de venda.

## 22/10

Wallpaper comercial:

- finalizar página;
- checkout;
- entrega.

## 23/10

Wallpaper publicidade:

- conceito;
- roteiro;
- captura.

## 24/10

Wallpaper publicidade:

- produção;
- finalização.

## 25/10

Fechamento:

- revisão geral;
- planejamento da próxima fase.

---

# 35. Projeto Wallpaper Scheduler — contexto necessário

O Wallpaper Scheduler é um aplicativo Windows já em desenvolvimento.

Objetivo:

- troca automática de wallpapers;
- regras configuráveis;
- suporte real a múltiplos monitores.

Estado anterior:

- versão 0.7 reportada operacional;
- build self-contained win-x64 já testado anteriormente.

Recursos centrais existentes/planejados:

- regras ilimitadas;
- dias da semana;
- janelas de tempo;
- períodos atravessando meia-noite;
- wallpaper único para todos os monitores;
- wallpaper individual por monitor;
- estilos Fill/Fit/Span/Center/Stretch/Tile;
- detecção de monitores;
- alternância dinâmica;
- tray;
- iniciar com Windows;
- UI responsiva;
- ajustes de DPI;
- conforto visual;
- Auto Dark Mode;
- f.lux-like;
- gradiente de temperatura/cor;
- futura arquitetura master/slave para rede.

Pendências citadas:

- pequena implementação restante;
- revisão UI/UX;
- nitidez/DPI;
- módulos de conforto visual;
- monitores fantasmas;
- iniciar com Windows habilitado por padrão;
- futura arquitetura master/slave.

Para o ciclo atual, o objetivo é apenas:

- fechar pequena implementação;
- revisar UI/UX;
- validar;
- congelar uma versão comercial;
- estruturar venda;
- produzir propaganda.

Não expandir escopo desnecessariamente.

---

# 36. Sistema hospitalar — contexto necessário

Produto:

- sistema de gestão administrativa hospitalar.

Estado:

- MVP.

Durante este ciclo:

- não há desenvolvimento relevante previsto;
- foco apenas em propaganda;
- apresentação será feita após as eleições.

---

# 37. Regras de execução para novo chat

O novo chat deve:

1. considerar todas as decisões acima como aprovadas;
2. não voltar a perguntar o que já está definido;
3. trabalhar diretamente na execução;
4. preservar lógica histórica;
5. não simplificar o core para um to-do convencional;
6. não apagar flags após conclusão;
7. não transformar tarefas recuperadas em correção retroativa;
8. manter categorias extensíveis;
9. tratar SQLite como fonte persistente;
10. manter o projeto compatível com versionamento Git;
11. entregar arquivos completos sempre que alterar código;
12. evitar trechos soltos quando o usuário solicitar arquivo final;
13. preservar compilabilidade;
14. registrar breaking changes;
15. priorizar MVP funcional antes de gamificação avançada.

---

# 38. Prioridade imediata de desenvolvimento

Próxima sequência recomendada:

## Etapa 1 — Base compilável

- Solution;
- projeto WPF;
- estrutura MVVM simples;
- banco SQLite;
- migrations/init;
- modelos;
- serviços.

## Etapa 2 — Core de tarefas

- CRUD;
- categorias;
- datas;
- horários;
- estados;
- flags;
- iniciar agora;
- definir data.

## Etapa 3 — Motor temporal

- atraso;
- perda;
- recuperação;
- reagendamento;
- cancelamento;
- bônus;
- recorrências.

## Etapa 4 — Timeline

- dias;
- nós;
- seleção;
- hoje;
- passado;
- futuro.

## Etapa 5 — Métricas

- diária;
- ciclo;
- mensal;
- consistência;
- entrega;
- alterações.

## Etapa 6 — Sistema residente

- tray;
- notificações;
- inicialização Windows;
- always-on-top;
- modo compacto.

## Etapa 7 — UI/UX

- acabamento;
- responsividade;
- hierarquia;
- estados visuais;
- acessibilidade.

## Etapa 8 — Testes

- regras;
- persistência;
- virada de dia;
- datas;
- horários;
- recorrência;
- recuperação;
- notificações.

---

# 39. Critérios mínimos de aceitação do MVP

O MVP só deve ser considerado funcional quando for possível:

- criar uma tarefa para hoje;
- criar uma tarefa futura;
- iniciar tarefa imediatamente;
- definir início;
- definir início e fim;
- concluir;
- atrasar;
- perder;
- recuperar posteriormente;
- cancelar;
- reagendar;
- preservar data original;
- diferenciar Miscelânia e Exercícios;
- impedir reposição de exercício perdido;
- mostrar bônus separadamente;
- calcular progresso diário;
- calcular consistência;
- navegar pela timeline;
- persistir após reiniciar;
- receber notificações;
- rodar em tray;
- iniciar com Windows.

---

# 40. Não implementar desta forma

Evitar:

- Electron apenas por conveniência;
- aplicação dependente de navegador;
- armazenamento somente em JSON se comprometer integridade histórica;
- sobrescrever datas originais;
- apagar atrasos após conclusão;
- permitir recuperação de exercício perdido;
- pontuação por peso/prioridade;
- progresso superior a 100% por bônus;
- tratar reagendamento como se o plano original nunca tivesse existido;
- misturar "cancelada" com "não realizada".

---

# 41. Direção conceitual futura

O sistema pode evoluir para um produto maior de autogestão pessoal baseado em:

- disciplina;
- planejamento;
- execução;
- consistência;
- histórico;
- feedback;
- gamificação;
- análise comportamental não clínica;
- ciclos de melhoria.

Mas o MVP deve continuar simples o suficiente para uso diário.

---

# 42. Prompt operacional para novo chat

Use o texto abaixo como instrução ao iniciar uma nova conversa:

> Você está assumindo o desenvolvimento do projeto **Discipline Timeline**, um aplicativo desktop Windows em C#/.NET 10 LTS + WPF + SQLite/EF Core, com arquitetura separada em App, Core e Infrastructure.
>
> Leia integralmente o documento `CONTEXTO_MESTRE.md` antes de propor qualquer alteração.
>
> Todas as regras nele descritas já foram aprovadas. Não volte a questionar conceitos definidos, salvo conflito técnico real.
>
> O projeto será versionado em Git e testado localmente pelo usuário em Windows.
>
> Sua função é continuar a execução de onde o repositório estiver.
>
> Antes de alterar código:
>
> 1. inspecione a estrutura atual;
> 2. identifique a versão;
> 3. identifique o que já está implementado;
> 4. compare com os critérios do documento;
> 5. prossiga para a próxima etapa objetiva.
>
> Preserve compilabilidade, histórico de dados e regras antifraude.
>
> Quando modificar um arquivo, entregue o arquivo completo ou faça a alteração diretamente no repositório quando tiver acesso.
>
> Não transforme o sistema em um simples to-do list.
>
> O núcleo conceitual é:\
> **planejamento original, execução real, consistência temporal e qualidade do planejamento devem permanecer mensuráveis separadamente.**

---

# 43. Estado atual do trabalho

O repositório GitHub foi criado e a base técnica inicial foi implantada na branch develop.

Em 06/10/2026 a arquitetura foi revisada antes do crescimento do core:

- migração de .NET 8 para .NET 10 LTS;
- separação em App, Core e Infrastructure;
- CommunityToolkit.Mvvm;
- EF Core 10 + SQLite;
- Generic Host/DI/Logging;
- TimeProvider para regras temporais;
- xUnit v3;
- GitHub Actions em Windows para restore, build, testes e publish win-x64;
- Windows App SDK disponível seletivamente para integrações modernas do Windows.

O ambiente do chat não precisa ser considerado fonte de validação de build WPF. O workflow de CI do GitHub é a validação automatizada do repositório, complementada por testes manuais de comportamento visual/Windows quando necessário. A fundação revisada foi validada no GitHub Actions em Windows com restore, build sem erros, 5 testes aprovados, publish self-contained win-x64 e geração de artefato.

Portanto, ao assumir o projeto:

1. verificar o conteúdo real do repositório;
2. verificar o último resultado do CI;
3. corrigir qualquer erro de build/teste antes de avançar;
4. não presumir que implementações anteriores estejam perfeitas;
5. usar este documento como fonte de verdade funcional.

---

# 43.1. Estado de execução — Core de tarefas (06/10/2026)

A etapa de Core de tarefas foi implementada e integrada à branch `develop` via PR #1.

Implementado:

- CRUD seguro de tarefas;
- criação por **Definir data**;
- criação por **Iniciar agora**;
- início manual de tarefa planejada no dia operacional;
- conclusão de tarefa;
- persistência de flag histórica de atraso quando concluída após horário final;
- categorias persistidas e carregadas do SQLite;
- edição de tarefa ainda não iniciada sem sobrescrever datas planejadas;
- exclusão física restrita a tarefa planejada sem histórico;
- navegação simples por data para operar tarefas futuras;
- testes unitários do serviço de tarefas;
- teste de integração CRUD contra SQLite.

Decisão de integridade:

- alteração de data de uma tarefa existente não é tratada como edição comum;
- reagendamento terá fluxo próprio na próxima etapa para preservar `OriginalPlannedDate`, flag e contador de reagendamentos;
- tarefa já perdida não pode ser concluída pelo fluxo normal; conclusão posterior deverá usar o futuro fluxo de recuperação/bônus.

Validação:

- CI em Windows aprovado na branch de feature e novamente após merge em `develop`;
- restore, build, testes, publish self-contained win-x64 e upload de artefato concluídos com sucesso.

Próxima etapa objetiva:

- motor temporal persistente;
- transição automática para atraso/perda;
- recuperação e bônus;
- reagendamento;
- cancelamento;
- recorrências.

---

# 43.2. Estado de execução — Timeline e Métricas (06/10/2026)

A etapa foi integrada à branch `develop` pelo PR #3, commit funcional
`fc8f481297b56bac679b895c88207cb82846f4c3`.

Entregas:

- timeline horizontal navegável com passado, hoje e futuro diferenciados;
- seleção por dia, navegação anterior/próximo e calendário;
- indicadores diários de execução, consistência, atraso, perda, bônus e planejamento movido;
- indicadores mensais e por ciclo: entrega, consistência, recuperação, perda, cancelamento,
  reagendamento, média de mudanças e dias completos/parciais/zerados;
- leitura histórica otimizada de SQLite, incluindo bônus recuperado posteriormente;
- cálculos puros na camada Core, com testes automáticos;
- ciclo inicial 06/10–25/10/2026 criado idempotentemente sem mudar o esquema do banco;
- inicialização do banco anterior à inicialização do serviço em segundo plano.

Regras de relatório:

- indicadores diários ancorados na data operacional vigente;
- relatórios mensais e por ciclo ancorados na data ORIGINAL planejada;
- recuperação posterior conta como entrega geral, mas jamais repara consistência do dia;
- bônus separados dos denominadores diários, progresso nunca ultrapassa 100%;
- atraso por horário mantém flag histórica mesmo quando concluído no dia correto;
- mudança de data aparece como alteração do planejamento original;
- dias futuros não são classificados como zerados; dias sem tarefas exibem taxa indefinida;
- relatórios de ciclo usam sua janela de datas originais. CycleId também é atribuído
  a tarefas criadas dentro de ciclos conhecidos.

Validação:

- workflow Windows aprovado na feature e após merge na develop;
- restore, build, 29 testes aprovados, publish win-x64 self-contained, artefato gerado;
- execuções: feature 37551768954, develop 37552005863.

Pendência: validar manualmente UX, renderização, DPI e interação no Windows do usuário.

Próxima etapa: sistema residente — notificações, tray, startup, always-on-top e modo compacto.

---
# 44. Regra final

O sistema deve sempre responder à pergunta:

> **"Eu fiz o que precisava fazer, no momento em que eu havia decidido fazer, e meu planejamento original era realista?"**

Se uma decisão de arquitetura, UI ou métrica esconder alguma dessas três dimensões, ela está errada.