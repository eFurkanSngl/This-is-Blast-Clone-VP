using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LauncherManager : MonoBehaviour
{
    [SerializeField] private Transform[] _launcherBox;
    [SerializeField] private Transform _hiddenLauncherBox;
    [SerializeField] private float _placeGoalDuration = 0.08f;
    [SerializeField] private float _mergeAnimDuration = 0.1f;
    [Inject] private SignalBus _signalBus;
    [Inject] private IMergeAnim _mergeAnim;
    [Inject] private GridManager _gridManager;
    private GoalItem[] _goalItemsInLauncher;
    private bool[] _reservedSlot;
    List<int> matchedList = new List<int>();
    private Dictionary<int,bool> _idFireLock = new Dictionary<int,bool>();
    private bool _isMerging = false;
    private void Awake()
    {
        _goalItemsInLauncher = new GoalItem[_launcherBox.Length];
        _reservedSlot = new bool[_launcherBox.Length];
    }

    public void PlayOnShot()
    {
        if (_hiddenLauncherBox == null) return;

        GoalItem leftItem = null;
        int leftIndex = -1;

        for (int i = 0; i < _launcherBox.Length; i++)
        {
            if (_launcherBox[i].childCount > 0)
            {
                leftItem = _launcherBox[i].GetComponentInChildren<GoalItem>();
                if (leftItem != null)
                {
                    leftIndex = GetGoalItemIndex(leftItem);
                    break; 
                }
            }
        }

        if (leftItem == null || leftIndex < 0)
        {
            Debug.Log("[LauncherManager] PlayOn: GoalItem bulunamadı");
            return;
        }

        ClearGoalItem(leftIndex);
        SlideToHiddenAndFire(leftItem);
        PlaceGoalBoxAnim(leftItem);
    }

    private void SlideToHiddenAndFire(GoalItem item)
    {
        if (item == null) return;

        Transform transform = item.transform;
        transform.DOKill();

        transform.SetParent(_hiddenLauncherBox, true);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(_hiddenLauncherBox.position, 0.22f).SetEase(Ease.OutCubic));
        seq.Join(transform.DOPunchScale(new Vector3(0.12f, -0.12f, 0f), 0.18f, 6, 0.6f));

        seq.OnComplete(() =>
        {
            transform.localPosition = new Vector3(0f, 0f, -0.4f);
            transform.localScale = Vector3.one;

            BoxCollider col = item.GetCollider();
            if (col != null)
            {
                col.enabled = false;
            }

            StartCoroutine(FireGoalItemRoutine(item));
        });
    }
    public void ClearGoalItem(int index)
    {
        if(index >= 0 && index < _goalItemsInLauncher.Length)
        {
            _goalItemsInLauncher[index] = null;
            _reservedSlot[index] = false;
        }
    }

    public int GetGoalItemIndex(GoalItem goalItem)
    {
        for(int i = 0; i < _goalItemsInLauncher.Length; i++)
        {
            if(_goalItemsInLauncher [i] == goalItem) return i;
        }
        return -1;
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

    public void PlaceGoalItem(GoalItem goalItem , int index)
    {
        goalItem.transform.SetParent(_launcherBox[index],false);
        PlaceGoalBoxAnim(goalItem);

        goalItem.GetCollider().enabled = false;
        _goalItemsInLauncher[index] = goalItem;
        _reservedSlot[index] = false;

       CheckMerge();
        StartCoroutine(FireGoalItemRoutine(goalItem));

    }

    private IEnumerator FireGoalItemRoutine(GoalItem item)
    {
        int id = item.GetID();

        if (!_idFireLock.ContainsKey(id))
        {
            _idFireLock[id] = false;
        }

        while (_idFireLock[id] || _isMerging) yield return null;

        _idFireLock[id] = true;

        yield return StartCoroutine(_gridManager.GoalItemMatch(item));

        _idFireLock[id] = false;
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
        _isMerging = true;
        matchedList.Sort();
        int centerIndex = matchedList[1];
        GoalItem centerItem = _goalItemsInLauncher[centerIndex];

        for (int i = 0; i < matchedList.Count; i++)
        {
            int currentIndex = matchedList[i];
            if (currentIndex == centerIndex)
                continue;

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
            ItemTextMerge(matchedList);
            centerItem.transform.DOScale(Vector3.one, _mergeAnimDuration).SetEase(Ease.InOutSine);
            HapticManager.PlayHaptic(HapticManager.HapticType.Heavy);
            _signalBus.Fire<MergeSignal>();
            _isMerging = false;

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
