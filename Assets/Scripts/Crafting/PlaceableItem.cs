using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(menuName = "CustardFall/PlaceableItem")]
public class PlaceableItem : ScriptableObject
{
    [SerializeField] private GameObject Prototype;
    [SerializeField] private GameObject PlaceablePreview;
    [SerializeField] private Sprite icon { get; }
    
    public Sprite Icon => icon;
}