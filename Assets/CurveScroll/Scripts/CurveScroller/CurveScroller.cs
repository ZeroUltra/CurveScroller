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
        #region SerializeField
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
        [SerializeField] float localScaleRate = 1; //克隆出来缩放系数
        [SerializeField] DOTweenPath tweenPath;
        [SerializeField] CurveScrollerItemComponent curveItemPrefab;
        #endregion

        #region Private
        private bool isInit = false;
        private float[] basePercentages;  //初始百分比 用于还原
        private SmallList<CurveScrollerItemData> listData = new SmallList<CurveScrollerItemData>();  //存放数据
        private List<TransPercentageStruct> listTransPerc = new List<TransPercentageStruct>();  //记录trans 和 百分比

        //------------------
        private float tempOffv = 0;  //用来缓存最后一个滑动值
        private Tweener inertiaTweenr;
        private float half0Prec;
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
            //遍历 i=1 第一个永远是0
            for (int i = 0; i < needwplengths.Length; i++)
            {
                tempPercentage += needwplengths[i] / pathLength;
                basePercentages[i] = tempPercentage;
            }
            half0Prec = basePercentages[0] * 0.5f;
            for (int i = 0; i < basePercentages.Length; i++)
            {
                //克隆对象
                CurveScrollerItemComponent cutveItem = Instantiate(curveItemPrefab.gameObject).GetComponent<CurveScrollerItemComponent>();
                cutveItem.transform.transform.SetParent(this.transform);
                cutveItem.transform.localScale = Vector3.one * localScaleRate;

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
            OnScrollChange(offv, false);
        }

        /// <summary>
        /// 滑动结束
        /// </summary>
        public void DoScrollEnd()
        {
            //  Debug.Log(tweenPath)
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
            if (Mathf.Approximately(rate, 0)) return;
            inertiaTweenr = DOTween.To(() => tempOffv, x => tempOffv = x, 0, rate).SetSpeedBased(true)
                .OnUpdate(() => OnScrollChange(tempOffv, true));

        }
        /// <summary>
        /// 结束吸附
        /// </summary>
        private void OnEndFixed()
        {
            //排序 //按照百分比大小排序
            listTransPerc.Sort(new TransPercentageStruct());

            //判断是否滑动超过一半
            if (listTransPerc[0].curveItemData.Percentage >= half0Prec)
            {
                for (int i = 0; i < listTransPerc.Count; i++)
                {
                    int index = i;
                    TransPercentageStruct itemStruct = listTransPerc[i];
                    DOTween.To(() => itemStruct.curveItemData.Percentage, x => itemStruct.curveItemData.Percentage = x, basePercentages[index], fixedDuartion).OnUpdate(() =>
                    {
                        SetTransPosForTweenPerc(listTransPerc[index].cueveItem.transform, listTransPerc[index].curveItemData.Percentage);
                        listTransPerc[index] = itemStruct;
                    });
                }
            }
            //后退一格
            else
            {
                float num = listTransPerc[0].curveItemData.Percentage - 0f;
                StartCoroutine(IEDoEnd(-num));
            }
        }
        IEnumerator IEDoEnd(float nums)
        {
            float timer = fixedDuartion;
            float offv = nums / timer;
            while (timer>=0)
            {
                timer -= Time.deltaTime;
                DoScroll(offv*Time.deltaTime);
                yield return null;
            }
        }

        private void OnScrollChange(float offv, bool endAction)
        {
            //如果不是结束动作 当拖动的时候 应该关掉惯性tweenr
            if (!endAction && inertiaTweenr != null && inertiaTweenr.IsPlaying())
            {
                inertiaTweenr.Kill();
            }
            tempOffv = offv;
            for (int i = 0; i < listTransPerc.Count; i++)
            {
                int index = i;
                TransPercentageStruct itemStruct = listTransPerc[i];
                itemStruct.curveItemData.Percentage += offv;
                if (itemStruct.curveItemData.Percentage >= 1f)
                {
                    itemStruct.cueveItem.transform.SetAsFirstSibling();

                    // 取出最后一个然后把当前的插入到开头
                    CurveScrollerItemData data = listData.RemoveEnd();
                    float offset = itemStruct.curveItemData.Percentage - 1f;
                    data.Percentage = 0 + offset;

                    listData.AddStart(itemStruct.curveItemData);
                    itemStruct.SetData(data);
                }
                else if (itemStruct.curveItemData.Percentage <= 0f)
                {
                    itemStruct.cueveItem.transform.SetAsLastSibling();

                    //往上滑动 取出第一个然后把当前的插到最后一个
                    CurveScrollerItemData data = listData.RemoveStart();
                    listData.Add(itemStruct.curveItemData);

                    float offset = 0f - itemStruct.curveItemData.Percentage;
                    data.Percentage = 1f - offset;
                    itemStruct.SetData(data);
                }
                listTransPerc[index] = itemStruct;
                SetTransPosForTweenPerc(listTransPerc[index].cueveItem.transform, listTransPerc[index].curveItemData.Percentage);
            }
        }

        private void SetTransPosForTweenPerc(Transform trans, float percentage)
        {
            trans.position = tweenPath.tween.PathGetPoint(percentage);
        }

        #region Struct
        private struct TransPercentageStruct : IComparer<TransPercentageStruct>
        {
            public CurveScrollerItemComponent cueveItem;
            public CurveScrollerItemData curveItemData;

            public TransPercentageStruct(CurveScrollerItemComponent _cueveItem, CurveScrollerItemData _curveItemData)
            {
                this.cueveItem = _cueveItem;
                this.curveItemData = _curveItemData;
            }

            public void SetData(CurveScrollerItemData _curveItemData)
            {
                this.curveItemData = _curveItemData;
                cueveItem.SetData(_curveItemData);
            }
            public int Compare(TransPercentageStruct x, TransPercentageStruct y)
            {
                if (x.curveItemData.Percentage > y.curveItemData.Percentage) return 1;
                else if (x.curveItemData.Percentage < y.curveItemData.Percentage) return -1;
                else return 0;
            }
        }
        #endregion
    }
}