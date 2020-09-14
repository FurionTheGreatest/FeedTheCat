using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class OnDestroyAddressables : MonoBehaviour
{
    public void OnDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
    }
}
