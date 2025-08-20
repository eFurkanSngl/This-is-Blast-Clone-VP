using UnityEngine;
using System;
public interface IGoalItemExitScreen
{
    void PlayExit(GoalItem goalItem, Action onComplete = null);
}
