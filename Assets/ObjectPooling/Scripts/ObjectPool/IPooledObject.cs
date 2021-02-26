using CustomTools.ObjectPooling.Scripts.ObjectPool;
using UnityEngine;

public interface IPooledObject
{
    PooledObjectType PoolType { get; set; }

     ObjectPooler Pooler { get; }
     
    void OnObjectSpawn();
    void OnObjectDespawn();
    void Despawn();
}
