using DG.Tweening;
using UnityEngine;

public class ClickAnimation : IClickAnim
{
    public void Play(Transform transform,Vector3 pos,float jump, float duration, TweenCallback OnComplete = null)
    {
        transform.localScale = Vector3.one * 0.9f;
        transform.DORotate(Vector3.zero, 0.1f);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(transform.DOJump(pos, jump, 1, duration).SetEase(Ease.InOutSine));
        seq.Join(transform.DORotate(new Vector3(0, 0, 15f), duration / 2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));
        seq.Append(transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
        seq.OnComplete(OnComplete);
    }
}
