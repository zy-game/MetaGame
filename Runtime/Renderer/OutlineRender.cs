using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime
{
    public class OutlineRender : ScriptableRendererFeature
    {
        public Material previewMat;
        private OutlineRenderPass rendererPass;
        public LayerMask layerMask;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(rendererPass);
           
        }

        public override void Create()
        {
            rendererPass = new OutlineRenderPass(previewMat, layerMask);
            rendererPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }
    }
}