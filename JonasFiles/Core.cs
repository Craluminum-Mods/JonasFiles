using Cairo;
using HarmonyLib;
using System;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace JonasFiles;

public class Core : ModSystem
{
    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);
    public override bool ShouldLoad(EnumAppSide forSide) => forSide.IsClient();
    public override void StartPre(ICoreAPI api) => HarmonyInstance.PatchAll();
    public override void Dispose() => HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
}

[HarmonyPatch(typeof(TextDrawUtil), nameof(TextDrawUtil.Lineize), [ typeof(Context), typeof(string), typeof(EnumLinebreakBehavior), typeof(TextFlowPath[]), typeof(double), typeof(double), typeof(double), typeof(bool)])]
public static class TextDrawUtil_Lineize_Patch
{
    [HarmonyPrefix]
    public static void Prefix(ref string text) => text = TextCensorer.Process(text);
}

[HarmonyPatch(typeof(TextDrawUtil), nameof(TextDrawUtil.DrawTextLine))]
public static class TextDrawUtil_DrawTextLine_Patch
{
    [HarmonyPrefix]
    public static void Prefix(ref string text) => text = TextCensorer.Process(text);
}

[HarmonyPatch(typeof(TextureAtlasManager), nameof(TextureAtlasManager.ToTextureAssetLocation))]
public static class TextureAtlasManager_ToTextureAssetLocation_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ref AssetLocationAndSource __result, AssetLocationAndSource loc)
    {
        if (!ShouldCensor(loc)) return true;

        AssetLocationAndSource assetLocationAndSource = new AssetLocationAndSource("game", "textures/unknown", loc.Source);
        assetLocationAndSource.Path = assetLocationAndSource.Path.Replace("@90", "").Replace("@180", "").Replace("@270", "");
        assetLocationAndSource.Path = Regex.Replace(assetLocationAndSource.Path, "å\\d+", "");
        assetLocationAndSource.WithPathAppendixOnce(".png");
        __result = assetLocationAndSource;
        return false;
    }

    private static bool ShouldCensor(AssetLocationAndSource loc)
    {
        string location = loc.ToString();

        if (location.Contains("seraph")) return false;
        if (location.Contains("clothing")) return false;
        if (location.Contains("skin/")) return false;

        if (location.Contains("entity/"))
        {
            if (location.Contains("entity/humanoid")) return false;
            if (location.Contains("entity/lore")) return !location.Contains("bell-tobias");
            if (location.Contains("entity/bell-tobias")) return false;
            if (location.Contains("entity/bee")) return false;
            if (location.Contains("entity/firefly")) return false;
            if (location.Contains("entity/animal/bird")) return false;
            if (location.Contains("entity/animal/fish")) return false;
            if (location.Contains("entity/animal/invertebrate")) return false;
            if (location.Contains("entity/animal/mammal/fox")) return false;
            if (location.Contains("entity/animal/mammal/hare")) return false;
            if (location.Contains("entity/animal/mammal/hooved")) return false;
            if (location.Contains("entity/animal/mammal/racoon")) return false;
        }
        return true;
    }
}

public static class TextCensorer
{
    public static string Process(string input)
    {
        double censorChance = 0.75;
        if (string.IsNullOrEmpty(input)) return input;

        return Regex.Replace(input, @"\w+", m =>
        {
            string word = m.Value;
            Random rng = new Random(word.ToLower().GetHashCode());

            if (rng.NextDouble() < censorChance)
            {
                return new string('\u2588', word.Length);
            }

            return word;
        });
    }
}