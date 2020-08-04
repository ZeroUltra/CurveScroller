using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ZeroUltra.CurveScroller;

public class TestVertical : MonoBehaviour, IDragHandler, IEndDragHandler
{

    public CurveScroller curveScroller;


    private void Start()
    {
        int nums = 20;
        TestData[] curveItem = new TestData[nums];
        for (int i = 0; i < nums; i++)
        {
            curveItem[i] = new TestData(i);
        }
        curveScroller.Init(curveItem);

    }


    public void OnDrag(PointerEventData eventData)
    {
        curveScroller.DoScroll(eventData.delta.y * 0.001f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        curveScroller.DoScrollEnd();
    }


}
