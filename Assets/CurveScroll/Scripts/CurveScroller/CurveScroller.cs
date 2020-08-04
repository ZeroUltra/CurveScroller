using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace ZeroUltra.CurveScroller
{
    /// <summary>
    /// 曲线滑动器
    /// </summary>
    public class CurveScroller : MonoBehaviour
    {
        //事件
        public event System.Action OnHeadChanged;
        public event System.Action OnTailChanged;

        #region SerializeField
        [Tooltip("是否使用惯性")]
        [SerializeField] bool useInertia;
        [Tooltip("是否固定到起始点")]
        [SerializeField] bool useFixed;

        [SerializeField] DOTweenPath tweenPath;
        [SerializeField] CurveScrollerItemView curveItemPrefab;
        #endregion

        #region Private
        private bool isInit = false;
        private Coroutine endCoroutine;
        private float[] basePercentages;  //初始百分比 用于还原
        private SmallList<CurveScrollerItemData> listData = new SmallList<CurveScrollerItemData>();  //存放数据
        private List<TransPercentageStruct> listTransPerc = new List<TransPercentageStruct>();  //记录trans 和 百分比

        //------------------
        private float endOffValue;
        private bool isDragEnd = false;
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="curveItems"></param>
        public void Init(CurveScrollerItemData[] curveItems)
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

            //计算比例
            basePercentages = new float[needwplengths.Length];
            float tempPercentage = 0;

            for (int i = 0; i < needwplengths.Length; i++)
            {
                tempPercentage += needwplengths[i] / pathLength;
                basePercentages[i] = tempPercentage;
            }

            for (int i = 0; i < basePercentages.Length; i++)
            {
                //克隆对象
                CurveScrollerItemView cutveItem = Instantiate(curveItemPrefab.gameObject).GetComponent<CurveScrollerItemView>();
                cutveItem.transform.transform.SetParent(this.transform);
                //cutveItem.transform.localScale = Vector3.one;

                CurveScrollerItemData curveItemData = listData.RemoveStart();
                curveItemData.Percentage = basePercentages[i];
                cutveItem.SetData(curveItemData);
                SetTransPosForTweenPerc(cutveItem.transform, basePercentages[i]);

                //添加到列表
                listTransPerc.Add(new TransPercentageStruct(cutveItem, curveItemData));
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
            isDragEnd = false;
            this.endOffValue = offv;
            if (endCoroutine != null)
            {
                StopCoroutine(endCoroutine);
                endCoroutine = null;
            }
            OnScrollChange(offv);
        }

        /// <summary>
        /// 滑动结束
        /// </summary>
        public void DoScrollEnd()
        {
            isDragEnd = true;
        }

        private void OnScrollChange(float _Offsetpercentage)
        {
            for (int i = 0; i < listTransPerc.Count; i++)
            {
                int index = i;
                TransPercentageStruct itemStruct = listTransPerc[i];
                itemStruct.curveItemData.Percentage += _Offsetpercentage;
                if (itemStruct.curveItemData.Percentage >= 1f)
                {
                    itemStruct.curveItemView.transform.SetAsFirstSibling();

                    // 取出最后一个然后把当前的插入到开头
                    CurveScrollerItemData data = listData.RemoveEnd();
                    float offset = itemStruct.curveItemData.Percentage - 1f;
                    data.Percentage = 0 + offset;

                    listData.AddStart(itemStruct.curveItemData);
                    itemStruct.SetData(data);
                    OnTailChanged?.Invoke();
                }
                else if (itemStruct.curveItemData.Percentage <= 0f)
                {
                    itemStruct.curveItemView.transform.SetAsLastSibling();

                    //往上滑动 取出第一个然后把当前的插到最后一个
                    CurveScrollerItemData data = listData.RemoveStart();
                    listData.Add(itemStruct.curveItemData);

                    float offset = 0f - itemStruct.curveItemData.Percentage;
                    data.Percentage = 1f - offset;
                    itemStruct.SetData(data);
                    OnHeadChanged?.Invoke();
                }
                listTransPerc[index] = itemStruct;
                SetTransPosForTweenPerc(listTransPerc[index].curveItemView.transform, listTransPerc[index].curveItemData.Percentage);
            }
        }

        private void SetTransPosForTweenPerc(Transform trans, float percentage)
        {
            trans.position = tweenPath.tween.PathGetPoint(percentage);
        }

        private void LateUpdate()
        {
            if (isDragEnd)
            {
                if (useInertia && Abs(endOffValue) >0)
                {
                    endOffValue = Mathf.Lerp(endOffValue, 0f, 5 * Time.unscaledDeltaTime);
                    OnScrollChange(this.endOffValue);
                    if (!useFixed && Abs(endOffValue) - 0 < 0.0001f)
                    {
                        endOffValue = 0f;
                        isDragEnd = false;
                    }
                    else if (useFixed && Abs(endOffValue) - 0 < 0.01f)
                    {
                        endOffValue = 0f;
                        endCoroutine = StartCoroutine(IEDoEnd());
                    }
                }
                if (!useInertia && useFixed)
                {
                    endCoroutine = StartCoroutine(IEDoEnd());
                }
            }
        }
        private IEnumerator IEDoEnd()
        {
            listTransPerc.Sort(new TransPercentageStruct());
            //判断第一个是否过半 过半 就直接到目标区域
            if (listTransPerc[0].curveItemData.Percentage >= basePercentages[0] * 0.5f)
            {

                while (true)
                {
                    yield return null;
                    for (int i = 0; i < listTransPerc.Count; i++)
                    {
                        TransPercentageStruct itemStruct = listTransPerc[i];
                        itemStruct.curveItemData.Percentage = Mathf.Lerp(itemStruct.curveItemData.Percentage, basePercentages[i], 5 * Time.unscaledDeltaTime);
                        SetTransPosForTweenPerc(listTransPerc[i].curveItemView.transform, itemStruct.curveItemData.Percentage);
                    }
                    if (Abs(listTransPerc[0].curveItemData.Percentage - basePercentages[0]) <= 0.0001f)
                        break;
                }
            }
            //如果没有过半 就后退 回到上一个的地方
            else
            {
                float d_value = listTransPerc[0].curveItemData.Percentage - 0;
                //更换收尾
                while (d_value >= 0)
                {
                    yield return null;
                    if (d_value >= 0)
                    {
                        float off = Time.unscaledDeltaTime * 0.5f;
                        d_value -= off;
                        OnScrollChange(-off);
                    }
                }
                //更换之后在排序
                listTransPerc.Sort(new TransPercentageStruct());
                while (true)
                {
                    yield return null;
                    //归位
                    for (int i = 0; i < listTransPerc.Count; i++)
                    {
                        TransPercentageStruct itemStruct = listTransPerc[i];
                        itemStruct.curveItemData.Percentage = Mathf.Lerp(itemStruct.curveItemData.Percentage, basePercentages[i], 5 * Time.unscaledDeltaTime);
                        SetTransPosForTweenPerc(listTransPerc[i].curveItemView.transform, itemStruct.curveItemData.Percentage);
                    }
                    if (Abs(listTransPerc[0].curveItemData.Percentage - basePercentages[0]) <= 0.0001f)
                        break;
                }
            }
            endCoroutine = null;
            isDragEnd = false;
        }

        #region Struct
        private struct TransPercentageStruct : IComparer<TransPercentageStruct>
        {
            public CurveScrollerItemView curveItemView;
            public CurveScrollerItemData curveItemData;

            public TransPercentageStruct(CurveScrollerItemView _curveItemView, CurveScrollerItemData _curveItemData)
            {
                this.curveItemView = _curveItemView;
                this.curveItemData = _curveItemData;
            }

            public void SetData(CurveScrollerItemData _curveItemData)
            {
                this.curveItemData = _curveItemData;
                curveItemView.SetData(_curveItemData);
            }
            public int Compare(TransPercentageStruct x, TransPercentageStruct y)
            {
                if (x.curveItemData.Percentage > y.curveItemData.Percentage) return 1;
                else if (x.curveItemData.Percentage < y.curveItemData.Percentage) return -1;
                else return 0;
            }
        }
        #endregion

        private float Abs(float v)
        {
            return v > 0f ? v : v * -1;
        }
    }
}