using UnityEngine;
using DG.Tweening;
using System;

public class Tile : MonoBehaviour, ITileAnim
{
    public enum TileColor { Yellow = 0, Blue = 1, Red = 2, Pink = 3 };

    [SerializeField] private MeshRenderer _mr;
    [SerializeField] private Transform _tileTransform;
    public TileColor tileColor { get; private set; }

    [Header("Referans")]
    [SerializeField] private GameObject _baseLayer;
    [SerializeField] private GameObject _topLayer;
    private int _layerHealth = 1;

    [SerializeField] private float totalDuration = 0.03f; 
    [SerializeField] private Ease easeType = Ease.InBack;

    private bool _isBeingDestroyed;
    private bool _topLayerDestroyed = false;
    private bool _baseLayerDestroyed = false;

    public Vector2Int GridPos { get; private set; }

    public bool HasTopLayerOnly() => _topLayer != null && _topLayer.activeSelf && !_topLayerDestroyed;
    public bool HasBaseLayerOnly() => _baseLayer != null && _baseLayer.activeSelf && !_baseLayerDestroyed;
    public bool IsCompletelyDestroyed() => _topLayerDestroyed && _baseLayerDestroyed;
    public bool ShouldHitTopLayer() => HasTopLayerOnly();

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        if (_topLayer != null)
            _topLayer.transform.localPosition = new Vector3(0f, 0f, -0.8f);
    }
    public void SetGridPos(int row, int col)
    {
        GridPos = new Vector2Int(col, row);
    }

    public void PlayDestroyAnim(Transform target, Action onComplete = null)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }

        target.DOScale(1.1f, totalDuration * 0.05f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(target.DOScale(0f, totalDuration * 0.05f)
                    .SetEase(easeType));
                seq.OnComplete(() =>
                {
                    target.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
            });
    }

    public void HitLayer(bool hitTop, Action onComplete = null)
    {
        if (_isBeingDestroyed)
        {
            onComplete?.Invoke();
            return;
        }

        _isBeingDestroyed = true;

        if (hitTop && HasTopLayerOnly())
        {
            _topLayerDestroyed = true; 
            PlayDestroyAnim(_topLayer.transform, () =>
            {
                _topLayer.SetActive(false);
                _isBeingDestroyed = false;
                onComplete?.Invoke();
            });
        }
        else if (!hitTop && HasBaseLayerOnly())
        {
            _baseLayerDestroyed = true;
            PlayDestroyAnim(_baseLayer.transform, () =>
            {
                _baseLayer.SetActive(false);
                _isBeingDestroyed = false;
                onComplete?.Invoke();
            });
        }
        else
        {
            _isBeingDestroyed = false;
            onComplete?.Invoke();
        }
    }

    public void Initialize(TileColor color, int layerHealth = 1)
    {
        tileColor = color;
        _layerHealth = Mathf.Max(1, layerHealth);

        // Reset flags
        _topLayerDestroyed = false;
        _baseLayerDestroyed = false;
        _isBeingDestroyed = false;

        if (_baseLayer != null)
        {
            _baseLayer.SetActive(true);
            _baseLayer.transform.localScale = Vector3.one;
        }

        if (_topLayer != null)
        {
            _topLayer.SetActive(true);
            _topLayer.transform.localScale = Vector3.one;
            _topLayer.transform.localPosition = new Vector3(0f,1f,0f);
        }
    }

    public void ResetTile()
    {
        tileColor = (TileColor)(-1);
        _topLayerDestroyed = false;
        _baseLayerDestroyed = false;
        _isBeingDestroyed = false;
    }

    public int GetId() => (int)tileColor;
}