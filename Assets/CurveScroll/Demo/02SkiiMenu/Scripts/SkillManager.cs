using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ZeroUltra.CurveScroller;

public class SkillManager : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public CurveScroller curveScroller;
    public Sprite[] sps;

    private void Start()
    {
        SkiiItemData[] skiiItemDatas = new SkiiItemData[sps.Length];
        for (int i = 0; i < skiiItemDatas.Length; i++)
        {
            skiiItemDatas[i] = new SkiiItemData(sps[i]);
        }
        curveScroller.Init(skiiItemDatas);
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
