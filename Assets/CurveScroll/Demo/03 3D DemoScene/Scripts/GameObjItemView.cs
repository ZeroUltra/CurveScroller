using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroUltra.CurveScroller;

public class GameObjItemView : CurveScrollerItemView
{

    public override void SetData(CurveScrollerItemData t)
    {
        if (this.transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        GameObjItemData itemData = t as GameObjItemData;
        GameObject go = GameObject.Instantiate(itemData.itemPrefab);
        go.transform.SetParent(this.transform, false);
        go.transform.localPosition = Vector3.zero;
    }

}
