using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[Serializable]
public class TreasureHunterInventory : MonoBehaviour
{
    [Serializable]
    public class myDictionary : SerializableDictionary<collectible, int> {}
    public myDictionary itemsCollected;   
}
