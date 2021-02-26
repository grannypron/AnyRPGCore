using CustomTools.ObjectPooling.Scripts.ObjectPool;
using UnityEngine;

public class PooledObject : MonoBehaviour, IPooledObject
{

    public PooledObjectType PoolType { get; set; }
    public ObjectPooler Pooler { get; set; }

    public virtual void OnObjectSpawn()
    {

    }

    public virtual void OnObjectDespawn()
    {

    }

    public void Despawn()
    {
        Pooler.Despawn(gameObject);
    }


}
