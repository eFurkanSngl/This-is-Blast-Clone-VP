using System;
using UnityEngine;

public interface ITileAnim
{
    void PlayDestroyAnim(Action onComplete = null);
}