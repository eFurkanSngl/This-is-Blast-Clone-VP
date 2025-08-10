using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoalItem : MonoBehaviour
{
    [Serializable]
    public class GoalItemData
    {
        public Tile.TileColor tileColor;
    }

    [SerializeField] private TextMeshPro _countText;
    [SerializeField] private GoalItemData _goalItemData;
    [SerializeField] private TrailRenderer trailRenderer;

    private MeshRenderer _renderer;
    private int _currentCount;
    public int CurrentCount => _currentCount;
    public TextMeshPro CounText => _countText;
    public bool IsLauncher { get; set; } = false;
    
    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if(trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        
    }
    public void EnableTrail()
    {
        if (trailRenderer != null)
            trailRenderer.emitting = true;
    }
    public void DisableTrail(float delay)
    {
        if (trailRenderer != null)
            StartCoroutine(DisableTrailDelayed(delay));
    }
    private IEnumerator DisableTrailDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        trailRenderer.emitting = false;
    }
    public void Initialize(int count)
    {
        _currentCount = count;
        UpdateText();
    }

    public int GetID() => (int)_goalItemData.tileColor;
    public Tile.TileColor GetColor() => _goalItemData.tileColor;
    private void UpdateText()
    {
        if(_countText != null)
        {
            _countText.text = _currentCount.ToString();
        }
    }

    public void DecreaseCount(int count)
    {
        _currentCount -= count;
        if(_currentCount < 0)
        {
            _currentCount = 0;
        }
        UpdateText();
    }
}
