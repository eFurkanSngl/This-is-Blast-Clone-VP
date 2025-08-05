using System.Collections;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class Tile : MonoBehaviour
{
    public enum TileColor { Yellow = 0, Blue = 1, Red = 2, Pink = 3};

    [SerializeField] private MeshRenderer _mr;
    [SerializeField] private Transform _tileTransform;

    public TileColor tileColor { get; private set; }

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
    }

    public void Initialize(TileColor color)
    {
        tileColor = color;
        SpawnEffect();
    }

    private void SpawnEffect()
    {
        _tileTransform.localScale = Vector3.zero; // sadece görsel küçülür
        _tileTransform.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                _tileTransform.DOScale(Vector3.one * 1.1f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(3, LoopType.Yoyo);
            });

        //if (_materialInstance != null)
        //{
        //    Color originColor = _materialInstance.color;
        //    Color brightColor = originColor * 1.5f;
        //    brightColor.a = originColor.a;

        //    _materialInstance.DOColor(brightColor, 0.25f)
        //        .OnComplete(() =>
        //        {
        //            _materialInstance.DOColor(originColor, 0.25f);
        //        });
        //}
    }
    public void ResetTile() => tileColor = (TileColor)(-1);

    public int GetId()
    {
        return (int)tileColor;
    }
}