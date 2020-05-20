using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core.Easing;

public class Test : MonoBehaviour
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
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime;
            curveScroller.DoScroll(mouseX); 
        }
        if (Input.GetMouseButtonUp(0))
        {
            curveScroller.DoScrollEnd();
        }
    }

}
