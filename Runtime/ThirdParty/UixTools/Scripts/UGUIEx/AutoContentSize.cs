
using UnityEngine;
using UnityEngine.UI;

public class AutoContentSize : ContentSizeFitter
{
    public Vector2 startSize;
    public RectTransform target; 
    public bool limit= false;
    public Vector2 limitSize = Vector2.one * 100;
     
    private RectTransform mRt;
    private RectTransform Rt 
    {
        get
        {
            if (!mRt) mRt = GetComponent<RectTransform>();
            return mRt;
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetImageSize();
    }

    public void SetImageSize()
    {
        if (!target)
            return;

        Vector2 size = startSize + Rt.sizeDelta;

        target.sizeDelta = size;
        if (!limit)
            return;
        SetLimit(size);
        
    }

    private void SetLimit(Vector2 size)
    {
        if (limitSize.x > 0)
        {
            if (size.x >= limitSize.x)
            {
                if (horizontalFit == FitMode.PreferredSize)
                {
                    horizontalFit = FitMode.Unconstrained;
                    OnRectTransformDimensionsChange();
                    Rt.sizeDelta = new Vector2(limitSize.x,size.y);                    
                }

            }
        }
    }

}