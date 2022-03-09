using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    [SerializeField]
    private float _health = 100f;

    public float Health { 
        get { return _health; }
    }
}
