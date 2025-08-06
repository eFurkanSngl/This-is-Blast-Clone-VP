using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LauncherManager : MonoBehaviour
{
    [SerializeField] private Transform[] _launcherBox;
    [SerializeField] private float _placeGoalDuration = 0.08f;
    [SerializeField] private float _mergeAnimDuration = 0.1f;
    private GoalItem[] _goalItemsInLauncher;
    private bool[] _reservedSlot;

    private void Awake()
    {
        _goalItemsInLauncher = new GoalItem[_launcherBox.Length];
        _reservedSlot = new bool[_launcherBox.Length];
    }

    public bool HasEmptyBox(out int index)
    {
        for(int i = 0; i  < _launcherBox.Length; i++)
        {
            if(_goalItemsInLauncher[i] == null && !_reservedSlot[i])
            {
                index = i;
                _reservedSlot[i] = true;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public bool HasFullBox()
    {
        foreach(var goalItem in _goalItemsInLauncher)
        {
            if (goalItem == null) return false;
        }
        return true;
    }

    public void RemoveGoalItem(int index)
    {
        if( index >= 0 && index < _goalItemsInLauncher.Length)
        {
            if(_goalItemsInLauncher[index] != null)
            {
                Destroy(_goalItemsInLauncher[index].gameObject);
                _goalItemsInLauncher[index] = null;
            }
        }
    }
    public void PlaceGoalItem(GoalItem goalItem , int index)
    {
        goalItem.transform.SetParent(_launcherBox[index],false);
        //PlaceGoalBoxAnim(goalItem);

        BoxCollider boxCollider = goalItem.GetComponent<BoxCollider>();
        boxCollider.enabled = false;

        _goalItemsInLauncher[index] = goalItem;
        _reservedSlot[index] = false;

        CheckMerge();

    }


    private void PlaceGoalBoxAnim(GoalItem item)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(item.transform.DOScale(new Vector3(1.3f, 0.7f, 1f), _placeGoalDuration).SetEase(Ease.OutQuad));
        seq.Append(item.transform.DOScale(new Vector3(0.9f, 1.1f, 1f), _placeGoalDuration).SetEase(Ease.OutQuad));
        seq.Append(item.transform.DOScale(Vector3.one, _placeGoalDuration).SetEase(Ease.OutQuad));
    }

    public Transform GetBoxTransform(int index)
    {
        return _launcherBox[index];
    }

    private void CheckMerge()
    {
        for(int i = 0; i < _goalItemsInLauncher.Length; i++)
        {
            if( _goalItemsInLauncher[i] == null) continue;

            int matchId = _goalItemsInLauncher[i].GetID();
            int matchCount = 1;
            List<int> matchedList = new List<int> { i };

            for(int y = i + 1; y < _goalItemsInLauncher.Length; y++)
            {
                if (_goalItemsInLauncher[y] != null && _goalItemsInLauncher[y].GetID() == matchId)
                {
                    matchCount++;
                    matchedList.Add(y);
                }
            }

            if(matchCount == 3)
            {
                Debug.Log("is merge");
                MergeAnim(matchedList);
                return;
            }
        }
    }

    private void MergeAnim(List<int> matchedList)
    {
        matchedList.Sort();
        int centerIndex = matchedList[1];
        GoalItem centerItem = _goalItemsInLauncher[centerIndex];

        for (int i = 0; i < matchedList.Count; i++)
        {
            int currentIndex = matchedList[i];
            if (currentIndex == centerIndex) continue;

            GoalItem itemMerge = _goalItemsInLauncher[currentIndex];
            if (itemMerge == null) continue;

            Transform targetTrans = _launcherBox[centerIndex];
            Transform itemTrans = itemMerge.transform;

            Sequence seq = DOTween.Sequence();
            seq.Append(itemTrans.DOMove(targetTrans.position, _mergeAnimDuration).SetEase(Ease.InOutSine));
            seq.Join(itemTrans.DOScale(1.4f, _mergeAnimDuration));
            seq.Append(itemTrans.DOScale(0.3f, _mergeAnimDuration));
            seq.AppendCallback(() =>
            {
                itemTrans.DOKill();
                Destroy(itemMerge.gameObject);
                _goalItemsInLauncher[currentIndex] = null;
            });
        }

        centerItem.transform.DOScale(1.5f, _mergeAnimDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                centerItem.transform.DOScale(Vector3.one, _mergeAnimDuration).SetEase(Ease.InOutSine);
                ItemTextMerge(matchedList);
            });
    }
    private void ItemTextMerge(List<int> matchedList)
    {
        matchedList.Sort();
        int centerIndex = matchedList[1];
        GoalItem centerItem = _goalItemsInLauncher[centerIndex];
        if (centerItem == null) return;

        int centerID = centerItem.GetID();
        int totalCount = 0;

        HashSet<int> uniqueIndices = new HashSet<int>(matchedList);
        foreach (int index in uniqueIndices)
        {
            var item = _goalItemsInLauncher[index];
            if (item == null) continue;
            if (item.GetID() == centerID)
            {
                totalCount += item.CurrentCount;
            }
        }
        centerItem.Initialize(totalCount);
    }
}
