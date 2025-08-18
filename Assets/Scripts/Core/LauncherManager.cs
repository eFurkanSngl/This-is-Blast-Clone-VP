using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static UnityEditor.Progress;

public class LauncherManager : MonoBehaviour
{
    [SerializeField] private Transform[] _launcherBox;
    [SerializeField] private float _placeGoalDuration = 0.08f;
    [SerializeField] private float _mergeAnimDuration = 0.1f;
    [Inject] private SignalBus _signalBus;
    [Inject] private IMergeAnim _mergeAnim;
    [Inject] private GridManager _gridManager;
    private GoalItem[] _goalItemsInLauncher;
    private bool[] _reservedSlot;
    List<int> matchedList = new List<int>();
    private Dictionary<int,bool> _idFireLock = new Dictionary<int,bool>();
    private void Awake()
    {
        _goalItemsInLauncher = new GoalItem[_launcherBox.Length];
        _reservedSlot = new bool[_launcherBox.Length];
    }
    
    private IEnumerator FireGoalItemRoutine(GoalItem goalItem)
    {
        int id = goalItem.GetID();

        if (!_idFireLock.ContainsKey(id))
        {
            _idFireLock[id] = false;
        }

        while (_idFireLock[id]) yield return null;

        _idFireLock[id] |= true;

        yield return new WaitForSeconds(0.1f);

        //_gridManager.GoalItemMatchRoutine(goalItem);
        Debug.Log("start routine");

        _idFireLock[id] = false;
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

        goalItem.GetCollider().enabled = false;
        _goalItemsInLauncher[index] = goalItem;
        _reservedSlot[index] = false;

        CheckMerge();

        _gridManager.GoalItemMatchRoutine(goalItem);
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
        matchedList.Clear();
        for(int i = 0; i < _goalItemsInLauncher.Length; i++)
        {
            if( _goalItemsInLauncher[i] == null) continue;

            int matchId = _goalItemsInLauncher[i].GetID();
            int matchCount = 1;

            matchedList.Clear();
            matchedList.Add(i);

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
        _mergeAnim.PlayMergeAnim(
          matchedList,
          _goalItemsInLauncher,
          _launcherBox,
          _mergeAnimDuration,
         onCompelete: ()=>
          {
              ItemTextMerge(matchedList);
              _signalBus.Fire<MergeSignal>();

              // istersen burada eşleşme kontrolünü ya da ateşlemeyi tetikle
              // StartCoroutine(_gridManager.GoalItemMatchRoutine(_goalItemsInLauncher[matchedList[1]]));
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
