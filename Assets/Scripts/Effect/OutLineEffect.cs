using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLineEffect : MonoBehaviour
{
    [SerializeField] private MeshRenderer _OutLineRenderer;

    public void EnableOutLine()
    {
        _OutLineRenderer.enabled = true;
    }

    public void DisableOutLine()
    {
        _OutLineRenderer.enabled = false;
    }
}
