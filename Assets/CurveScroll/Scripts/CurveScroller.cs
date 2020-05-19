using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 曲线滑动器
/// </summary>
public class CurveScroller : MonoBehaviour
{
    #region SerializeField
    [SerializeField] DOTweenPath tweenPath;
    [SerializeField] CurveItemBase curveItemPrefab;

    #endregion

    #region Private
    private float[] percentages;

    #endregion

    void Start()
    {
        #region 计算tweenpath自瞄点的百分比
        //获取path总长度
        float pathLength = tweenPath.tween.PathLength();
        //获取每个点的长度 第一个点为0 
        float[] wplengths = tweenPath.path.wpLengths;
        //存放百分比
        percentages = new float[wplengths.Length - 1];
        float tempPercentage = 0;
        //遍历 i=1 第一个永远是0
        for (int i = 1; i < wplengths.Length; i++)
        {
            tempPercentage += wplengths[i] / pathLength;
            percentages[i] = tempPercentage;
            //生成点位
            GameObject curgo = GameObject.Instantiate(curveItemPrefab.gameObject);
            curgo.transform.SetParent(this.transform);
            SetTransPosForTweenPerc(curgo.transform, tempPercentage);
        }
        #endregion

    }

    private void SetTransPosForTweenPerc(Transform trans,float percentage)
    {
        trans.position= tweenPath.tween.PathGetPoint(percentage);
    }
}
