
using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public interface IPool<T> where T: MonoBehaviour
{
    T Get();
}

public interface IPooledObject
{
    void Release();
}

public abstract class PoolItem<T> : MonoBehaviour, IPooledObject where T: PoolItem<T>
{
    protected IObjectPool<T> pool;

    public void SetReturnToPool(IObjectPool<T> _pool) 
    {
        pool = _pool;
    }

    public abstract void Release();
}

public abstract class Pool<T> : MonoBehaviour, IPool<T> where T: PoolItem<T>
{
    [SerializeField]
    protected T poolItem;
    protected ObjectPool<T> pool;
    protected Scene poolScene;

    protected bool collectionCheck = true;
    protected int defaultCapacity = 10;
    protected int maxSize = 50;
    
    protected virtual void Awake()
    {
        SetupPoolScene();
        pool = new ObjectPool<T>(CreatePoolItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolItem, collectionCheck, defaultCapacity, maxSize);
    }

    private void SetupPoolScene()
    {
        poolScene = SceneManager.CreateScene(poolItem.gameObject.name + " Pool");
    }
    
    protected virtual T CreatePoolItem()
    {
        T item = Instantiate<T>(poolItem);
        item.SetReturnToPool(pool); 
        GameObject itemObject = item.gameObject;
        itemObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(itemObject, poolScene);
        return item;
    }

    protected virtual void OnTakeFromPool(T item)
    {
        item.gameObject.SetActive(true);
    }

    protected virtual void OnReturnToPool(T item)
    {
        item.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyPoolItem(T item)
    {
        Destroy(item.gameObject);
    }

    public virtual T Get()
    {
        return pool.Get();
    }
}