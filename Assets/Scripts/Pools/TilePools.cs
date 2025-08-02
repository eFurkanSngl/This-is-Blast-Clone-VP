using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour
{
    [Serializable]
    public class TilePrefabData
    {
        public int tileID;
        public GameObject prefab;
        public int poolSize;
    }

    [SerializeField] private List<TilePrefabData> _tiledata;

    private Dictionary<int, Queue<GameObject>> _tilePools = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, int> tileCount = new Dictionary<int, int>();

    public Dictionary<int, int> TileCount => tileCount;

    private void Awake()
    {
        foreach (var data in _tiledata)
        {
            int id = data.tileID;
            GameObject prefab = data.prefab;
            int size = data.poolSize;

            Queue<GameObject> pools = new Queue<GameObject>();

            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pools.Enqueue(obj);
            }

            _tilePools.Add(id, pools);
            tileCount.Add(id, size);
        }
    }

    public GameObject GetTile(int id)
    {
        if (!_tilePools.ContainsKey(id))
        {
            Debug.LogError("Tile pool does not contain ID");
            return null;
        }

        Queue<GameObject> pool = _tilePools[id];

        if (pool.Count == 0)
        {
            Debug.Log("Pool empty");
            return null;
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnTile(int id, GameObject tile)
    {
        tile.SetActive(false);
        tile.transform.SetParent(transform);
        _tilePools[id].Enqueue(tile);
    }

    public int GetCount(int id)
    {
        if (_tilePools.ContainsKey(id))
        {
            return _tilePools[id].Count;
        }
        return -1;
    }
}
