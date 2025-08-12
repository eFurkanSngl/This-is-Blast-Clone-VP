using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Zenject; // Zenject kullanıyorsan

public class BulletManager : MonoBehaviour
{
    [Inject] private BulletPool _bulletPool;

    [SerializeField] private float bulletSpeed = 22f;   // mermi hızı (speed-based)
    [SerializeField] private float pushAmount = 0.12f;  // yan itme mesafesi
    [SerializeField] private float pushDuration = 0.08f;

    /// <summary>
    /// Mermiyi başlatır, yol üstündeki tile’ları ittirir.
    /// </summary>
    public void FireBullet(Vector3 startPos, Vector3 targetPos, IReadOnlyList<Transform> tilesToPush, Action onHit = null)
    {
        GameObject bullet = _bulletPool.GetBullet();
        var t = bullet.transform;

        t.position = startPos;
        t.localScale = Vector3.one;

        // Yön ve hız bazlı hareket
        var move = t.DOMove(targetPos, bulletSpeed)
                    .SetSpeedBased(true)
                    .SetEase(Ease.Linear);

        // Yan itme yönü (dikey/ yatay fark etmez, hedefe dik)
        Vector3 dir = (targetPos - startPos).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;

        foreach (var tile in tilesToPush)
        {
            if (tile == null) continue;

            float distToTile = Vector3.Distance(startPos, tile.position);
            float delay = distToTile / bulletSpeed; // merminin oraya varma süresi

            float sign = (UnityEngine.Random.value > 0.5f) ? 1f : -1f; // rastgele sağ/sol itme

            tile.transform
                .DOBlendableMoveBy(perp * (pushAmount * sign), pushDuration)
                .SetDelay(Mathf.Max(0f, delay - pushDuration * 0.5f))
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }

        // Hedefe ulaştığında yok et
        move.OnComplete(() =>
        {
            _bulletPool.ReturnPool(bullet);
            onHit?.Invoke();
        });
    }
}
