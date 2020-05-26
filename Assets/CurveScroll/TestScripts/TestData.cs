using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroUltra.CurveScroller;

public class TestData : CurveScrollerItemData
{

    public int ID { get; set; }

    public TestData(int _id)
    {
        ID = _id;
    }

}
