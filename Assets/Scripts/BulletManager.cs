using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BulletManager : MonoBehaviour
{
    [Inject] private BulletPool _bulletPool;
    [Inject] private GridManager _gridManager;

    [SerializeField] private float bulletSpeed = 70f;

    [Header("Tile Effect Settings")]
    [SerializeField] private float pushDistance = 0.12f;
    [SerializeField] private float squashDuration = 0.08f;
    [SerializeField] private float restoreDuration = 0.15f;

    [Header("Movement Settings")]
    [SerializeField] private float stepSize = 0.6f; 
    [SerializeField] private LayerMask tileLayerMask;

    public void FireBullet(GoalItem sourceGoal, Vector3 targetPos, Action onHit = null)
    {
        GameObject bullet = _bulletPool.GetBullet();
        Transform transform = bullet.transform;

        transform.position = sourceGoal.BulletSpawnPoint.position;
        transform.localScale = Vector3.one;

        sourceGoal.PlayShotAnim();

        StartCoroutine(MoveBulletCoroutine(transform, targetPos, onHit));
    }

    private IEnumerator MoveBulletCoroutine(Transform bullet, Vector3 targetPos, Action onHit)
    {
        Vector3 start = bullet.position;
        Vector3 dir = (targetPos - start).normalized;
        float totalDistance = Vector3.Distance(start, targetPos);
        float traveled = 0f;

        HashSet<Tile> affectedTiles = new HashSet<Tile>();

        while (traveled < totalDistance)
        {
            float moveStep = stepSize;
            if (traveled + moveStep > totalDistance)
                moveStep = totalDistance - traveled;

            bullet.position += dir * moveStep;
            traveled += moveStep;

            Collider[] hits = Physics.OverlapSphere(bullet.position, 0.3f, tileLayerMask);
            foreach (var hit in hits)
            {
                Tile tile = hit.GetComponent<Tile>();
                if (tile != null && !affectedTiles.Contains(tile))
                {
                    affectedTiles.Add(tile);
                    ApplyTileSquashPush(tile.transform, dir);
                }
            }

            yield return null;
        }

        bullet.DOKill();
        _bulletPool.ReturnPool(bullet.gameObject);
        onHit?.Invoke();
    }

    private void ApplyTileSquashPush(Transform tile, Vector3 bulletDir)
    {
        tile.DOKill();

        Vector3 originalScale = Vector3.one;
        Vector3 originalPos = tile.localPosition;

        Vector3 squashedScale = new Vector3(0.75f, 1.25f, 1f);

        Vector3 perp = Vector3.Cross(bulletDir, Vector3.forward).normalized;
        float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        Vector3 pushOffset = perp * 0.14f;

        Vector3 bounceScale = new Vector3(1.08f, 0.95f, 1f); 

        Sequence seq = DOTween.Sequence();

        seq.Append(tile.DOScale(squashedScale, 0.01f).SetEase(Ease.OutQuad));
        seq.Join(tile.DOLocalMove(originalPos + pushOffset, 0.01f).SetEase(Ease.OutQuad));

        seq.Append(tile.DOScale(bounceScale, 0.12f).SetEase(Ease.OutBack));
        seq.Join(tile.DOLocalMove(originalPos, 0.12f).SetEase(Ease.OutBack));

        seq.Append(tile.DOScale(originalScale,0.01f).SetEase(Ease.OutSine));

        seq.OnComplete(() =>
        {
            tile.localScale = originalScale;
            tile.localPosition = originalPos;
        });
    }





}
