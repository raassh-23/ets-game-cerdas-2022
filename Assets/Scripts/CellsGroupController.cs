using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsGroupController : MonoBehaviour
{
    [SerializeField]
    private float _initialPoints = 50f;

    [SerializeField]
    private float _addPoints = 5f;
    
    public float Points { get; private set; }
}
