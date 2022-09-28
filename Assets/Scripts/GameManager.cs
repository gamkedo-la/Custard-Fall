using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour
{
    public TerrainLoader terrainLoader;
    public CustardManager custardManager;
    
    private void Start()
    {
        terrainLoader.LoadWorld();
        custardManager.InitCustardState();
    }
    
}