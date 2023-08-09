using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class Collectible : Inhalable
{

    public static EventHandler<int> onCollectiblePickup;


    static int ID = 0;

    int id;
    [SerializeField] private GameObject plusOnePrefab;

    public override void Init()
    {
        base.Init();
        id = ID++;
        AddToInhaleQueue(new Resource("Collectible", null), .6f);
    }

    void OnPickup()
    {
        onCollectiblePickup?.Invoke(this, id);
        if (plusOnePrefab)
            Instantiate(plusOnePrefab,
                new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z),
                Quaternion.Euler(-90f, 0f, 0f));
        Destroy(gameObject);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            // Debug.Log($"Got Collectible {id}");
            OnPickup();
        }
    }
}
