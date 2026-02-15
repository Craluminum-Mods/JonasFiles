using Cairo;
using HarmonyLib;
using JonasFiles.Configuration;
using System;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace JonasFiles;

public class Core : ModSystem
{
    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);
    private DesaturationRenderer desatRenderer;

#nullable disable
    public static JonasFilesConfig Config { get; set; }
#nullable enable

    public override bool ShouldLoad(EnumAppSide forSide) => forSide.IsClient();

    public override void StartPre(ICoreAPI api)
    {
        HarmonyInstance.PatchAll();

        Config = ModConfig.ReadConfig(api);

        if (api.ModLoader.IsModEnabled("configlib"))
        {
            new ConfigLibCompatibility(api);
        }
    }

    public override void Dispose()
    {
        HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
        desatRenderer?.Dispose();
        desatRenderer = null;
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        desatRenderer = new DesaturationRenderer(api);
        desatRenderer.Register();
    }
}

[HarmonyPatch(typeof(TextDrawUtil), nameof(TextDrawUtil.Lineize), [ typeof(Context), typeof(string), typeof(EnumLinebreakBehavior), typeof(TextFlowPath[]), typeof(double), typeof(double), typeof(double), typeof(bool)])]
public static class TextDrawUtil_Lineize_Patch
{
    [HarmonyPrefix]
    public static void Prefix(ref string text)
    {
        if (!Core.Config.RedactText) return;
        text = TextCensorer.Process(text);
    }
}

[HarmonyPatch(typeof(TextDrawUtil), nameof(TextDrawUtil.DrawTextLine))]
public static class TextDrawUtil_DrawTextLine_Patch
{
    [HarmonyPrefix]
    public static void Prefix(ref string text)
    {
        if (!Core.Config.RedactText) return;
        text = TextCensorer.Process(text);
    }
}


public static class TextCensorer
{
    private const char RedactionSymbol = '█'; // '\u2588'

    public static string Process(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return Regex.Replace(input, @"\w+", m =>
        {
            string word = m.Value;
            Random rng = new Random(word.ToLower().GetHashCode());

            if (rng.NextDouble() < Core.Config.RedactionDensity)
            {
                return new string(RedactionSymbol, word.Length);
            }

            return word;
        });
    }
}