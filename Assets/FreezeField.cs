using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class FreezeField : MonoBehaviour
{
    public AssetReference freezeExplosion;
    public AssetReference icePrefab;
    public float freezeRadius;
    public LayerMask mask;
    public Collider2D[] results;

    public void Freeze()
    {
        GetComponent<VFX>().DisableParticleSystem();
        GetComponent<SpriteRenderer>().enabled = false;
        
        Addressables.LoadAssetAsync<GameObject>(freezeExplosion).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instanceHandler = Addressables.InstantiateAsync(freezeExplosion,
                    new InstantiationParameters(transform.position,Quaternion.identity, null));
                var go = await instanceHandler.Task;
                StartCoroutine(WaitForEndOfEmission(go));
            }
            else
            {
                Addressables.Release(handle);
            }
            Addressables.Release(handle);
        };
        results = new Collider2D[FoodSupplyManager.Instance.foodOnScene.Count];
        Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, freezeRadius,results,mask);
        if (results == null) return;
        foreach (var food in results)
        {
            if(food != null)
                InstantiateIce(food.transform);
        }
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

    private void InstantiateIce(Transform parent)
    {
        Addressables.LoadAssetAsync<GameObject>(icePrefab).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instanceHandler = Addressables.InstantiateAsync(icePrefab,
                    new InstantiationParameters(parent.transform.position, parent.transform.rotation, parent));
                var go = await instanceHandler.Task;
                go.GetComponentInParent<Collectible>().touchCounter++;
                go.GetComponentInParent<Collectible>().iceAnimators.Add(go.GetComponent<Animator>());
            }
            else
            {
                Addressables.Release(handle);
            }
            Addressables.Release(handle);
        };
    }
}
    
    

