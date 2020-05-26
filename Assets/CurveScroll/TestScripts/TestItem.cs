using UnityEngine;
using UnityEngine.UI;
using ZeroUltra.CurveScroller;

public class TestItem : CurveScrollerItemComponent
{
    public Text text;
    public override void SetData(CurveScrollerItemData t)
    {
        text.text = (t as TestData).ID.ToString();
    }
}
