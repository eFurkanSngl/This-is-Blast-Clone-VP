using DG.Tweening;
using UnityEngine;

public interface ISwapAnim
{
    void Play(
        Transform transform,
        float delay,
        float swapDuration,
        float loopDuration,
        TweenCallback onComplete = null
        );
}