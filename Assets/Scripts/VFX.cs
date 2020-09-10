using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour
{
    public GameObject particleFx;
    private bool _isParticleSystemNotNull;
    
    private void Start()
    {
        particleFx = gameObject.GetComponentInChildren<ParticleSystem>(true).gameObject;
        _isParticleSystemNotNull = particleFx != null;
    }

    public void DisableParticleSystem()
    {
        if(_isParticleSystemNotNull)
            particleFx.SetActive(false);
    }

    public bool IsVfxEnabled()
    {
        return particleFx.activeInHierarchy;
    }
}
