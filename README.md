<p align="center">
  <img src="assets/alpegram-mascot.png" alt="AlpeGram mascot: a claymotion-style monitor character wearing headphones, heart-shaped blue eyes, and a blue bunny clip acting as the circular logo" width="220" />
  <br />
  <em>AlpeGram the monitor — always listening, always remembering.</em>
</p>

# AlpeGram

Persistent project memory for AI coding agents. A **.NET 10** engine (SQLite + FTS5, HTTP daemon, CLI) that lives as an **independent module** and integrates **natively** with [Pi](https://github.com/alexlocal-works/pi) — no MCP.

Inspired by [engram](https://github.com/Gentleman-Programming/engram), built to be your own: adapt it freely, run it standalone, or plug it into your Pi-based harness.

```
Pi harness (AlpefePI)
    ↓ native extension (pi-extension/, registers mem_* tools)
AlpeGram daemon (.NET 10, HTTP on 127.0.0.1:8765)
    ↓
SQLite + FTS5 (~/.alpegram/alpegram.db)
```

## What it does

- **Memory per project** — resolves the project from the working directory (git remote when available), so each project has its own memory namespace.
- **Structured observations** — What / Why / Where / Learned, with a searchable title and type.
- **Full-text search** — SQLite FTS5 over observations.
- **Topic keys** — stable keys (`architecture/auth-model`) to keep evolving knowledge in one place.
- **Sessions + handoff** — `session_summary` saves goal, discoveries, and next steps so the next session recovers context.

## Architecture

| Project | Role |
|---|---|
| `src/AlpeGram.Core` | Domain + SQLite/FTS5 store (`AlpeGramStore`) + project resolver |
| `src/AlpeGram.Server` | HTTP daemon (ASP.NET Core minimal API) |
| `src/AlpeGram.Cli` | Standalone CLI (no daemon needed) |
| `pi-extension/` | Native Pi extension registering the `mem_*` tools |

## Run the daemon

```bash
cd src/AlpeGram.Server
dotnet run -c Release
# listens on http://127.0.0.1:8765 (override with ALPEGRAM_URLS)
# DB at ~/.alpegram/alpegram.db (override with ALPEGRAM_DB)
```

## HTTP API

| Method | Path | Purpose |
|---|---|---|
| GET | `/health` | Health + DB path |
| GET | `/api/projects` | List projects |
| GET | `/api/projects/current?cwd=` | Resolve project for a cwd |
| POST | `/api/observations?cwd=` | Save an observation |
| GET | `/api/observations/{id}` | Get one observation |
| GET | `/api/observations?cwd=&limit=` | Recent observations |
| GET | `/api/observations/topic/{key}?cwd=` | Observations by topic key |
| GET | `/api/search?cwd=&q=&limit=` | FTS5 search |
| GET | `/api/topics?cwd=` | Existing topic keys |
| POST | `/api/sessions/start?cwd=` | Start a session |
| POST | `/api/sessions/end?cwd=` | End a session (summary) |
| GET | `/api/sessions?cwd=` | List sessions |

Project scope: pass `cwd` (or `project`) as a query param; the daemon resolves the project from it.

## CLI (standalone)

```bash
alpegram project [cwd]
alpegram save <title> [--content] [--what] [--why] [--where] [--learned] [--topic] [--type] [--project] [--cwd]
alpegram search <query> [--limit] [--project] [--cwd]
alpegram list [--limit] [--project] [--cwd]
alpegram topics [--project] [--cwd]
alpegram session-start [--id] [--project] [--cwd]
alpegram session-end <id> [--summary] [--goal] [--next] [--project] [--cwd]
alpegram projects
```

## Pi integration (native, no MCP)

The `pi-extension/` directory is a Pi extension that registers the `mem_*` tools. Pi discovers it via the `pi.extensions` field in its `package.json` (or by adding the path to the `extensions` array in `~/.pi/agent/settings.json`).

Tools: `mem_current_project`, `mem_save`, `mem_search`, `mem_context`, `mem_timeline`, `mem_get_observation`, `mem_suggest_topic_key`, `mem_session_summary`.

```bash
cd pi-extension && npm install   # typebox + jiti (for the verify script)
```

Verify the extension against a running daemon:

```bash
node --experimental-strip-types --no-warnings verify-extension.mjs
```

## License

MIT
