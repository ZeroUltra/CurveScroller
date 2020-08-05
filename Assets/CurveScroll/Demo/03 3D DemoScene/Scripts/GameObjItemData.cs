using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroUltra.CurveScroller;
public class GameObjItemData : CurveScrollerItemData
{

    public GameObject itemPrefab;
    public GameObjItemData(GameObject _itemPrefab)
    {
        this.itemPrefab = _itemPrefab;
    }
}
