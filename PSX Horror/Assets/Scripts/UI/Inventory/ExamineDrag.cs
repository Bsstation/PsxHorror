using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExamineDrag : MonoBehaviour, IDragHandler
{
    InventoryUI inventory;

    public void OnDrag(PointerEventData eventData)
    {
        RotateExamineObject();
    }

    public void RotateExamineObject()
    {
        inventory = InventoryUI.instance;

        if (inventory.examing)
        {
            Vector2 velocity = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            float xAxis = velocity.x * inventory.speedExamination * Time.unscaledDeltaTime;
            float yAxis = velocity.y * inventory.speedExamination * Time.unscaledDeltaTime;

            inventory.examinePivot.Rotate(Vector3.up, -xAxis, Space.World);
            inventory.examinePivot.Rotate(Vector3.right, yAxis, Space.World);
        }
    }
}
