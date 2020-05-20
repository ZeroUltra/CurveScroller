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
    //[Space(10)]
    [Header("fixedToBasePos and useInertia choose one!!!")]
    [Tooltip("是否固定到起始点")]
    [SerializeField] bool fixedToBasePos;
    [Range(0, 1f)]
    [Tooltip("固定滑动时间")]
    [SerializeField] float fixedDuartion = 0.4f;

    [Tooltip("是否使用惯性")]
    [SerializeField] bool useInertia;
    [Range(0, 1f)]
    [Tooltip("惯性值")]
    [SerializeField] float inertiaRate = 0.135f;

    [Space(10)]
    [SerializeField] DOTweenPath tweenPath;
    [SerializeField] CurveItemBase curveItemPrefab;
    #endregion

    #region Private
    private bool isInit = false;
    private float[] basePercentages;  //初始百分比 用于还原
    private SmallList<CurveItemData> listData = new SmallList<CurveItemData>();  //存放数据
    private List<TransPercentage> listTransPerc = new List<TransPercentage>();  //记录trans 和 百分比

    //------------------
    private float tempOffv = 0;  //用来缓存最后一个滑动值
    private Tweener inertiaTweenr;
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="curveItems"></param>
    public void Init(CurveItemData[] curveItems)
    {
        if (isInit) return;
        if (tweenPath.path.wpLengths.Length <= 1)
        {
            Debug.LogError("tween Path 没有点位信息");
            return;
        }

        isInit = true;
        //获取path总长度
        float pathLength = tweenPath.tween.PathLength();
        //获取每个点的长度 第一个点为0 排除
        float[] needwplengths = new float[tweenPath.path.wpLengths.Length - 1];
        for (int i = 0; i < tweenPath.path.wpLengths.Length; i++)
        {
            //第一个永远是0 不需要
            if (i == 0) continue;
            needwplengths[i - 1] = tweenPath.path.wpLengths[i];
        }

        //添加数据 
        //数据过少 就循环添加
        if (curveItems.Length < needwplengths.Length)
        {
            int arrayIndex = 0;
            //多添加一个
            for (int i = 0; i <= needwplengths.Length; i++)
            {
                listData.Add(curveItems[arrayIndex]);
                arrayIndex++;
                if (arrayIndex > curveItems.Length - 1) arrayIndex = 0;
            }
        }
        else
        {
            for (int i = 0; i < curveItems.Length; i++)
                listData.Add(curveItems[i]);
        }

        //存放百分比
        basePercentages = new float[needwplengths.Length];
        float tempPercentage = 0;
        //遍历 i=1 第一个永远是0
        for (int i = 0; i < needwplengths.Length; i++)
        {
            tempPercentage += needwplengths[i] / pathLength;
            basePercentages[i] = tempPercentage;

            //克隆对象
            CurveItemBase cutveItem = Instantiate(curveItemPrefab.gameObject).GetComponent<CurveItemBase>();
            cutveItem.transform.transform.SetParent(this.transform);

            CurveItemData curveItemData = listData.RemoveStart();
            cutveItem.SetData(curveItemData);
            SetTransPosForTweenPerc(cutveItem.transform, tempPercentage);
            //添加到列表
            listTransPerc.Add(new TransPercentage(cutveItem, tempPercentage, curveItemData));
        }
        listTransPerc.Capacity = listTransPerc.Count;
    }

    /// <summary>
    /// 做滑动
    /// </summary>
    /// <param name="offv">滑动值</param>
    public void DoScroll(float offv)
    {
        if (isInit == false) Debug.LogError("No Init!!! please Init() first");
        OnScrollChange(offv, false);
    }
    /// <summary>
    /// 滑动结束
    /// </summary>
    public void DoScrollEnd()
    {
        if (fixedToBasePos)
        {
            OnEndFixed();
        }
        else if (useInertia)
        {
            OnendInertia();
        }
    }

    /// <summary>
    /// 结束惯性滑动
    /// </summary>
    private void OnendInertia()
    {
        float rate = tempOffv * inertiaRate * 10;
        inertiaTweenr = DOTween.To(() => tempOffv, x => tempOffv = x, 0, rate).SetSpeedBased(true)
            .OnUpdate(() => OnScrollChange(tempOffv, true))
            .OnKill(() => { inertiaTweenr = null; });
    }
    /// <summary>
    /// 结束吸附
    /// </summary>
    private void OnEndFixed()
    {
        //排序 //按照百分比大小排序
        listTransPerc.Sort(new TransPercentage());
        for (int i = 0; i < listTransPerc.Count; i++)
        {
            int index = i;
            TransPercentage itemTuple = listTransPerc[i];
            DOTween.To(() => itemTuple.percentage, x => itemTuple.percentage = x, basePercentages[index], fixedDuartion).OnUpdate(() =>
            {
                SetTransPosForTweenPerc(listTransPerc[index].cueveItem.transform, listTransPerc[index].percentage);
                listTransPerc[index] = itemTuple;
            });
        }
    }


    private void OnScrollChange(float offv, bool endAction)
    {
        //如果不是结束动作 当拖动的时候 应该关掉惯性tweenr
        if (endAction == false)
        {
            if (inertiaTweenr != null && inertiaTweenr.IsPlaying())
                inertiaTweenr.Kill();
        }
        tempOffv = offv;
        for (int i = 0; i < listTransPerc.Count; i++)
        {
            TransPercentage item = listTransPerc[i];
            item.percentage += offv;
            if (item.percentage > 1f)
            {
                float offset = item.percentage - 1f;
                item.percentage = 0f + offset;
                item.cueveItem.transform.SetAsFirstSibling();

                //往下滑动 取出最后一个然后把当前的插入到开头
                CurveItemData data = listData.RemoveEnd();
                listData.AddStart(item.curveItemData);
                item.SetData(data);
            }
            else if (item.percentage < 0f)
            {
                float offset = 0f - item.percentage;
                item.percentage = 1f - offset;
                item.cueveItem.transform.SetAsLastSibling();

                //往上滑动 取出第一个然后把当前的插到最后一个
                CurveItemData data = listData.RemoveStart();
                listData.Add(item.curveItemData);
                item.SetData(data);
            }
            listTransPerc[i] = item;
            SetTransPosForTweenPerc(listTransPerc[i].cueveItem.transform, listTransPerc[i].percentage);
        }
    }

    private void SetTransPosForTweenPerc(Transform trans, float percentage)
    {
        trans.position = tweenPath.tween.PathGetPoint(percentage);
    }

    private struct TransPercentage : IComparer<TransPercentage>
    {
        /// <summary>
        /// 百分比
        /// </summary>
        public float percentage;
        public CurveItemBase cueveItem;
        public CurveItemData curveItemData;

        public TransPercentage(CurveItemBase _cueveItem, float _percentage, CurveItemData _curveItemData)
        {
            this.percentage = _percentage;
            this.cueveItem = _cueveItem;
            this.curveItemData = _curveItemData;
        }

        public void SetData(CurveItemData _curveItemData)
        {
            this.curveItemData = _curveItemData;
            cueveItem.SetData(_curveItemData);
        }
        public int Compare(TransPercentage x, TransPercentage y)
        {
            if (x.percentage > y.percentage) return 1;
            else if (x.percentage < y.percentage) return -1;
            else return 0;
        }
    }
}
