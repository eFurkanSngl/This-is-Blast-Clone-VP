using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaEffect : MonoBehaviour
{
    private MeshRenderer _renderer;
    private Color _originColor;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _originColor = _renderer.material.color;
    }

    public void SetAlpha(float alpha)
    {
        Color color = _renderer.material.color;
        color.a = alpha;
        _renderer.material.color = color;
    }

    public void FadeAlpha(float targetAlpha, float duration)
    {
        Color startColor = _renderer.material.color;
        Color endColor = startColor;
        endColor.a = targetAlpha;

        _renderer.material.DOColor(endColor,duration);
    }

    public void ResetColor()
    {
        _renderer.material.color = _originColor;
    }
}
