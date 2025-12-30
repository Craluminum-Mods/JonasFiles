using Vintagestory.API.Common;

namespace JonasFiles.Configuration;

static class ModConfig
{
    private const string jsonConfig = "JonasFiles.json";

    public static JonasFilesConfig ReadConfig(ICoreAPI api)
    {
        JonasFilesConfig config;

        try
        {
            config = LoadConfig(api);

            if (config == null)
            {
                GenerateConfig(api);
                config = LoadConfig(api);
            }
            else
            {
                GenerateConfig(api, config);
            }
        }
        catch
        {
            GenerateConfig(api);
            config = LoadConfig(api);
        }
        return config;
    }

    public static void WriteConfig(ICoreAPI api, JonasFilesConfig config) => GenerateConfig(api, config);

    private static JonasFilesConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<JonasFilesConfig>(jsonConfig);
    }

    private static void GenerateConfig(ICoreAPI api)
    {
        api.StoreModConfig(new JonasFilesConfig(api), jsonConfig);
    }

    private static void GenerateConfig(ICoreAPI api, JonasFilesConfig previousConfig)
    {
        api.StoreModConfig(new JonasFilesConfig(api, previousConfig), jsonConfig);
    }
}