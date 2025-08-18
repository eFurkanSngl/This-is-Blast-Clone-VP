using DG.Tweening;
using UnityEngine;


public class MoveAnimation: IMoveAnim
{
    public Sequence MoveAnim(
         Transform tr,
         Vector3 targetWorldPos,
         float duration = 0.48f,
         float jumpPower = 0.42f,
         float squash = 0.10f,
         System.Action onComplete = null)
    {
        tr.DOKill();

        var seq = DOTween.Sequence();

        seq.Append(tr.DOScale(new Vector3(1f + squash, 1f - squash, 1f), duration * 0.35f)
            .SetEase(Ease.OutSine));

        seq.Append(tr.DOJump(targetWorldPos, jumpPower, 1, duration)
            .SetEase(Ease.InOutSine));

        seq.Append(tr.DOScale(Vector3.one, duration * 0.25f)
            .SetEase(Ease.OutBack));

        if (onComplete != null)
            seq.OnComplete(() => onComplete());

        return seq;
    }

    public void Kill(Transform tr) => tr.DOKill();
}
