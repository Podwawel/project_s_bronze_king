using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<TInstance> : MonoBehaviour where TInstance : Singleton<TInstance>
{
    public static TInstance instance;

    protected virtual void Awake()
    {
        if (!instance) instance = this as TInstance;
        else Destroy(gameObject);
    }
}
