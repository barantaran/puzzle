using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCore : MonoBehaviour
{
    Vector3 LastFreePosition;

    public int CollisionCount = 0;
    private DragAndDropCore DragAndDropData;
    Stack<Vector3> FreePositions;

    // Start is called before the first frame update
    void Start()
    {
        FreePositions = new Stack<Vector3>();
        //Cashes access to DragAndDropCore
        DragAndDropData = GetComponent<DragAndDropCore>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    int FigureCount()
    {
        return transform.parent.childCount + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.IsChildOf(transform) && !transform.IsChildOf(other.transform))
        {
            CollisionCount++;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.IsChildOf(transform) && !transform.IsChildOf(other.transform))
        {
            CollisionCount--;

        }
    }

    private void LateUpdate()
    {
        if (CollisionCount > 0 && !DragAndDropData.ReturnToFreePosition)
        {
            DragAndDropData.IsDragging = false;
            DragAndDropData.ReturnToFreePosition = true;
        }
        else if(CollisionCount == 0 && DragAndDropData.IsDragging)
        {
            FreePositions.Push(transform.position);
        }
        else if(CollisionCount > 0 && DragAndDropData.ReturnToFreePosition)
        {
            if (FreePositions.Count == 0)
            {
                DragAndDropData.ReturnToFreePosition = false;
                return;
            }

            transform.position = FreePositions.Pop();
        }
        else if(CollisionCount == 0 && DragAndDropData.ReturnToFreePosition)
        {
            DragAndDropData.ReturnToFreePosition = false;
            FreePositions.Clear();
        }
    }
}
