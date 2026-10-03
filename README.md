<p align="center">
  <img src="assets/hannigram-mascot.png" alt="HanniGram mascot: a claymotion-style monitor character wearing headphones, heart-shaped blue eyes, and a blue bunny clip acting as the circular logo" width="220" />
  <br />
  <em>HanniGram the monitor — always listening, always remembering.</em>
</p>

# HanniGram

Memoria persistente por proyecto para agentes de IA de código. Un motor **.NET 10** (SQLite + FTS5, daemon HTTP, CLI) que vive como **módulo independiente** y se integra **nativamente** con [HanniPI](https://github.com/AlPeFe/HanniPI) — sin MCP.

Inspirado en [engram](https://github.com/Gentleman-Programming/engram), construido para que sea tuyo: adáptalo libremente, ejecútalo standalone, o conéctalo a tu harness basado en Pi.

```
Harness HanniPI
    ↓ extensión nativa (pi-extension/, registra tools mem_*)
HanniGram daemon (.NET 10, HTTP en 127.0.0.1:8765)
    ↓
SQLite + FTS5 (~/.hannigram/hannigram.db)
```

## Qué hace

- **Memoria por proyecto** — resuelve el proyecto desde el directorio de trabajo (git remote cuando está disponible), así cada proyecto tiene su propio espacio de memoria.
- **Observaciones estructuradas** — What / Why / Where / Learned, con título y tipo buscables.
- **Búsqueda de texto completo** — SQLite FTS5 sobre las observaciones.
- **Topic keys** — claves estables (`architecture/auth-model`) para mantener el conocimiento evolutivo en un solo lugar.
- **Sesiones + handoff** — `session_summary` guarda objetivo, descubrimientos y próximos pasos para que la siguiente sesión recupere el contexto.

## Arquitectura

| Proyecto | Rol |
|---|---|
| `src/HanniGram.Core` | Dominio + store SQLite/FTS5 (`HanniGramStore`) + resolver de proyecto |
| `src/HanniGram.Server` | Daemon HTTP (ASP.NET Core minimal API) |
| `src/HanniGram.Cli` | CLI standalone (no necesita daemon) |
| `pi-extension/` | Extensión nativa de Pi que registra las tools `mem_*` |

## Ejecutar el daemon

```bash
cd src/HanniGram.Server
dotnet run -c Release
# escucha en http://127.0.0.1:8765 (override con HANNIGRAM_URLS)
# BD en ~/.hannigram/hannigram.db (override con HANNIGRAM_DB)
```

## HTTP API

| Método | Ruta | Propósito |
|---|---|---|
| GET | `/health` | Health + ruta de la BD |
| GET | `/api/projects` | Listar proyectos |
| GET | `/api/projects/current?cwd=` | Resolver proyecto para un cwd |
| POST | `/api/observations?cwd=` | Guardar una observación |
| GET | `/api/observations/{id}` | Obtener una observación |
| GET | `/api/observations?cwd=&limit=` | Observaciones recientes |
| GET | `/api/observations/topic/{key}?cwd=` | Observaciones por topic key |
| GET | `/api/search?cwd=&q=&limit=` | Búsqueda FTS5 |
| GET | `/api/topics?cwd=` | Topic keys existentes |
| POST | `/api/sessions/start?cwd=` | Iniciar una sesión |
| POST | `/api/sessions/end?cwd=` | Terminar una sesión (summary) |
| GET | `/api/sessions?cwd=` | Listar sesiones |

Ámbito de proyecto: pasa `cwd` (o `project`) como query param; el daemon resuelve el proyecto desde ahí.

## CLI (standalone)

```bash
hannigram project [cwd]
hannigram save <title> [--content] [--what] [--why] [--where] [--learned] [--topic] [--type] [--project] [--cwd]
hannigram search <query> [--limit] [--project] [--cwd]
hannigram list [--limit] [--project] [--cwd]
hannigram topics [--project] [--cwd]
hannigram session-start [--id] [--project] [--cwd]
hannigram session-end <id> [--summary] [--goal] [--next] [--project] [--cwd]
hannigram projects
```

## Integración con HanniPI (nativa, sin MCP)

El directorio `pi-extension/` es una extensión de Pi que registra las tools `mem_*`. Pi la descubre vía el campo `pi.extensions` de su `package.json` (o añadiendo la ruta al array `extensions` de `~/.pi/agent/settings.json`).

Tools: `mem_current_project`, `mem_save`, `mem_search`, `mem_context`, `mem_timeline`, `mem_get_observation`, `mem_suggest_topic_key`, `mem_session_summary`.

```bash
cd pi-extension && npm install   # typebox + jiti (para el script de verificación)
```

Verifica la extensión contra un daemon en ejecución:

```bash
node --experimental-strip-types --no-warnings verify-extension.mjs
```

## Licencia

MIT
