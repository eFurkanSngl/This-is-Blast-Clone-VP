using DG.Tweening;
using UnityEngine;

public interface IClickAnim
{
    void Play(Transform target, Vector3 pos, float jump, float duration, TweenCallback OnComplete = null);
}
