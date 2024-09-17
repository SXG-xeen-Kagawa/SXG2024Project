using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPassFeature : ScriptableRendererFeature
{
    class OutlineRenderPass : ScriptableRenderPass
    {
        private readonly string m_passName = nameof(OutlineRenderPass);
        private readonly Material m_material;

        RTHandle m_cameraColorTarget;

        public List<(Color outlineColor, Renderer[] renderers)> targets { get; set; } = new();

        public OutlineRenderPass(Material material)
        {
            m_material = material;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        public void SetTarget(RTHandle colorHandle)
        {
            m_cameraColorTarget = colorHandle;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_cameraColorTarget);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (targets == null || targets.Count == 0)
                return;

            var camData = renderingData.cameraData;
            var cmd = CommandBufferPool.Get(m_passName);

            using (new ProfilingScope(cmd, new ProfilingSampler(m_passName)))
            {
                var texId = Shader.PropertyToID("_CameraOpaqueTexture");
                var w = camData.camera.scaledPixelWidth;
                var h = camData.camera.scaledPixelHeight;

                cmd.GetTemporaryRT(texId, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
                cmd.SetRenderTarget(texId);
                cmd.ClearRenderTarget(false, true, Color.clear);

                foreach (var target in targets)
                {
                    if (target.renderers == null || target.renderers.Length == 0)
                        continue;

                    // アウトラインをつけたいオブジェクトを描画
                    foreach (var renderer in target.renderers)
                    {
                        if (renderer == null)
                            continue;

                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            cmd.DrawRenderer(renderer, renderer.sharedMaterials[i], i, 0);
                        }
                    }
                }

                var handle = camData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, handle, m_cameraColorTarget, m_material, 0);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // アウトラインカラー
                //m_material.SetColor("_OutlineColor", target.outlineColor);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    private OutlineRenderPass m_scriptablePass;

    [SerializeField]
    private Material m_material;

    public void SetRenderer(Color outlineColor, Renderer[] renderers)
    {
        m_scriptablePass.targets.Add(new(outlineColor, renderers));
    }

    /// <inheritdoc/>
    public override void Create()
    {
        m_scriptablePass = new OutlineRenderPass(m_material);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_scriptablePass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            m_scriptablePass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_scriptablePass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }
}


