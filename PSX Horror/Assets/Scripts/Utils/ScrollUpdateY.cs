using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollUpdateY : MonoBehaviour
{
    RectTransform scrollRectTransform;
    ScrollRect scrollRect;
    RectTransform content;

    RectTransform selectedRectTransform;
    GameObject lastSelected;

    public float speed = 5f;

    // Start is called before the first frame update
    public void Start()
    {
        scrollRectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    // Update is called once per frame
    public void Update()
    {
        // Get the currently selected UI element from the event system.
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        //return conditions
        if (selected == null || selected.transform.parent != content.transform ||
            Cursor.lockState == CursorLockMode.None && Cursor.visible == true) return;
        //
        SmoothlySnap();
    }

    public void MoveScroll()
    {
        float scroll = Input.GetAxisRaw("Scroll");
        scrollRect.verticalNormalizedPosition += speed * 7 * -scroll * Time.unscaledDeltaTime;
    }

    public void SmoothlySnap()
    {
        // Get the currently selected UI element from the event system.
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // The upper bound of the scroll view is the anchor position of the content we're scrolling.
        float scrollViewMinY = content.anchoredPosition.y;
        // The lower bound is the anchor position + the height of the scroll rect.
        float scrollViewMaxY = content.anchoredPosition.y + scrollRectTransform.rect.height;

        // Get the rect tranform for the selected game object.
        selectedRectTransform = selected.GetComponent<RectTransform>();
        //tamanho do selected game object
        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + selectedRectTransform.rect.height + 10;


        // If the selected position is below the current lower bound of the scroll view we scroll down.
        if (selectedPositionY > scrollViewMaxY && scrollRect.verticalNormalizedPosition > 0)
            scrollRect.verticalNormalizedPosition -= speed * Time.unscaledDeltaTime;
        // If the selected position is above the current upper bound of the scroll view we scroll up.
        else if ((Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY) && scrollRect.verticalNormalizedPosition < 1)
            scrollRect.verticalNormalizedPosition += speed * Time.unscaledDeltaTime;

        lastSelected = selected;
    }

    public void SnapTo(GameObject selected)
    {
        // The upper bound of the scroll view is the anchor position of the content we're scrolling.
        float scrollViewMinY = content.anchoredPosition.y;
        // The lower bound is the anchor position + the height of the scroll rect.
        float scrollViewMaxY = content.anchoredPosition.y + scrollRectTransform.rect.height;

        // Get the rect tranform for the selected game object.
        selectedRectTransform = selected.GetComponent<RectTransform>();
        //tamanho do selected game object
        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + selectedRectTransform.rect.height + 10;

        // If the selected position is below the current lower bound of the scroll view we scroll down.
        if (selectedPositionY > scrollViewMaxY && scrollRect.verticalNormalizedPosition > 0)
        {
            float newY = selectedPositionY - scrollRectTransform.rect.height;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);
        }
        // If the selected position is above the current upper bound of the scroll view we scroll up.
        else if ((Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY) && scrollRect.verticalNormalizedPosition < 1)
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Abs(selectedRectTransform.anchoredPosition.y));

        lastSelected = selected;
    }
}