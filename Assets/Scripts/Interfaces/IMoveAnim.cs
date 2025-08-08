using DG.Tweening;
using UnityEngine;

public interface IMoveAnim
{
    Sequence MoveAnim(
        Transform _transform,
        Vector3 _targetPos,
        float _duration = 0.48f,
        float _jumpPower = 0.42f,
        float _squash = 0.10f,
        System.Action onComplete = null
        );

    void Kill(Transform transform);
}