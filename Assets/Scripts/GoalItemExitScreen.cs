using DG.Tweening;
using System;
using UnityEngine;
using Zenject;

public class GoalItemExitScreen : IGoalItemExitScreen
{
    private float _exitOffset = 20f;
    private bool _isRight = false;
    [Inject] private LauncherManager _launcherManager;
    [Inject] private SignalBus _signalBus;
    public void PlayExit(GoalItem goalItem, Action onComplete = null)
    {
        if (goalItem == null) return;
        goalItem.transform.DOKill();

        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float leftEdge = cam.transform.position.x - camWidth / 2f;
        float rightEdge = cam.transform.position.x + camWidth / 2f;

        float goalItemX = goalItem.transform.position.x;

        int index = _launcherManager.GetGoalItemIndex(goalItem);

        Vector3 exitPos;

        if (goalItemX >= cam.transform.position.x)
        {
            exitPos = new Vector3(rightEdge + _exitOffset, goalItem.transform.position.y, goalItem.transform.position.z);
            _isRight = true;
            Debug.Log($"[EXIT] {goalItem.name} SAĞA çıkıyor: {exitPos}");

        }
        else
        {
            exitPos = new Vector3(leftEdge - _exitOffset, goalItem.transform.position.y, goalItem.transform.position.z);
            _isRight= false;
            Debug.Log($"[EXIT] {goalItem.name} SOLA çıkıyor: {exitPos}");
        }

        Vector3 direction = _isRight ? Vector3.right : Vector3.left;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        Vector3 start = goalItem.transform.position;
        Vector3 forwardPos = start + new Vector3(0, 0, 3f);
        Vector3 curvePos = (start + exitPos) * 0.5f + new Vector3(0, UnityEngine.Random.Range(0.3f, 0.9f), 0);
        Sequence seq = DOTween.Sequence();
        seq.Append(goalItem.transform.DOMove(forwardPos, 0.3f)
            .SetEase(Ease.OutQuad));
        seq.Append(goalItem.transform.DORotateQuaternion(lookRotation, 0.3f)
            .SetEase(Ease.OutBack));
        seq.Append(goalItem.transform.DOMove(curvePos, 0.3f)
            .SetEase(Ease.InOutSine));
        seq.Append(goalItem.transform.DOMove(exitPos, 0.3f)
            .SetEase(Ease.InOutSine));
        _launcherManager.ClearGoalItem(index);
        _signalBus.Fire<SwipeSignalBus>();

        seq.OnComplete(() =>
        {
            goalItem.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}

