using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[Serializable]
public class TreasureHunterInventoryA4 : MonoBehaviour
{
    [Serializable]
    public class myDictionary : SerializableDictionary<collectible, int> {}
    public myDictionary itemsCollected;   
}
