using ASL.Api;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public sealed class ModHandler : AslMod
{
    public override void OnLoad(IModContext ctx)
    {
        var modFolder = ctx.ModDirectory;
        var modsFolder = Directory.GetParent(modFolder);
        var assFolder = Directory.GetParent(modsFolder.FullName);
        var modsDisabledFolder = Path.Combine(assFolder.FullName, "modsDisabled");

        if (!Directory.Exists(modsDisabledFolder))
        {
            Directory.CreateDirectory(modsDisabledFolder);
            ctx.Log.Info("modsDisabled folder not found, creating one.");
        }
        else
        {
            ctx.Log.Info("modsDisabled folder found.");
        }

        ctx.Log.Info("ModHandler running!");
        ctx.Menu.AddLabel("! A restart is required after enabling / disabling a mod !");

        string[] mods = Directory.GetDirectories(modsFolder.FullName);
        foreach (string modPath in mods)
        {
            if (Path.GetFileName(modPath) != Path.GetFileName(modFolder))
            {
                ctx.Menu.AddToggle(Path.GetFileName(modPath), true, state => HandleMod(modPath, state, modsDisabledFolder, modsFolder, ctx));
            }
        }
        

        string[] disabledMods = Directory.GetDirectories(modsDisabledFolder);
        foreach (string modPath in disabledMods)
        {
            ctx.Menu.AddToggle(Path.GetFileName(modPath), false, state => HandleMod(modPath, state, modsDisabledFolder, modsFolder, ctx));
        }
        
        
        ctx.Menu.AddButton("Open mods folder", () => Process.Start("explorer.exe", modsFolder.FullName));
        ctx.Menu.AddButton("Open disabled mods folder", () => Process.Start("explorer.exe", modsDisabledFolder));
        ctx.Menu.AddButton("Restart game", RestartGame);
        ctx.Menu.AddButton("Check for updates", () => UpdateChecker(ctx));
    }

    private void HandleMod(string pathToMod, bool enabled, string modsDisabledFolder, DirectoryInfo modsFolder, IModContext ctx)
    {
        if (enabled)
        {
            pathToMod = Path.Combine(modsDisabledFolder, Path.GetFileName(pathToMod));
        }
        else
        {
            pathToMod = Path.Combine(modsFolder.FullName, Path.GetFileName(pathToMod));
        }
        
        ctx.Log.Info($"Function call, pathToMod: {pathToMod}, enabled: {enabled}");
        
        if (Directory.Exists(pathToMod))
        {
            if (enabled)
            {
                ctx.Log.Info("Enabling mod " + Path.GetFileName(pathToMod));
                Directory.Move(pathToMod, Path.Combine(modsFolder.FullName, Path.GetFileName(pathToMod)));
            }
            else
            {
                ctx.Log.Info("Disabling mod " + Path.GetFileName(pathToMod));
                Directory.Move(pathToMod, Path.Combine(modsDisabledFolder, Path.GetFileName(pathToMod)));
            }
        }
        else
        {
            ctx.Log.Error($"pathToMod is invalid! ({pathToMod})");
        }
    }

    private void RestartGame()
    {
        Process.Start(new ProcessStartInfo{FileName = "steam://rungameid/4285690", UseShellExecute = true});
        Process.GetCurrentProcess().Kill();
    }

    private async Task UpdateChecker(IModContext ctx)
    {
        ctx.Log.Info("Checking for updates...");

        var client = new HttpClient();
        var response = await client.GetStringAsync(
            "https://raw.githubusercontent.com/purple032/Mod-Handler-Airport-Security-Sucks-mod/refs/heads/main/ModHandlerCSharpProject/manifest.json");
        var version = JsonDocument.Parse(response).RootElement.GetProperty("version").GetString();
        var currentManifest = File.ReadAllText(Path.Combine(ctx.ModDirectory, "manifest.json"));
        var currentVersion = JsonDocument.Parse(currentManifest).RootElement.GetProperty("version").GetString();

        if (version == currentVersion)
        {
            ctx.Log.Info("ModHandler is up to date.");
            
            Process.Start(new ProcessStartInfo{FileName = "https://github.com/purple032/Mod-Handler-Airport-Security-Sucks-mod/blob/main/you_are_up_to_date.md", UseShellExecute = true});
        }
        else
        {
            ctx.Log.Info("ModHandler is out of date.");

            Process.Start(new ProcessStartInfo{FileName = "https://github.com/purple032/Mod-Handler-Airport-Security-Sucks-mod/blob/main/you_are_out_of_date.md", UseShellExecute = true});
        }
    }
}
