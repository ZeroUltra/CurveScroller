using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class TestA : MonoBehaviour
{
    public DOTweenPath tweenPath;

    public TestA()
    {

    }
    void Start()
    {

        Debug.Log($"tweenpath.lastpostion:{tweenPath.lastSrcPosition}");    
    }

    
}
