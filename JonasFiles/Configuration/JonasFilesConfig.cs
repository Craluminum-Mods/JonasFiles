using Vintagestory.API.Common;

namespace JonasFiles.Configuration;

public class JonasFilesConfig
{
    public bool RedactText { get; set; } = true;

    public bool RedactTextures { get; set; } = true;

    public double RedactionDensity { get; set; } = 0.75;

    public string[] WhitelistedPartialTexturePaths { get; set; } =
    [
        "seraph",
        "clothing",
        "skin/",
        "entity/humanoid",
        "bell-tobias",
        "entity/bell-tobias",
        "entity/bee",
        "entity/firefly",
        "entity/animal/bird",
        "entity/animal/fish",
        "entity/animal/invertebrate",
        "entity/animal/mammal/fox",
        "entity/animal/mammal/hare",
        "entity/animal/mammal/hooved",
        "entity/animal/mammal/raccoon"
    ];

    public JonasFilesConfig(ICoreAPI api, JonasFilesConfig? previousConfig = null)
    {
        if (previousConfig == null) return;

        RedactText = previousConfig.RedactText;
        RedactTextures = previousConfig.RedactTextures;
        RedactionDensity = previousConfig.RedactionDensity;
        WhitelistedPartialTexturePaths = [.. WhitelistedPartialTexturePaths];
    }
}