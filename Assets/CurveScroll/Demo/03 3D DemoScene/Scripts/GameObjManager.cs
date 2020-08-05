using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroUltra.CurveScroller;

public class GameObjManager : MonoBehaviour
{
    public CurveScroller curveScroller;
    public GameObject[] prefabs;

    private void Start()
    {
        GameObjItemData[] curveItem = new GameObjItemData[prefabs.Length];
        for (int i = 0; i < curveItem.Length; i++)
        {
            curveItem[i] = new GameObjItemData(prefabs[i]);
        }
        curveScroller.Init(curveItem);
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
           
            curveScroller.DoScroll(Input.GetAxis("Mouse X")*0.1f);
        }
        if (Input.GetMouseButtonUp(0))
        {
            curveScroller.DoScrollEnd();
        }
    }
}
