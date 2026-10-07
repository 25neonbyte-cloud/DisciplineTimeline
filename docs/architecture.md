# Arquitetura

## Objetivo

Separar interface, regras de domínio e infraestrutura para que a lógica de disciplina temporal seja testável sem depender de WPF, relógio real ou banco de produção.

## Projetos

### DisciplineTimeline.App

Responsável por:

- WPF;
- composição via Generic Host;
- DI;
- logging;
- ViewModels via CommunityToolkit.Mvvm;
- futuras integrações de janela, tray e notificações.

Windows App SDK está disponível nesta camada para APIs modernas do Windows quando necessário.

### DisciplineTimeline.Core

Não depende de WPF nem EF Core.

Responsável por:

- modelos;
- contratos de repositório;
- regras antifraude;
- regras temporais;
- métricas;
- políticas por categoria.

Toda regra dependente de relógio deve receber TimeProvider.

### DisciplineTimeline.Infrastructure

Responsável por:

- SQLite;
- EF Core;
- implementação dos repositórios;
- bootstrap do banco;
- integrações externas ao domínio.

## Persistência

Banco local padrão:

%LOCALAPPDATA%\DisciplineTimeline\discipline-timeline.db

O bootstrap inicial usa EF Core EnsureCreated para estabelecer o schema da fundação. Antes da primeira alteração de schema de produto, o fluxo deve migrar para migrations versionadas via dotnet-ef.

## Testes

xUnit v3.

Dois níveis iniciais:

- testes unitários do Core com FakeTimeProvider;
- teste de integração SQLite temporário.

O CI roda em windows-latest e é a validação automatizada de build/test/publish.

## Regras estruturais

- OriginalPlannedDate nunca é substituída por CurrentPlannedDate.
- flags históricas não dependem apenas do estado atual.
- categorias são persistidas e extensíveis.
- recuperação referencia a tarefa original.
- recorrências devem gerar ocorrências independentes.
- DateTime.Now e DateTime.Today não devem ser usados diretamente nas regras do domínio.


## Timeline e métricas (etapa v0.2)

- \`IMetricsReadRepository\` recupera dados por intervalo, incluindo bônus recuperado
  fora do período quando sua tarefa original está dentro dele.
- \`MetricsSnapshot\` é puro e calcula relatórios sem EF ou WPF.
- Os indicadores diários utilizam a data operacional corrente; alterações para outros dias
  também aparecem na data original como registros de demandas movidas.
- A entrega mensal/de ciclo mantém o denominador ancorado na data originalmente planejada.
  Recuperação posterior aumenta a entrega geral, mas não corrige a consistência temporal.
- Atraso por prazo é flag histórica mesmo quando concluído no mesmo dia (correto temporalmente).
- O ciclo inicial (06–25/10/2026) é adicionado no primeiro bootstrap.
- Os relatórios por ciclo são janelas pelas datas originais; novas tarefas
  criadas dentro de ciclos conhecidos também recebem CycleId.
- Dias futuros não são classificados como zero; dias sem planejamento têm percentual indefinido.
