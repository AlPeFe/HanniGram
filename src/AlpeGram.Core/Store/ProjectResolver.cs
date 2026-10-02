namespace AlpeGram.Core.Store;

/// <summary>
/// Resolves a working directory to a stable project name, mirroring engram's
/// project-aware reads. Uses the git remote (owner/repo) when available, else
/// the directory name. Falls back to a normalized absolute path for non-git dirs.
/// </summary>
public static class ProjectResolver
{
    public static string Resolve(string cwd)
    {
        var gitRemote = TryGetGitRemote(cwd);
        if (gitRemote != null) return gitRemote;

        var dirName = Path.GetFileName(Path.TrimEndingDirectorySeparator(cwd));
        if (!string.IsNullOrWhiteSpace(dirName)) return dirName;

        return cwd.Replace('\\', '/').Trim('/').Replace('/', '-');
    }

    private static string? TryGetGitRemote(string cwd)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "config --get remote.origin.url")
            {
                WorkingDirectory = cwd,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p == null) return null;
            var url = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit(3000);
            if (p.ExitCode != 0 || string.IsNullOrEmpty(url)) return null;

            // Normalize: https://github.com/owner/repo.git | git@github.com:owner/repo.git
            url = url.TrimEnd('/');
            if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) url = url[..^4];
            var parts = url.Split([':', '/'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var owner = parts[^2];
                var repo = parts[^1];
                return $"{owner}/{repo}";
            }
            return url;
        }
        catch
        {
            return null;
        }
    }
}
