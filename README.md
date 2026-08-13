# LeaderboardRedisMvc

MVP de estudo: ASP.NET Core MVC + Redis. Mostra dois usos clássicos de Redis lado a lado — ranking (Sorted Set) e cache-aside (String + TTL) — sem autenticação, sem persistência fora do Redis.

## Stack

- ASP.NET Core MVC (.NET 10)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) (client oficial .NET pra Redis)
- Redis rodando em container Docker
- [redis-commander](https://github.com/joeferner/redis-commander) — GUI web pra inspecionar as chaves
- [Pico.css](https://picocss.com) (vendorizado localmente) + tema claro próprio, acento no vermelho da marca Redis (`#DC382D`) — top 3 do ranking em dourado/prata/bronze, badge de status do cache, sem Bootstrap/jQuery

## Estrutura

```
Models/LeaderboardEntry.cs, PlayerProfile.cs
Settings/RedisSettings.cs
Services/LeaderboardService.cs     # Sorted Set: pontuação, top N, posição do jogador
Services/PlayerProfileService.cs   # cache-aside: perfil do jogador com TTL
Controllers/LeaderboardController.cs
Views/Leaderboard/                 # Index (ranking + form), Player (perfil)
docker-compose.yml                 # redis + redis-commander
```

## Rodando

Pré-requisitos: .NET 10 SDK, Docker Desktop.

```bash
docker compose up -d   # Redis (porta 6379) + redis-commander (porta 8082)
dotnet run
```

Acesse `http://localhost:5060/Leaderboard` (rota default). Envie pontuações pelo formulário, depois clique em "Perfil" de um jogador — a primeira chamada demora ~1.5s (simula busca cara) e cacheia; recarregando em menos de 30s, a resposta vem instantânea do cache. A página "Sobre" (`/Home`) explica o projeto pra quem chegar sem contexto. `http://localhost:8082` mostra as chaves cruas no Redis.

Config (connection string, TTL) fica em `appsettings.json`, seção `Redis`.

## O que Redis faz aqui

**Ranking — Sorted Set**
Cada jogador é um membro de um `ZSET` (`leaderboard:players`) com a pontuação como "score". Redis mantém tudo ordenado automaticamente:
- `SortedSetIncrementAsync` — soma pontos (equivale a `ZINCRBY`)
- `SortedSetRangeByRankWithScoresAsync` — pega o top N, já ordenado (`ZREVRANGE ... WITHSCORES`)
- `SortedSetRankAsync` / `SortedSetScoreAsync` — posição e pontuação de 1 jogador, sem varrer nada (`ZREVRANK` / `ZSCORE`)

Diferente de SQL/Mongo, não precisa de `ORDER BY` nem índice pra isso — a estrutura já é ordenada por natureza.

**Perfil — cache-aside**
`PlayerProfileService.GetProfileAsync`:
1. Tenta `StringGetAsync("profile:{jogador}")`.
2. Se tem (cache hit): desserializa e devolve — resposta rápida.
3. Se não tem (cache miss): simula uma consulta lenta (`Task.Delay`), monta o perfil, serializa em JSON, grava com `StringSetAsync(..., TimeSpan)` (TTL de 30s) e devolve.

Depois que o TTL expira, a chave some sozinha do Redis e a próxima leitura recalcula — sem job de limpeza manual.

**Invalidação ativa no `AddScoreAsync`**: além do TTL de 30s, toda vez que a pontuação de um jogador muda, `AddScoreAsync` também apaga a chave `profile:{jogador}` (`KeyDeleteAsync`). Isso evita mostrar rank/score desatualizado enquanto o cache ainda não expirou — a próxima leitura do perfil recalcula na hora, mesmo antes do TTL estourar. Cache dos outros jogadores (que não mudaram) continua servindo normal.

## Diferença pros projetos irmãos

O [TodoMongoMvc](https://github.com/YuriLucka/TodoMongoMvc) guarda documentos (dado estruturado, consulta por campo). Este projeto usa Redis como estrutura de dados em memória (Sorted Set) e como cache (String + TTL) — não é feito pra ser fonte de verdade de dados de negócio, e sim pra acelerar leituras e resolver problemas específicos (ranking, cache). Já o [SocialGraphMvc](https://github.com/YuriLucka/SocialGraphMvc) usa Neo4j pra travessia de relacionamento — problema que nem Mongo nem Redis resolvem bem.
