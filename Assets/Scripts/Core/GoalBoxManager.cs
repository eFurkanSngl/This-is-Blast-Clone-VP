using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GoalBoxManager : MonoBehaviour
{
    [Header("Box References")]
    [SerializeField] private Transform[] _openBox;
    [SerializeField] private Transform[] _closeBox;
    [SerializeField] private GameObject[] _goalItem;

    [Header("Animation Settings")]
    [SerializeField] private float _animSize = 1.2f;
    [SerializeField] private float _loopDuration = 0.4f;
    [SerializeField] private float _spawnDuration = 0.3f;
    [SerializeField] private float _delay = 0.1f;

    [Inject] private TilePool _tilePool;
    [Inject] private LauncherManager _launcherManager;
    [Inject] private SignalBus _signalBus;
    [Inject] private IMoveAnim _moveAnim;
    [Inject] private IClickAnim _clickAnim;
    [Inject] private ISwapAnim _swapAnim;

    private List<Transform> closedBoxList = new List<Transform>();
    private List<Transform> allboxes = new List<Transform>();


    private void Awake()
    {
        allboxes.AddRange(_openBox);
        allboxes.AddRange(_closeBox);
        closedBoxList.AddRange(_closeBox);
    }

    private void Start()
    {
        SpawnGoalPrefab();   
        AnimOpenBox();
    }

    private void SpawnGoalPrefab()
    {
        Shuffle(allboxes);

        int totalSpawn = _goalItem.Length * 2;
        if(allboxes.Count < totalSpawn)
        {
            Debug.Log("Boxes not enough");
            return;
        }

        int index = 0;
        foreach(GameObject prefab in _goalItem)
        {
            for(int i = 0; i < 3; i++)
            {
                Transform targetBox = allboxes[index];
                GameObject newBox = Instantiate(prefab, targetBox);
                newBox.transform.localPosition = Vector3.zero;
                newBox.transform.rotation = Quaternion.identity;
                GoalItemCache cache = newBox.GetComponent<GoalItemCache>();

                CloseBoxOutLineEffect(targetBox, cache);
                GetGoalItem(cache);
                OpenBoxClick(targetBox,cache);
                CloseBoxClick(targetBox, cache);
                index++;
            }
           
        }
    }
    private void CloseBoxClick(Transform targetBox, GoalItemCache cache)
    {
        if (_closeBox.Contains(targetBox))
        {
            cache.goalItemClickHandler.Set(this,true); 
        }
    }
    private void OpenBoxClick(Transform target, GoalItemCache cache)
    {
        if (_openBox.Contains(target))
        {
            cache.goalItemClickHandler.Set(this,false);
        }
    }

    public void CloseGoalItemClick(GameObject clickedObj)
    {
        DOTween.Kill(clickedObj.transform);

        clickedObj.transform.DOShakePosition(
            0.4f,   
            0.1f,  
            6,      
            50f,   
            false,  
            true    
        ).SetEase(Ease.OutQuad);

    }
    private void GetGoalItem(GoalItemCache cache)
    {
        if(cache.goalItem != null)
        {
            int id = (int)cache.goalItem.GetColor();
            int totalCount = _tilePool.TileCount[id];
            cache.goalItem.Initialize((totalCount/3) * 2);
        }
    }

    public void OnGoalItemClicked(GameObject clickedObj)
    {
        if(_launcherManager.HasEmptyBox(out int emptyIndex))
        {
            GoalItemCache cache = clickedObj.GetComponent<GoalItemCache>();
            clickedObj.transform.SetParent(_launcherManager.GetBoxTransform(emptyIndex));
            ClickedAnim(cache, emptyIndex);
            HapticManager.PlayHaptic(HapticManager.HapticType.Light);
            _signalBus.Fire<ClickSignalBus>();
            RemoveOutline(cache);
            cache.goalItem.EnableTrail();
            DOTween.Kill(clickedObj);
        }
    }
    private void Shuffle(List<Transform> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void CloseBoxOutLineEffect(Transform target , GoalItemCache cache)
    {
        if (closedBoxList.Contains(target))
        {
            cache.outLineEffect.DisableOutLine();
            cache.alphaEffect.SetAlpha(0.6f);
        }
    }

    private void RemoveAlpha(GoalItemCache cache)
    {
        cache.alphaEffect.FadeAlpha(1f, 0.2f);
    }

    private void RemoveOutline(GoalItemCache cache)
    {
        cache.outLineEffect.DisableOutLine();
    }
    private void AnimOpenBox()
    {
        for(int i = 0; i < _openBox.Length; i++)
        {
           Transform box = _openBox[i];
            if (box.childCount == 0) continue;

            GameObject child = box.GetChild(0).gameObject;
            AnimNewGoalItem(child, i * _delay);
        }
    }

    private void AnimNewGoalItem(GameObject child , float delay = 0.1f)
    {
        _swapAnim.Play(child.transform, delay, _spawnDuration, _loopDuration, null);

        var cache = child.GetComponent<GoalItemCache>();
        cache.outLineEffect.EnableOutLine();
    }   

    private void ClickedAnim(GoalItemCache cache,int index)
    {
        Vector3 targetPos = _launcherManager.GetBoxTransform(index).position;
        _clickAnim.Play(
       cache.transform,
       targetPos,
       jump: 1.2f,
       duration: 0.45f,
       OnComplete: () =>
       {
           cache.transform.localScale = Vector3.one;
           cache.transform.localPosition = new Vector3(0f, 0f, -0.55f);

           _launcherManager.PlaceGoalItem(cache.goalItem, index);
           CheckAndMoveCloseBox();
       }
   );
    }
    private void CheckAndMoveCloseBox()
    {
        for (int i = 0; i < _openBox.Length; i++)
        {
            Transform openBox = _openBox[i];
            if (openBox.childCount == 0)
            {
                Transform closeBox = _closeBox[i];

                if (closeBox.childCount > 0)
                {
                    GameObject goalItem = closeBox.GetChild(0).gameObject;
                    var cache = goalItem.GetComponent<GoalItemCache>();

                    goalItem.transform.SetParent(openBox);
                    _moveAnim.MoveAnim(
                       _transform:goalItem.transform,
                       _targetPos:openBox.position,
                        _duration:0.20f,
                        _jumpPower: 0.42f,
                        _squash: 0.10f,
                        onComplete: () =>
                        {
                            goalItem.transform.localScale = Vector3.one;
                            AnimNewGoalItem(goalItem);
                            _signalBus.Fire<SwipeSignalBus>();

                            cache.goalItem.DisableTrail(0.2f);
                        }
                        );
                    cache.goalItem.EnableTrail();

                    RemoveAlpha(cache);
                    OpenBoxClick(openBox,cache);
                    CascadeCloseBox(closeBox, i);
                }
            }
        }
    }
    private void CascadeCloseBox(Transform upperCloseBox, int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _closeBox.Length - _openBox.Length)
            return;

        int lowerIndex = columnIndex + _openBox.Length;
        Transform lowerCloseBox = _closeBox[lowerIndex];

        if (lowerCloseBox.childCount > 0)
        {
            GameObject goalItem = lowerCloseBox.GetChild(0).gameObject;
            GoalItemCache cache = goalItem.GetComponent<GoalItemCache>();
            goalItem.transform.SetParent(upperCloseBox);
            goalItem.transform.position = lowerCloseBox.position;


            _moveAnim.MoveAnim(goalItem.transform, upperCloseBox.position, 0.14f, 0.30f, 0.08f,()=>
            {
                goalItem.transform.localScale = Vector3.one;
                goalItem.transform.localPosition = Vector3.zero;
                cache.goalItem.DisableTrail(0.2f);
            });
            cache.goalItem.EnableTrail();
        }
    }
}
    