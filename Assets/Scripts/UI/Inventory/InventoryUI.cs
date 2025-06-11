using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
                        InventoryUI

            - 인벤토리 각 슬롯들을 모두 관리
            - 인벤토리 영역에 슬롯 생성
            - 마우스 이벤트 처리
            - Inventory <-> ItemSlotUI 중간다리역할
*/
public class InventoryUI : MonoBehaviour
{
    #region ** Serialized Fields **
    [SerializeField] private RectTransform contentAreaRT;   // 아이템 영역
    [SerializeField] private GameObject itemSlotPrefab;     // 복제할 원본 슬롯 프리팹
    [SerializeField] private InventoryPopupUI popup;        // 팝업 UI
    [SerializeField] private ItemTooltipUI itemTooltipUI;
    [SerializeField] private UIRaycaster rc;                // 레이캐스터
    #endregion

    #region ** 인벤토리 옵션 **
    private int horizontalSlotCount = 6;                    // 슬롯 가로갯수
    private int verticalSlotCount = 6;                      // 슬롯 세로갯수
    private float slotMargin = 9f;                          // 슬롯 여백
    private float contentAreaPadding = 8f;                  // 인벤토리 영역 내부 여백
    private float slotSize = 81f;                           // 슬롯 사이즈
    private bool showHighlight = true;                      // 하이라이트 보이기
    #endregion

    #region ** Fields **
    private Inventory inventory;                            // 연결된 인벤토리
    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();

    private GraphicRaycaster gr;
    private PointerEventData ped;
    private List<RaycastResult> rrList;

    private ItemSlotUI pointerOverSlot;                     // 현재 마우스 포인터가 위치한 곳의 슬롯
    private ItemSlotUI beginDragSlot;                       // 마우스 드래그를 시작한 슬롯
    private Transform beginDragIconTransform;               // 마우스 드래그를 시작한 슬롯의 위치

    private int leftClick = 0;                              // 좌클릭 = 0
    private int rightClick = 1;                             // 우클릭 = 0;

    private Vector3 beginDragIconPoint;                     // 마우스 드래그를 시작한 아이콘 위치
    private Vector3 beginDragCursorPoint;                   // 마우스 드래그를 시작한 커서 위치
    private int beginDragSlotSiblingIndex;                  // 마우스 드래그를 시작한 슬롯의 SiblingIdx
    #endregion

    #region ** 유니티 이벤트 함수 **
    private void Awake()
    {
        Init();
        InitSlots();
    }

    private void Start()
    {
        HideUI();
    }

    private void Update()
    {
        OnPointerEnterAndExit();
        ShowOrHideTooltipUI();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();
    }

    #endregion

    #region ** Private Methods **
    // 초기화
    private void Init()
    {
        if(itemTooltipUI == null)
        {
            itemTooltipUI = GetComponentInChildren<ItemTooltipUI>();
        }
    }

    // 슬롯 영역 내에 슬롯 생성
    private void InitSlots()
    {
        // 슬롯 프리팹 설정
        itemSlotPrefab.TryGetComponent(out RectTransform slotRect);

        // 슬롯 사이즈 설정
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);

        // 슬롯에 ItemSlotUI 스크립트 붙이기
        itemSlotPrefab.TryGetComponent(out ItemSlotUI itemSlot);
        if (itemSlot == null)
            itemSlotPrefab.AddComponent<ItemSlotUI>();

        // 원본 슬롯 비활성화
        itemSlotPrefab.SetActive(false);

        // 슬롯을 채울 시작위치, 현재위치
        Vector2 beginPos = new Vector2(contentAreaPadding, -contentAreaPadding);
        Vector2 curPos = beginPos;

        // 아이템 슬롯들을 담을 리스트
        slotUIList = new List<ItemSlotUI>(verticalSlotCount * horizontalSlotCount);

        // 슬롯 생성하기
        for (int j = 0; j < verticalSlotCount; j++)
        {
            for (int i = 0; i < horizontalSlotCount; i++)
            {
                int slotIndex = (horizontalSlotCount * j) + i;       // 슬롯 인덱스

                // 슬롯하나 복제
                var slotRT = CloneSlot();
                // 복제된 슬롯정보 설정
                slotRT.pivot = new Vector2(0f, 1f);                  // Left Top 기준
                slotRT.anchoredPosition = curPos;
                slotRT.gameObject.SetActive(true);
                slotRT.gameObject.name = $"Item Slot [{slotIndex}]"; // 하이어라키상 슬롯이름("Item Slot 0~35")

                ItemSlotUI slotUI = slotRT.GetComponent<ItemSlotUI>();
                slotUI.SetSlotIndex(slotIndex);                      // 슬롯에 인덱스붙이기
                slotUIList.Add(slotUI);                              // 리스트에 생성된 슬롯정보 추가

                // 다음칸(가로)
                curPos.x += (slotMargin + slotSize);
            }

            // 다음줄(세로)
            curPos.x = beginPos.x;
            curPos.y -= (slotMargin + slotSize);
        }

        // 슬롯 원본파괴
        if (itemSlotPrefab.scene.rootCount != 0)
            Destroy(itemSlotPrefab);

        RectTransform CloneSlot()
        {
            GameObject slotGo = Instantiate(itemSlotPrefab);
            RectTransform rt = slotGo.GetComponent<RectTransform>();
            rt.SetParent(contentAreaRT);

            return rt;
        }
    }

    // 인벤토리 UI 비활성화
    private void HideUI()
    {
        this.gameObject.SetActive(false);
    }

    // 두 슬롯 아이템 교환
    private void TrySwapItems(ItemSlotUI begin, ItemSlotUI end)
    {
        // 자기자신 처리
        if (begin == end) return;

        begin.SwapOrMoveIcon(end);
        inventory.Swap(begin.Index, end.Index);
    }

    // 아이템 버리기 요청
    private void TryRemoveItem(int index)
    {
        inventory.Remove(index);
    }

    // 아이템 사용 및 장착
    private void TryUseItem(int index)
    {
        inventory.Use(index);
    }

    // 플레이어 아이템창에 아이템 등록
    private void TryRegisterItem(int index, PlayerItemSlotUI end)
    {
        inventory.AddItemAtPlayerItemSlot(index, end);
    }

    // 툴팁 UI 슬롯 데이터 갱신
    private void UpdateTooltipUI(ItemSlotUI slot)
    {
        if (!slot.IsAccessible || !slot.HasItem)
            return;

        // 툴팁 정보 갱신
        itemTooltipUI.SetItemInfo(inventory.GetItemData(slot.Index));

        // 툴팁 위치 설정
        itemTooltipUI.SetUIPosition(slot.SlotRect);
    }

    #endregion

    #region ** 마우스 이벤트 함수들 **

    // 마우스 커서가 UI 위에 있는지 여부
    private bool IsOverUI() => EventSystem.current.IsPointerOverGameObject();

    // 아이템 툴팁 UI 활성/비활성화
    private void ShowOrHideTooltipUI()
    {
        // 마우스가 아이템 아이콘 위에 올라가있을 때 툴팁표시
        bool isValid =
            pointerOverSlot != null && pointerOverSlot.HasItem && pointerOverSlot.IsAccessible
            && (pointerOverSlot != beginDragSlot);

        if (isValid)
        {
            UpdateTooltipUI(pointerOverSlot);
            itemTooltipUI.ShowTooltipUI();
        }
        else
            itemTooltipUI.HideTooltipUI();

    }

    
    // 마우스 올라갈때 나갈때 처리
    private void OnPointerEnterAndExit()
    {
        // 이전 프레임 슬롯
        var prevSlot = pointerOverSlot;

        // 현재 프레임 슬롯
        var curSlot = pointerOverSlot = rc.RaycastAndgetFirstComponent<ItemSlotUI>();

        // 마우스 올라갈 때
        if (prevSlot == null)
        {
            if (curSlot != null)
            {
                OnCurrentEnter();
            }
        }
        // 마우스 나갈 때
        else
        {
            if (curSlot == null)
            {
                OnPrevExit();
            }
            // 다른 슬롯으로 커서 옮길때
            else if (prevSlot != curSlot)
            {
                OnPrevExit();
                OnCurrentEnter();
            }
        }

        void OnCurrentEnter()
        {
            if (showHighlight)
                curSlot.Highlight(true);
        }

        void OnPrevExit()
        {
            prevSlot.Highlight(false);
        }
    }
    
    // 마우스 눌렀을 때 처리
    private void OnPointerDown()
    {
        // 마우스 좌클릭(Holding)
        if (Input.GetMouseButtonDown(leftClick))
        {
            // 시작 슬롯
            beginDragSlot = rc.RaycastAndgetFirstComponent<ItemSlotUI>();
            // 슬롯에 아이템이 있을 때
            if (beginDragSlot != null && beginDragSlot.HasItem && beginDragSlot.IsAccessible)
            {
                // 드래그 위치,참조 
                beginDragIconTransform = beginDragSlot.IconRect.transform;
                beginDragIconPoint = beginDragIconTransform.position;
                beginDragCursorPoint = Input.mousePosition;

                beginDragSlotSiblingIndex = beginDragSlot.transform.GetSiblingIndex();
                beginDragSlot.transform.SetAsLastSibling();     // 가장 위에 표시

                beginDragSlot.SetHighlightOnTop(false);
            }
            else
            {
                beginDragSlot = null;
            }
        }
        // 마우스 우클릭(아이템 사용 및 장착)
        else if (Input.GetMouseButtonDown(rightClick))
        {
            ItemSlotUI slotUI = rc.RaycastAndgetFirstComponent<ItemSlotUI>();
            
            if(slotUI != null && slotUI.HasItem && slotUI.IsAccessible)
            {
                TryUseItem(slotUI.Index);
            }
        }

    }

    // 마우스 드래그중일 때 처리
    private void OnPointerDrag()
    {
        // 드래그중이 아닐때
        if (beginDragSlot == null) return;

        if (Input.GetMouseButton(leftClick))
        {
            // 슬롯 아이콘 위치 업데이트
            beginDragIconTransform.position = beginDragIconPoint + (Input.mousePosition - beginDragCursorPoint);
        }
    }

    // 마우스 뗐을 때 처리
    private void OnPointerUp()
    {
        if (Input.GetMouseButtonUp(leftClick))
        {
            // 기존으로 복원
            if (beginDragSlot != null)
            {
                // 위치 복원
                beginDragIconTransform.position = beginDragIconPoint;

                // UI 순서 복원
                beginDragSlot.transform.SetSiblingIndex(beginDragSlotSiblingIndex);

                // 드래그 완료
                EndDrag();

                // 하이라이트 이미지를 아이콘보다 앞에
                beginDragSlot.SetHighlightOnTop(true);

                // 참조 제거
                beginDragSlot = null;
                beginDragIconTransform = null;
            }
        }
    }

    // 마우스 드래그 종료 처리(아이템 교환, 이동, 버리기 등)
    private void EndDrag()
    {
        ItemSlotUI endDragSlot = rc.RaycastAndgetFirstComponent<ItemSlotUI>();
        PlayerItemSlotUI playerSlot = rc.RaycastAndgetFirstComponent<PlayerItemSlotUI>();

        // 플레이어 아이템창에 아이템 등록
        if (playerSlot != null)
        {
            TryRegisterItem(beginDragSlot.Index, playerSlot);
        }

        // 아이템 이동 및 교환
        if (endDragSlot != null && endDragSlot.IsAccessible)
        {
            TrySwapItems(beginDragSlot, endDragSlot);
        }
        
        // 아이템 버리기
        if (!IsOverUI())
        {
            int index = beginDragSlot.Index;
            string itemName = inventory.GetItemName(index);

            popup.OpenConfirmationPopupUI(() => TryRemoveItem(index), itemName);
        }
    }
    #endregion

    #region ** Public Methods **
    // 인벤토리 참조등록
    public void SetInventoryRef(Inventory inv)
    {
        inventory = inv;
    }

    // 접근 가능한 슬롯 범위 설정(활성화될 슬롯)
    public void SetAccessibleSlotRange(int accessibleSlotCount)
    {
        // 총 36칸 중
        for (int i = 0; i < slotUIList.Count; i++)
        {
            // accessibleCount 갯수 만큼만 슬롯 활성화
            slotUIList[i].SetSlotAccessibleState(i < accessibleSlotCount);
        }
    }

    // 해당 인덱스 슬롯의 아이템 아이콘 등록 및 수량 표시
    public void SetItemIconAndAmountText(int index, string icon, int amount = 1)
    {
        slotUIList[index].SetItemIconAndAmount(icon, amount);
    }

    // 해당 인덱스 슬롯의 아이템 갯수 텍스트 제거
    public void HideItemAmountText(int index)
    {
        slotUIList[index].SetItemAmount(1);
    }

    // 해당 인덱스 슬롯의 아이템 제거(아이콘 및 텍스트 제거)
    public void RemoveItem(int index)
    {
        slotUIList[index].RemoveItemIcon();
    }

    #endregion
}
