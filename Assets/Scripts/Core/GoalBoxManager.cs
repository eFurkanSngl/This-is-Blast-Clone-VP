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

                CloseBoxOutLineEffect(targetBox, newBox);
                GetGoalItem(newBox);
                OpenBoxClick(newBox, targetBox);
                CloseBoxClick(newBox, targetBox);
                index++;
            }
           
        }
    }
    private void CloseBoxClick(GameObject obj, Transform targetBox)
    {
        if(obj.TryGetComponent<GoalItemClickHandler>(out var goalItemClick))
        {
            if (_closeBox.Contains(targetBox))
            {
                goalItemClick.Set(this, true);
            }
        }
    }
    private void OpenBoxClick(GameObject obj ,Transform target)
    {
        if(obj.TryGetComponent<GoalItemClickHandler>(out var goalItemClick))
        {
            if (_openBox.Contains(target))
            {
                goalItemClick.Set(this, false);
            }
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
    private void GetGoalItem(GameObject obj)
    {
        if(obj.TryGetComponent<GoalItem>(out var goalItem))
        {
            int id = (int)goalItem.GetColor();
            int totalCount = _tilePool.TileCount[id];
            goalItem.Initialize(totalCount / 3);
        }
    }

    public void OnGoalItemClicked(GameObject clickedObj)
    {
        if(_launcherManager.HasEmptyBox(out int emptyIndex))
        {
            clickedObj.transform.SetParent(_launcherManager.GetBoxTransform(emptyIndex));
            ClickedAnim(clickedObj, emptyIndex);
            _signalBus.Fire<ClickSignalBus>();
            RemoveOutline(clickedObj);
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

    private void CloseBoxOutLineEffect(Transform target , GameObject obj)
    {
        if (closedBoxList.Contains(target))
        {
            if(obj.TryGetComponent<OutLineEffect>(out var outLine))
            {
                outLine.DisableOutLine();
            }

            if(obj.TryGetComponent<AlphaEffect>(out var outAlpha))
            {
                outAlpha.SetAlpha(0.6f);
            }
        }
    }

    private void RemoveAlpha(GameObject obj)
    {
        if(obj.TryGetComponent<AlphaEffect>(out var alphaEffect))
        {
            alphaEffect.FadeAlpha(1f,0.2f);
        }
    }

    private void RemoveOutline(GameObject obj)
    {
        if(obj.TryGetComponent<OutLineEffect>(out var outLine))
        {
            outLine.DisableOutLine();
        }
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
        child.transform.localScale = new Vector3(1f, 0.85f, 1f); 

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);

        seq.Append(child.transform.DOScale(Vector3.one * 1.2f, _spawnDuration * 0.5f)
            .SetEase(Ease.OutBack));
        seq.Append(child.transform.DOScale(Vector3.one * 0.97f, _spawnDuration * 0.3f)
            .SetEase(Ease.InOutSine));
        seq.Append(child.transform.DOScale(Vector3.one, _spawnDuration * 0.2f)
            .SetEase(Ease.OutSine));

        // Sonsuz minimal yoyo loop
        seq.AppendCallback(() =>
        {
            child.transform.DOScale(Vector3.one * 1.1f, _loopDuration * 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        });

        if (child.TryGetComponent<OutLineEffect>(out var alphaOutline))
        {
            alphaOutline.EnableOutLine();
        }
    }

    private void ClickedAnim(GameObject obj,int index)
    {
        Vector3 targetPos = _launcherManager.GetBoxTransform(index).position;

        obj.transform.localScale = Vector3.one * 0.9f;
        obj.transform.DORotate(Vector3.zero, 0.1f);

        float jumpPower = 1.2f;  
        float duration = 0.45f;  

        Sequence seq = DOTween.Sequence();

        seq.Append(obj.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutBack));

        seq.Append(obj.transform.DOJump(targetPos, jumpPower, 1, duration)
            .SetEase(Ease.InOutSine));

        seq.Join(obj.transform.DORotate(new Vector3(0, 0, 15f), duration / 2f)
            .SetLoops(2, LoopType.Yoyo) 
            .SetEase(Ease.InOutSine));

        seq.Append(obj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));

        seq.OnComplete(() =>
        {
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = new Vector3(0f, 0f, -0.55f);
            //obj.transform.localPosition = new Vector3(0f,0f,-0.55f);

            _launcherManager.PlaceGoalItem(obj.GetComponent<GoalItem>(), index);
            CheckAndMoveCloseBox();
        });
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
                    goalItem.transform.SetParent(openBox);

                    goalItem.transform.DOMove(openBox.position, 0.35f).SetEase(Ease.InOutQuad)
                        .OnComplete(() =>
                        {
                            goalItem.transform.localScale = Vector3.one;
                            AnimNewGoalItem(goalItem);
                        });

                    RemoveAlpha(goalItem);
                    OpenBoxClick(goalItem, openBox);
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
            goalItem.transform.SetParent(upperCloseBox);
            goalItem.transform.position = lowerCloseBox.position;

            goalItem.transform.DOMove(upperCloseBox.position, 0.35f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    goalItem.transform.localScale = Vector3.one;
                    goalItem.transform.localPosition = Vector3.zero;

                });
        }
    }
}
