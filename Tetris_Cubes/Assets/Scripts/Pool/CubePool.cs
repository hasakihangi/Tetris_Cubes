using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

// 通过继承的方式, 使用具体的Pool
public class CubePool: MonoBehaviour, IPool<Cube>
{
    // 引用原型预制体
    public Cube poolItem;

    public ObjectPool<Cube> pool;
        // T是PoolItem的子类, 但是不可以 ObjectPool<PoolItem> = ObjectPool<T>
        // 父类装子类不适用于泛型
        // 要么将T改成PoolItem, 那么获取到PoolItem后, 还需要一次GetComponent<Cube>
        // 拒绝继承

    private Scene poolScene;

    private void Awake()
    {
        pool = new ObjectPool<Cube>(CreatePoolItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolItem, true, 10, 200);

        // 附加场景
        SetupPoolScene();
    }

    // 创建的时候要放进新的场景
    private Cube CreatePoolItem()
    {
        Cube item = Instantiate<Cube>(poolItem);
        item.pool = pool;
        GameObject itemObject = item.gameObject;
        itemObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(itemObject, poolScene);
        return item;
    }

    private void OnTakeFromPool(Cube item)
    {
        item.gameObject.SetActive(true);
    }

    private void OnReturnToPool(Cube item)
    {
        item.gameObject.SetActive(false);
        item.transform.localScale = Vector3.one;
    }

    private void OnDestroyPoolItem(Cube item)
    {
        Destroy(item.gameObject);
    }

    private void SetupPoolScene()
    {
        poolScene = SceneManager.CreateScene(poolItem.gameObject.name + " Pool");
    }

    public Cube Get()
    {
        return pool.Get();
    }
}