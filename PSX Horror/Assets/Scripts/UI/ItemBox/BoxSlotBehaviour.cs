using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoxSlotBehaviour : MonoBehaviour, IScrollHandler
{
    public ItemBase currentItem;
    private Image image;
    private Text text;

    public bool isEmpty;

    ScrollUpdateY scroll;

    // Start is called before the first frame update
    void Start()
    {
        scroll = GetComponentInParent<ScrollUpdateY>();

        image = transform.GetChild(2).GetComponent<Image>();
        text = image.transform.GetChild(0).GetComponent<Text>();
    }

    public void Update()
    {
        if (!image || !text) return;

        isEmpty = currentItem == null ? true : false;
        image.enabled = !isEmpty;
        text.enabled = !isEmpty;

        if (currentItem != null)
        {
            image.sprite = currentItem.icon;
            image.type = Image.Type.Sliced;
            if (currentItem.isStock)
            {
                text.text = string.Format("{0}", currentItem.amount);

                text.color = (currentItem.amount == currentItem.amountLimit) ? Color.green : Color.white;
            }
            else
            {
                if (currentItem.type == ItemType.Weapon && currentItem.GetComponent<WeaponBase>().weaponType != WeaponType.Melee)
                {
                    text.text = string.Format("{0}", currentItem.GetComponent<WeaponBase>().currentAmmo);

                    text.color = (currentItem.GetComponent<WeaponBase>().currentAmmo ==
                        currentItem.GetComponent<WeaponBase>().capacityAmmo) ? Color.green : Color.white;

                    text.enabled = true;
                }
                else
                    text.enabled = false;
            }

            if (currentItem.isStock && currentItem.amount <= 0)
            {
                Destroy(currentItem.gameObject);
                currentItem = null;
            }
        }
    }

    public void Interact()
    {
        if (currentItem)
        {
            ItemBox box = ItemBox.instance;
            InventoryUI.instance.PlayAcceptAudio();
            if(box.selectedSlot)
                box.selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;

            box.selectedSlot = this;
            GetComponent<Image>().color = InventoryUI.instance.colors.selectingColor;
            GameManager.instance.ChangeSelected(box.SearchAvailableSlot(currentItem).gameObject);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        scroll.MoveScroll();
    }
}