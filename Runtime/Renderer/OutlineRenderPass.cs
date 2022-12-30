using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime
{
    public class OutlineRenderPass : ScriptableRenderPass
    {
        private Material mat;
        FilteringSettings filterSetting;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        RenderStateBlock renderStateBlock;

        public OutlineRenderPass(Material mat,LayerMask mask)
        {
            this.mat = mat;

            filterSetting = new FilteringSettings(RenderQueueRange.all, mask);

            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));

            renderStateBlock = new RenderStateBlock(RenderStateMask.Depth);
            renderStateBlock.mask |= RenderStateMask.Depth;
            renderStateBlock.depthState = new DepthState(true, CompareFunction.Greater);

        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);


            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSetting, ref renderStateBlock);
            drawingSettings.overrideMaterial = mat;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSetting);
            drawingSettings.overrideMaterial = null;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSetting);
        }
    }
}