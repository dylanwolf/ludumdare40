using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("LudumDareResources/Utility/Object Pools")]
public class ObjectPools : MonoBehaviour
{

    static Dictionary<string, List<PoolItem>> pools = new Dictionary<string, List<PoolItem>>();
    static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public class PoolItem
    {
        public GameObject GameObject;
        public Transform Transform;
        public Behaviour Script;

        public static PoolItem Create<T>(GameObject obj) where T : Behaviour
        {
            return new PoolItem()
            {
                GameObject = obj,
                Transform = obj.transform,
                Script = obj.GetComponent<T>()
            };
        }
    }

    [System.Serializable]
    public class PoolDefinition
    {
        public string Name;
        public GameObject Prefab;
    }

    public PoolDefinition[] PoolDefinitions;

    #region Unity Lifecycle
    void Awake()
    {
        CleanupPools();
        SetupPools();
    }
    #endregion

    #region Utility Methods
    void SetupPools()
    {
        for (int i = 0; i < PoolDefinitions.Length; i++)
        {
            if (!pools.ContainsKey(PoolDefinitions[i].Name))
                pools[PoolDefinitions[i].Name] = new List<PoolItem>();

            if (!prefabs.ContainsKey(PoolDefinitions[i].Name))
                prefabs[PoolDefinitions[i].Name] = PoolDefinitions[i].Prefab;
        }
    }

    void CleanupPools()
    {
        int i = 0;
        foreach (var pool in pools.Values)
        {
            i = 0;
            while (i < pools.Count)
            {
                if (pool[i] == null)
                    pool.RemoveAt(i);
                else
                    i++;
            }
        }
    }
    #endregion

    static PoolItem tmpPI;
    public static T Spawn<T>(string poolName, Vector3 position, Transform parent) where T : Behaviour
    {
        // Try to get the object from the pool
        foreach (var pi in pools[poolName])
        {
            if (!pi.GameObject.activeSelf)
            {
                pi.Transform.parent = parent;
                pi.Transform.position = position;
                pi.GameObject.SetActive(true);
                return pi.Script as T;
            }
        }

        // Otherwise, instanitate
        tmpPI = PoolItem.Create<T>((GameObject)Instantiate(prefabs[poolName]));
        tmpPI.Transform.parent = parent;
        tmpPI.Transform.position = position;
        pools[poolName].Add(tmpPI);
        return tmpPI.Script as T;
    }

    public static void Despawn(GameObject obj)
    {
        obj.SetActive(false);
    }
}
