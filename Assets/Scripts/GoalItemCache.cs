using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalItemCache : MonoBehaviour
{
    [SerializeField] private AlphaEffect _alphaEffect;
    [SerializeField] private OutLineEffect _outLineEffect;
    [SerializeField] private GoalItem _goalItem;
    [SerializeField] private GoalItemClickHandler _goalItemClickHandler;

    public AlphaEffect alphaEffect => _alphaEffect;
    public OutLineEffect outLineEffect => _outLineEffect;
    public GoalItem goalItem => _goalItem;

    public GoalItemClickHandler goalItemClickHandler => _goalItemClickHandler;

}
