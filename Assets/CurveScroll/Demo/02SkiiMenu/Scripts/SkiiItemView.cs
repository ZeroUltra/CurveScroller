using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroUltra.CurveScroller;
public class SkiiItemView : CurveScrollerItemView
{
    public Image img;
    public override void SetData(CurveScrollerItemData t)
    {
        img.sprite = (t as SkiiItemData).sprite;
    }
    
}
