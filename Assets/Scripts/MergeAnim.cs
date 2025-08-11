using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MergeAnim : IMergeAnim
{
    public void PlayMergeAnim(
        List<int> matchedList,
        GoalItem[] goalItemsInLauncher,
        Transform[] launcherBox,
        float mergeAnimDuration,
        Action onComplete = null)
    {
        matchedList.Sort();
        int centerIndex = matchedList[1];
        GoalItem centerItem = goalItemsInLauncher[centerIndex];

        for (int i = 0; i < matchedList.Count; i++)
        {
            int currentIndex = matchedList[i];
            if (currentIndex == centerIndex) continue;

            GoalItem itemMerge = goalItemsInLauncher[currentIndex];
            if (itemMerge == null) continue;

            Transform targetTrans = launcherBox[centerIndex];
            Transform itemTrans = itemMerge.transform;

            Sequence seq = DOTween.Sequence();
            seq.Append(itemTrans.DOMove(targetTrans.position, mergeAnimDuration).SetEase(Ease.InOutSine));
            seq.Join(itemTrans.DOScale(1.4f, mergeAnimDuration));
            seq.Append(itemTrans.DOScale(0.3f, mergeAnimDuration));
            seq.AppendCallback(() =>
            {
                itemTrans.DOKill();
                UnityEngine.Object.Destroy(itemMerge.gameObject);
                goalItemsInLauncher[currentIndex] = null;
            });
        }

        centerItem.transform.DOScale(1.5f, mergeAnimDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                centerItem.transform.DOScale(Vector3.one, mergeAnimDuration).SetEase(Ease.InOutSine);
                onComplete?.Invoke(); // ← buradan callback’i tetikliyoruz
            });
    }
}