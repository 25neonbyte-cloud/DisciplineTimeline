# Roadmap

## v0.1.0 — Fundação técnica + Core de tarefas

- [x] .NET 10 LTS;
- [x] solution multiprojeto;
- [x] WPF;
- [x] CommunityToolkit.Mvvm;
- [x] Generic Host / DI / Logging;
- [x] SQLite + EF Core;
- [x] categorias iniciais;
- [x] TimeProvider no domínio;
- [x] testes xUnit para regras temporais;
- [x] teste de integração SQLite;
- [x] GitHub Actions Windows para build/test/publish;
- [x] CI validado: build + testes + publish win-x64;
- [x] CRUD seguro de tarefas;
- [x] criar "iniciar agora";
- [x] criar "definir data";
- [x] iniciar tarefa planejada;
- [x] concluir tarefa;
- [x] persistir flag histórica de atraso na conclusão;
- [x] impedir exclusão física de tarefas com histórico;
- [x] navegação simples por data para operar tarefas futuras;
- [x] CI da etapa validado: build + testes + publish win-x64.

## v0.2.0 — Motor temporal + timeline

- [x] persistência automática de transições de atraso/perda;
- [x] recuperação e bônus recuperado;
- [x] reagendamento preservando data original;
- [x] cancelamento distinto de perda;
- [x] recorrências com ocorrências independentes (diária e semanal no MVP);
- [x] motor periódico de manutenção enquanto o aplicativo está ativo;
- [x] contadores visuais de perdidas e bônus;
- [ ] validar CI desta etapa;
- [ ] timeline visual navegável;
- [ ] métricas diárias e por ciclo completas.

## v0.3.0 — Sistema residente

- [ ] notificações com Windows App SDK;
- [ ] tray;
- [ ] iniciar com Windows;
- [ ] always-on-top;
- [ ] modo compacto.

## v0.4.0 — Refinamento UI/UX

- [ ] responsividade;
- [ ] estados visuais;
- [ ] acessibilidade;
- [ ] acabamento.

## v0.5.0 — Gamificação inicial

Somente após o core estar validado.
