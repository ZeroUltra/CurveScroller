using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestItem : CurveItemBase
{
    public Text text;
    public override void SetData(CurveItemData t)
    {
        text.text = (t as TestData).ID.ToString();
    }
}
