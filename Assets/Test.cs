using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test : MonoBehaviour
{
    public DOTweenPath tweenPath;
    public Transform[] trans;

    private float offperc = 0;
    public Vector3[] wps;
    public float[] wpDisArray;

    void Start()
    {
        offperc = 1f / trans.Length;

        Debug.Log("路径长度: " + tweenPath.tween.PathLength());
        wps = tweenPath.wps.ToArray();
        Vector3 startpos = transform.position;

        wpDisArray = tweenPath.path.wpLengths;

        float[] rates = new float[3];
        float rate = 0;

        for (int i = 0; i < wpDisArray.Length; i++)
        {
            if (i == 0) continue;
            rate += wpDisArray[i] / tweenPath.tween.PathLength();
            Debug.Log(rate);
            rates[i-1] = rate;
        }
        for (int i = 0; i < trans.Length; i++)
        {
            trans[i].position = tweenPath.tween.PathGetPoint(rates[i]);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    for (int i = 0; i < trans.Length; i++)
        //    {
        //        trans[i].position = tweenPath.tween.PathGetPoint((i + 1) * offperc);
        //    }
        //}
    }

}
