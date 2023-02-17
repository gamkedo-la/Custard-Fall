using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[CreateAssetMenu(menuName = "CustardFall/PlaceableItem")]
public class PlaceableItem : ScriptableObject
{
    [SerializeField] private GameObject prototype;
    [SerializeField] private GameObject placeablePreview;
    [SerializeField] private Sprite icon;

    public GameObject Prototype => prototype;
    public GameObject PlaceablePreview => placeablePreview;
    public Sprite Icon => icon;
}