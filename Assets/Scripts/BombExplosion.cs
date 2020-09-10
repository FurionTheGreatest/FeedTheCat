using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Random = UnityEngine.Random;

public class BombExplosion : MonoBehaviour
{
    public CircleCollider2D areaToForce;
    public PointEffector2D forceField;

    public AssetReference explosionPrefab;

    private Quaternion _explosionRotation;
    private const float MaxRotationValue = 60f;

    private void OnMouseDown()
    {
        GetComponent<VFX>().DisableParticleSystem();
        GetComponent<SpriteRenderer>().enabled = false;
        
        areaToForce.enabled = true;
        forceField.enabled = true;
        
        _explosionRotation = Quaternion.Euler(0,0,Random.Range(-MaxRotationValue,MaxRotationValue));
        Addressables.LoadAssetAsync<GameObject>(explosionPrefab).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instanceHandler = Addressables.InstantiateAsync(explosionPrefab,
                    new InstantiationParameters(transform.position,_explosionRotation,null));
                var go = await instanceHandler.Task;
                StartCoroutine(WaitForEndOfEmission(go));
            }
            else
            {
                Addressables.Release(handle);
            }
            Addressables.Release(handle);
        };
    }

    private IEnumerator WaitForEndOfEmission(GameObject fxGameObject)
    {
        var particleSystems = fxGameObject.GetComponentsInChildren<ParticleSystem>();
        Assert.IsTrue(particleSystems.Length > 0);
        foreach (var ps in particleSystems)
        {
            Assert.IsFalse(ps.main.loop);

            yield return new WaitUntil(() => ps.isPlaying == false);
        }
        GetComponent<OnTouch>().DestroyObject();
    }
}
