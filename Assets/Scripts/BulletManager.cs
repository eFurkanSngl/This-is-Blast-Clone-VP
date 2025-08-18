using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Zenject; // Zenject kullanıyorsan

public class BulletManager : MonoBehaviour
{
    [Inject] private BulletPool _bulletPool;


    [SerializeField] private float bulletSpeed = 22f; 
    [SerializeField] private float bounceScale = 1.15f;
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private float wiggleAngle = 8f; 
    [SerializeField] private float wiggleDuration = 0.1f; 
 
    public void FireBullet(Vector3 startPos, Vector3 targetPos, Action onHit = null)
    {
        GameObject bullet = _bulletPool.GetBullet();
        Transform transform = bullet.transform;

        transform.position = startPos;
        transform.localScale = Vector3.one;

        transform.DOScale(Vector3.one * bounceScale, bounceDuration)
         .SetLoops(2, LoopType.Yoyo)
         .SetEase(Ease.OutQuad);

        transform.DORotate(new Vector3(0, 0, wiggleAngle), wiggleDuration)
         .SetLoops(-1, LoopType.Yoyo)
         .SetEase(Ease.InOutSine);

        // Düz hedefe hareket
        var move = transform.DOMove(targetPos, bulletSpeed)
                    .SetSpeedBased(true)
                    .SetEase(Ease.Linear);

        move.OnComplete(() =>
        {
            transform.DOKill(); // Animasyonları durdur
            transform.rotation = Quaternion.identity;
            _bulletPool.ReturnPool(bullet);
            onHit?.Invoke();
        });
    }
}

