
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime
{

    public class PreviewRendererrPass : ScriptableRenderPass
    {
        private Material previewMat;
        FilteringSettings filterSetting;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        RenderStateBlock renderStateBlock;

        public PreviewRendererrPass(Material previewMat)
        {
            this.previewMat = previewMat;

            filterSetting = new FilteringSettings(RenderQueueRange.all, LayerMask.GetMask("Preview"));

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
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSetting);
            drawingSettings.overrideMaterial = previewMat;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSetting);
        }

    }
}