using DG.Tweening;
using UnityEngine;


public class SwapAnimation: ISwapAnim
{
    public void Play(
        Transform transform,
        float delay,
        float swapDuration,
        float loopDuration,
        TweenCallback onComplete = null
        )
    {
        transform.localScale = new Vector3(1f, 0.85f, 1f);
        
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);

        seq.Append(transform.DOScale(Vector3.one * 1.2f, swapDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(Vector3.one * 0.97f, swapDuration * 0.3f).SetEase(Ease.InOutSine));
        seq.Append(transform.DOScale(Vector3.one, swapDuration * 0.2f).SetEase(Ease.OutSine));

        seq.AppendCallback(() =>
        {
            transform.DOScale(Vector3.one * 1.1f, loopDuration * 1.2f)
             .SetEase(Ease.InOutSine)
             .SetLoops(-1, LoopType.Yoyo);
        });
    }

}