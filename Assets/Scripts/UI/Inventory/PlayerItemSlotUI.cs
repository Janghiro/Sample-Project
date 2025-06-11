using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
                     PlayerItemSlotUI

                - 플레이어 아이템 슬롯 관리      
                - 슬롯에 아이템을 가져다놓으면 아이템을 등록
                    - 인벤토리에서 아이템을 사용하거나 제거하면 반영
 */

public class PlayerItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text amount;
    public int index;

    private CountableItem slotItem;             // 이 슬롯의 아이템


    private void ShowAmount() => amount.gameObject.SetActive(true);
    private void HideAmount() => amount.gameObject.SetActive(false);

    // 슬롯 업데이트
    public void UpdateSlot()
    {
        SetItem(slotItem);
    }

    // 슬롯에 아이템 등록(아이콘이미지, 수량텍스트)
    public void SetItem(CountableItem item)
    {
        if (item == null)
        {
            return;
        }

        slotItem = item;

        if (slotItem.Amount > 1)
            ShowAmount();
        else
            HideAmount();

        amount.text = item.Amount.ToString();
    }

    // 슬롯의 아이템 제거
    public void RemoveItem()
    {
        icon.sprite = null;
        icon.color = new Color(1f, 1f, 1f, 0f);
        slotItem = null;
        HideAmount();
    }

    // 해당 아이템이 등록되어있는지 여부
    public bool HasItem(CountableItem ci)
    {
        return slotItem == ci;
    }
}
