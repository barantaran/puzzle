using UnityEngine;

public class DragAndDropPart : MonoBehaviour
{
    private Vector3 DragOffset;
    private Transform ObjectToDrag;

    private void OnMouseDown()
    {
        if (!IsDragging())
        {
            ObjectToDrag = transform;
            // Calculate the offset between the object's position and the mouse position
            if (ObjectToDrag.parent != null)
            {
                DragOffset = GetMouseWorldPosition() - ObjectToDrag.parent.position;
            } else
            {
            }

            SetDragging(true);
        }
    }

    private void SetDragging(bool isDragging)
    {
        GetComponentInParent<DragAndDropCore>().IsDragging = isDragging;
    }

    private bool IsDragging()
    {
        return GetComponentInParent<DragAndDropCore>().IsDragging;
    }

    private void OnMouseDrag()
    {
        if (IsDragging() && ObjectToDrag != null)
        {
            // Update the object's position based on the mouse position with the offset
            if (ObjectToDrag.parent != null)
            {
                ObjectToDrag.parent.position = GetMouseWorldPosition() - DragOffset;
            }
            else
            {
                ObjectToDrag.position = GetMouseWorldPosition() - DragOffset;
            }
        }
    }

    private void OnMouseUp()
    {
        SetDragging(false);
        ObjectToDrag = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Get the mouse position in screen coordinates
        Vector3 mousePosition = Input.mousePosition;

        // Set a depth for the mouse position
        mousePosition.z = 10f;

        // Convert the screen coordinates to world coordinates
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        return worldPosition;
    }
}
