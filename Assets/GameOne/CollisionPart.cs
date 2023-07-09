using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionPart : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.IsChildOf(transform) && !transform.IsChildOf(other.transform))
        { 
            transform.parent.GetComponent<CollisionCore>().CollisionCount++;
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.IsChildOf(transform) && !transform.IsChildOf(other.transform))
        {
            transform.parent.GetComponent<CollisionCore>().CollisionCount--;
        }
    }
}
