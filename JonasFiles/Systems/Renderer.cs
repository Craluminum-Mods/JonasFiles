using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace JonasFiles;

public class DesaturationRenderer : IRenderer
{
    private readonly ICoreClientAPI capi;
    private IShaderProgram shader;
    private MeshRef meshRef;

    public double RenderOrder => 0.89;
    public int RenderRange => 1;

    public float DesaturationStrength { get; set; } = 1f;

    public DesaturationRenderer(ICoreClientAPI capi)
    {
        this.capi = capi;

        MeshData triangleMesh = CreateFullscreenTriangle();
        meshRef = capi.Render.UploadMesh(triangleMesh);

        if (!LoadShader())
        {
            return;
        }
    }

    // we use an oversized triangle to cover the entire screen instead of a quad which gives issues on NVIDIA. blame jensen huang
    private MeshData CreateFullscreenTriangle()
    {
        MeshData mesh = new MeshData(3, 1, false, false, true, false);

        mesh.xyz = new float[3 * 3];
        mesh.Uv = new float[3 * 2];
        mesh.Indices = new int[3];
        mesh.IndicesCount = 3;
        mesh.VerticesCount = 3;

        mesh.xyz[0] = -1f; // x
        mesh.xyz[1] = -1f; // y
        mesh.xyz[2] = 0f;  // z
        mesh.Uv[0] = 0f;
        mesh.Uv[1] = 0f;

        mesh.xyz[3] = -1f; // x
        mesh.xyz[4] = 3f;  // y
        mesh.xyz[5] = 0f;  // z
        mesh.Uv[2] = 0f;
        mesh.Uv[3] = 2f;

        mesh.xyz[6] = 3f;  // x
        mesh.xyz[7] = -1f; // y
        mesh.xyz[8] = 0f;  // z
        mesh.Uv[4] = 2f;
        mesh.Uv[5] = 0f;

        mesh.Indices[0] = 0;
        mesh.Indices[1] = 1;
        mesh.Indices[2] = 2;

        return mesh;
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        if (meshRef == null || shader == null) return;

        try
        {
            IShaderProgram prevShader = capi.Render.CurrentActiveShader;
            prevShader?.Stop();

            shader.Use();

            shader.BindTexture2D("primaryScene", capi.Render.FrameBuffers[(int)EnumFrameBuffer.Primary].ColorTextureIds[0], 0);
            shader.BindTexture2D("depthTex", capi.Render.FrameBuffers[(int)EnumFrameBuffer.Primary].DepthTextureId, 1);

            shader.Uniform("desaturationStrength", DesaturationStrength);

            capi.Render.GLDisableDepthTest();
            capi.Render.GlToggleBlend(false);

            capi.Render.RenderMesh(meshRef);

            capi.Render.GlToggleBlend(true);
            capi.Render.GLEnableDepthTest();

            shader.Stop();

            prevShader?.Use();
        }
        catch (Exception ex)
        {
            capi.Logger.Error($"Render error: {ex.Message}");
        }
    }

    public bool LoadShader()
    {
        try
        {
            shader = capi.Shader.NewShaderProgram();

            var vertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
            var fragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

            IAsset vertAsset = capi.Assets.TryGet(new AssetLocation("jonasfiles:shaders/desat.vsh"));
            IAsset fragAsset = capi.Assets.TryGet(new AssetLocation("jonasfiles:shaders/desat.fsh"));

            if (vertAsset == null || fragAsset == null)
            {
                capi.Logger.Error("Could not find shader files");
                return false;
            }

            vertexShader.Code = vertAsset.ToText();
            fragmentShader.Code = fragAsset.ToText();

            shader.VertexShader = vertexShader;
            shader.FragmentShader = fragmentShader;

            capi.Shader.RegisterMemoryShaderProgram("jfdesaturation", shader);

            if (!shader.Compile())
            {
                capi.Logger.Error("Shader compilation failed");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            capi.Logger.Error($"Error loading shader: {ex.Message}");
            return false;
        }
    }

    public void Register()
    {
        if (shader == null)
        {
            capi.Logger.Error("Failed to load shader");
            return;
        }

        capi.Event.ReloadShader += LoadShader;
        capi.Event.RegisterRenderer(this, EnumRenderStage.AfterBlit);
    }

    public void Dispose()
    {
        shader?.Dispose();
        shader = null;

        if (meshRef != null)
        {
            capi.Render.DeleteMesh(meshRef);
            meshRef = null;
        }

        capi.Event.ReloadShader -= LoadShader;
        capi.Event.UnregisterRenderer(this, EnumRenderStage.AfterBlit);
    }
}