using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RedefineInputs : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.instance)
        {
            InputManager.instance.Default();

            InputMenu inputMenu = FindObjectOfType(typeof(InputMenu)) as InputMenu;
            inputMenu.Start();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
