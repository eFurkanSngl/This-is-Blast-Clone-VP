using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

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
    private BoxCollider _boxCollider;
    private MeshRenderer _renderer;
    private int _currentCount;
    public int CurrentCount => _currentCount;
    public TextMeshPro CounText => _countText;
    public bool IsLauncher { get; set; } = false;

    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private Transform _splashTransform;
    public Transform BulletSpawnPoint => _bulletSpawnPoint;
    [SerializeField] private SplashPool _splashPool;
    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if(trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        _boxCollider = GetComponent<BoxCollider>();
    }
    private void ShootSplash()
    {
        if (_splashTransform != null)
        {
            GameObject splash = _splashPool.GetSplash();
            splash.transform.position = _splashTransform.position;

            splash.transform.DOScale(0.5f, 0.15f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _splashPool.ReturnSplash(splash);
                });
        }
    }
    public void PlayShotAnim()
    {
        Vector3 baseScale = Vector3.one;

        Vector3 squashed = new Vector3(
            baseScale.x,
            baseScale.y * 0.85f,
            baseScale.z
        );

        transform.DOScale(squashed, 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(baseScale, 0.1f)
                         .SetEase(Ease.OutQuad);
            });
        ShootSplash();
    }


    public BoxCollider GetCollider()
    {
        return _boxCollider;
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
