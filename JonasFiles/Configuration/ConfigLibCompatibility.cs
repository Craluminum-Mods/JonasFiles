using ConfigLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace JonasFiles.Configuration;

public class ConfigLibCompatibility
{
    private static string SettingPrefix => "jonasfiles:Config.Setting.";
    private static string WarningReloadWorld => "jonasfiles:Config.Warning.ReloadWorld";
    private readonly ICoreAPI api;

    public ConfigLibCompatibility(ICoreAPI api)
    {
        this.api = api;
        Init();
    }

    private void Init()
    {
        api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig("jonasfiles", EditConfig);
    }

    private void EditConfig(string id, ControlButtons buttons)
    {
        if (buttons.Save)
        {
            ModConfig.WriteConfig(api, Core.Config);
            (api as ICoreClientAPI)?.TriggerChatMessage(".reload textures");
        }

        if (buttons.Restore) Core.Config = ModConfig.ReadConfig(api);
        if (buttons.Defaults) Core.Config = new JonasFilesConfig(api);
        Edit(Core.Config, id);
    }

    private void Edit(JonasFilesConfig config, string id)
    {
        config.RedactText = OnCheckBox(id, config.RedactText, nameof(config.RedactText));
        config.RedactionDensity = OnInputDouble(id, config.RedactionDensity, nameof(config.RedactionDensity));
        ImGui.NewLine();
        ImGui.TextWrapped(Lang.Get(WarningReloadWorld));
        config.RedactTextures = OnCheckBox(id, config.RedactTextures, nameof(config.RedactTextures));
        ImGui.Separator();
        ImGui.NewLine();
        ImGui.TextWrapped(Lang.Get(WarningReloadWorld));
        config.WhitelistedPartialTexturePaths = OnInputTextMultiline(id, config.WhitelistedPartialTexturePaths, nameof(config.WhitelistedPartialTexturePaths)).ToArray();
    }

    private bool OnCheckBox(string id, bool value, string name)
    {
        bool newValue = value;
        ImGui.Checkbox(Lang.Get(SettingPrefix + name) + $"##{name}-{id}", ref newValue);
        return newValue;
    }

    private IEnumerable<string> OnInputTextMultiline(string id, IEnumerable<string> values, string name)
    {
        string newValue = values.Any() ? values.Aggregate((first, second) => $"{first}\n{second}") : "";
        ImGui.InputTextMultiline(Lang.Get(SettingPrefix + name) + $"##{name}-{id}", ref newValue, 4096, new(0, 0));
        return newValue.Split('\n', StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
    }

    private double OnInputDouble(string id, double value, string name, double minValue = default)
    {
        double newValue = value;
        ImGui.InputDouble(Lang.Get(SettingPrefix + name) + $"##{name}-{id}", ref newValue, step: 0.01, step_fast: 0.1);
        return newValue < minValue ? minValue : newValue;
    }
}