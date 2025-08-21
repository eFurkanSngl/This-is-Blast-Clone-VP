using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Zenject;

public class BulletManager : MonoBehaviour
{
    [Inject] private BulletPool _bulletPool;
    [Inject] private GridManager _gridManager;

    [SerializeField] private float bulletSpeed = 22f;

    [Header("Tile Effect Settings")]
    [SerializeField] private float punchScale = -0.2f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private int punchVibrato = 6;
    [SerializeField] private float punchElasticity = 0.5f;

    [SerializeField] private float pushAmount = 0.15f;
    [SerializeField] private float pushDuration = 0.12f;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask tileLayerMask = -1;
    [SerializeField] private float rayRadius = 0.5f;

    public void FireBullet(GoalItem sourceGoal, Vector3 targetPos, Action onHit = null)
    {
        GameObject bullet = _bulletPool.GetBullet();
        Transform bulletTransform = bullet.transform;

        // Spawn pozisyonu GoalItem’dan
        Vector3 startPos = sourceGoal.BulletSpawnPoint.position;
        bulletTransform.position = startPos;
        bulletTransform.localScale = Vector3.one;

        // GoalItem animasyonu
        sourceGoal.PlayShotAnim();

        // Yoldaki tile’ları bul
        List<Tile> affectedTiles = GetTilesInPath(startPos, targetPos);

        // Efekt uygula
        Vector3 bulletDir = (targetPos - startPos).normalized;
        ApplyJuiceEffectToTiles(affectedTiles, bulletDir);

        // Hedefe doğru hareket
        var move = bulletTransform.DOMove(targetPos, bulletSpeed)
            .SetSpeedBased(true)
            .SetEase(Ease.Linear);

        move.OnComplete(() =>
        {
            bulletTransform.DOKill();
            bulletTransform.rotation = Quaternion.identity;
            _bulletPool.ReturnPool(bullet);
            onHit?.Invoke();
        });
    }

    private List<Tile> GetTilesInPath(Vector3 startPos, Vector3 targetPos)
    {
        List<Tile> tilesInPath = new List<Tile>();

        Vector3 direction = (targetPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPos);

        RaycastHit[] hits = Physics.SphereCastAll(startPos, rayRadius, direction, distance, tileLayerMask);

        foreach (var hit in hits)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null && !tilesInPath.Contains(tile))
            {
                tilesInPath.Add(tile);
            }
        }

        return tilesInPath;
    }

    private void ApplyJuiceEffectToTiles(List<Tile> tiles, Vector3 bulletDir)
    {
        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;

            tile.transform.DOKill();

            Vector3 originalScale = tile.transform.localScale;

            // 1. Squash (ezilme → toparlanma)
            Vector3 squashed = new Vector3(
                originalScale.x * 0.85f,   // x küçül
                originalScale.y * 0.7f,    // y küçül
                originalScale.z * 0.85f    // z küçül
            );

            Sequence seq = DOTween.Sequence();

            seq.Append(tile.transform.DOScale(squashed, 0.08f).SetEase(Ease.OutQuad));
            seq.Append(tile.transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack));

            // 2. Yana squash’lı itme (sanki ezilip yana kayıyormuş gibi)
            Vector3 perp = Vector3.Cross(bulletDir, Vector3.forward).normalized;
            float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;

            Vector3 pushTarget = perp * (0.12f * sign);

            tile.transform
                .DOBlendableMoveBy(pushTarget, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

}
