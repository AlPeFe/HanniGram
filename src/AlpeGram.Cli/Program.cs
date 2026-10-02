using AlpeGram.Core.Store;

// AlpeGram CLI — use the engine standalone, without the daemon.
// Usage:
//   alpegram project [cwd]                 resolve project for a cwd
//   alpegram save <title> [--content] [--what] [--why] [--where] [--learned] [--topic] [--type] [--project] [--cwd]
//   alpegram search <query> [--limit] [--project] [--cwd]
//   alpegram list [--limit] [--project] [--cwd]
//   alpegram topics [--project] [--cwd]
//   alpegram session-start [--id] [--project] [--cwd]
//   alpegram session-end <id> [--summary] [--goal] [--next] [--project] [--cwd]
//   alpegram projects

var dbPath = Env("ALPEGRAM_DB") ?? Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".alpegram", "alpegram.db");
using var store = new AlpeGramStore(dbPath);

var cliArgs = Args();
if (cliArgs.Length == 0) { Help(); return; }

var cmd = cliArgs[0];
var rest = cliArgs.Skip(1).ToArray();
var project = Flag(rest, "--project");
var cwd = Flag(rest, "--cwd") ?? Directory.GetCurrentDirectory();
var resolvedProject = project ?? ProjectResolver.Resolve(cwd);

try
{
    switch (cmd)
    {
        case "project":
            Console.WriteLine(resolvedProject);
            break;

        case "save":
        {
            var title = rest.FirstOrDefault(a => !a.StartsWith("--")) ?? throw new ArgumentException("title required");
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            var obs = store.SaveObservation(p.Id, title,
                Flag(rest, "--content") ?? "",
                Flag(rest, "--what"), Flag(rest, "--why"), Flag(rest, "--where"),
                Flag(rest, "--learned"), Flag(rest, "--topic"), Flag(rest, "--type"));
            Console.WriteLine($"saved #{obs.Id} [{p.Name}] {obs.Title}");
            break;
        }

        case "search":
        {
            var q = rest.FirstOrDefault(a => !a.StartsWith("--")) ?? throw new ArgumentException("query required");
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            var limit = IntFlag(rest, "--limit", 10);
            foreach (var hit in store.Search(p.Id, q, limit))
                Console.WriteLine($"[{hit.Rank:F2}] #{hit.Id} {hit.Title} — {Truncate(hit.Content, 80)}");
            break;
        }

        case "list":
        {
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            var limit = IntFlag(rest, "--limit", 50);
            foreach (var obs in store.ListObservations(p.Id, limit))
                Console.WriteLine($"#{obs.Id} [{obs.TopicKey ?? "-"}] {obs.Title} ({obs.CreatedAtUtc:yyyy-MM-dd})");
            break;
        }

        case "topics":
        {
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            foreach (var t in store.SuggestTopicKeys(p.Id))
                Console.WriteLine(t);
            break;
        }

        case "session-start":
        {
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            var s = store.StartSession(p.Id, Flag(rest, "--id") ?? Guid.NewGuid().ToString("N"));
            Console.WriteLine($"session {s.SessionId} started [{p.Name}]");
            break;
        }

        case "session-end":
        {
            var id = rest.FirstOrDefault(a => !a.StartsWith("--")) ?? throw new ArgumentException("session id required");
            var p = store.GetOrCreateProject(resolvedProject, cwd);
            store.EndSession(p.Id, id, Flag(rest, "--summary"), Flag(rest, "--goal"), Flag(rest, "--next"));
            Console.WriteLine($"session {id} ended");
            break;
        }

        case "projects":
            foreach (var p in store.ListProjects())
                Console.WriteLine($"{p.Name} ({p.RootPath ?? "-"})");
            break;

        default:
            Help();
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"error: {ex.Message}");
    Environment.ExitCode = 1;
}

static string? Env(string name) => Environment.GetEnvironmentVariable(name);
static string[] Args() => Environment.GetCommandLineArgs().Skip(1).ToArray();
static string? Flag(string[] a, string name)
{
    for (var i = 0; i < a.Length - 1; i++)
        if (a[i] == name) return a[i + 1];
    return null;
}
static int IntFlag(string[] a, string name, int def)
    => int.TryParse(Flag(a, name), out var v) ? v : def;
static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";

static void Help() => Console.WriteLine("""
    AlpeGram — persistent project memory for AI coding agents.

    Usage:
      alpegram project [cwd]
      alpegram save <title> [--content] [--what] [--why] [--where] [--learned] [--topic] [--type] [--project] [--cwd]
      alpegram search <query> [--limit] [--project] [--cwd]
      alpegram list [--limit] [--project] [--cwd]
      alpegram topics [--project] [--cwd]
      alpegram session-start [--id] [--project] [--cwd]
      alpegram session-end <id> [--summary] [--goal] [--next] [--project] [--cwd]
      alpegram projects

    Env: ALPEGRAM_DB (db path), ALPEGRAM_CWD (default cwd)
    """);
