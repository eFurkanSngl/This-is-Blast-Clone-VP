using System.Collections.Generic;
using UnityEngine;


public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject _bulletObject;
    [SerializeField] private int _bulletCount = 20;

    private Queue<GameObject> _queue = new Queue<GameObject>();

    private void Awake()
    {
        for(int i = 0; i < _bulletCount; i++)
        {
            GameObject obj = Instantiate(_bulletObject,transform);
            obj.SetActive(false);
            _queue.Enqueue(obj);
        }
    }

    public GameObject GetBullet()
    {
        if( _queue.Count > 0)
        {
            GameObject bullet = _queue.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        GameObject newBullet = Instantiate(_bulletObject,transform);
        return newBullet;
    }


    public void ReturnPool(GameObject bullet)
    {
        _queue.Enqueue(bullet);
        bullet.SetActive(false);
    }
}