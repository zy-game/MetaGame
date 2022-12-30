using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime
{
    public class PreviewRenderer : ScriptableRendererFeature
    {
        public Material previewMat;
        private PreviewRendererrPass rendererPass;

        public override void Create()
        {
            rendererPass = new PreviewRendererrPass(previewMat);
            rendererPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(rendererPass);
        }

    }
}
