using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMergeAnim
{
    void PlayMergeAnim(
        List<int> matchedList,
        GoalItem[] goalItemsLauncer,
        Transform[] launcherBox,
        float duration,
        Action onCompelete = null
        );
}