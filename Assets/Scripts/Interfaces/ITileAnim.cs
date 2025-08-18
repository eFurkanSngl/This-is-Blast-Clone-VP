using System;
using UnityEngine;

public interface ITileAnim
{
    void PlayDestroyAnim(Transform target,Action onComplete = null);
}