namespace MasteringAgentFramework.Shared;

public static class EnvLoader
{
    public static void LoadRepoRootEnv(string? repoRoot = null)
    {
        string root = repoRoot ?? FindRepoRoot();
        string envPath = Path.Combine(root, ".env");

        if (!File.Exists(envPath))
        {
            return;
        }

        foreach (string line in File.ReadLines(envPath))
        {
            string trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            int separator = trimmed.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            string key = trimmed[..separator].Trim();
            string value = trimmed[(separator + 1)..].Trim().Trim('"');

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MasteringAgentFramework.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
