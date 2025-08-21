using System.Collections.Generic;
using UnityEngine;

public class SplashPool : MonoBehaviour
{
    [SerializeField] private GameObject _splashPrefab;
    [SerializeField] private int _splashCount;

    private Queue<GameObject> _splashPool = new Queue<GameObject>();
    private void Awake()
    {
        for(int i  = 0; i < _splashCount; i++)
        {
            GameObject gameObject = Instantiate(_splashPrefab,transform);
            _splashPool.Enqueue(gameObject);
            gameObject.SetActive(false);
        }
    }

    public GameObject GetSplash()
    {
        if(_splashPool.Count > 0)
        {
            GameObject splash = _splashPool.Dequeue();
            splash.SetActive(true);
            return splash;
        }
       GameObject splash1 = Instantiate(_splashPrefab,transform);
        return splash1;
        
    }

    public void ReturnSplash(GameObject gameObject)
    {
        _splashPool.Enqueue(gameObject);
        gameObject.SetActive(false);
    }
}