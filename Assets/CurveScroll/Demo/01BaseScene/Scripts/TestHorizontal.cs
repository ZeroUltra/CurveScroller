using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZeroUltra.CurveScroller;
public class TestHorizontal : MonoBehaviour,IDragHandler,IEndDragHandler
{
    public CurveScroller curveScroller;


    private void Start()
    {
        int nums = 9;
        TestData[] curveItem = new TestData[nums];
        for (int i = 0; i < nums; i++)
        {
            curveItem[i] = new TestData(i);
        }
        curveScroller.Init(curveItem);
           
    }
  

    public void OnDrag(PointerEventData eventData)
    {
        curveScroller.DoScroll(eventData.delta.x*0.001f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        curveScroller.DoScrollEnd();
    }
}
