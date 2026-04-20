using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PSXRenderFeature : ScriptableRendererFeature
{
    class PSXRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTarget;
        private RTHandle source;

        public PSXRenderPass(Material material)
        {
            this.material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void SetSource(RTHandle source)
        {
            this.source = source;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTarget, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_PSXTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || source == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("PSX Post Process");

            Blitter.BlitCameraTexture(cmd, source, tempTarget, material, 0);
            Blitter.BlitCameraTexture(cmd, tempTarget, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
            tempTarget?.Release();
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material;
    }

    public Settings settings = new Settings();
    private PSXRenderPass pass;

    public override void Create()
    {
        pass = new PSXRenderPass(settings.material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        pass.SetSource(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}