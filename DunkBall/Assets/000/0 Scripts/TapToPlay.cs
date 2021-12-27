using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TapToPlay : MonoBehaviour
{
    void Start()
    {
        transform.DOScale(Vector3.one * 1.1f, 1).SetLoops(-1, LoopType.Yoyo);
    }
}