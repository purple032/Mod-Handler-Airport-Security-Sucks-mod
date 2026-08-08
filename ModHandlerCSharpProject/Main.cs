using ASL.Api;
using System.IO;
using System.Diagnostics;

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
        if (mods.Length > 0)
        {
            foreach (string modPath in mods)
            {
                if (Path.GetFileName(modPath) != Path.GetFileName(modFolder))
                {
                    ctx.Menu.AddToggle(Path.GetFileName(modPath), true, state => HandleMod(modPath, state, modsDisabledFolder, modsFolder, ctx));
                }
            }
        }
        

        string[] disabledMods = Directory.GetDirectories(modsDisabledFolder);
        if (disabledMods.Length > 0)
        {
            foreach (string modPath in disabledMods)
            {
                ctx.Menu.AddToggle(Path.GetFileName(modPath), false, state => HandleMod(modPath, state, modsDisabledFolder, modsFolder, ctx));
            }
        }
        
        ctx.Menu.AddLabel($"Disabled mods are stored in the modsDisabled folder. (path: {modsDisabledFolder})");
        ctx.Menu.AddButton("Close game", () => Process.GetCurrentProcess().Kill());
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
}