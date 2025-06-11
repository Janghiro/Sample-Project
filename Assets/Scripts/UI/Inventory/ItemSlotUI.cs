using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
                        ItemSlotUI

            - 인벤토리 각 개별 슬롯을 관리
            - 아이템 아이콘과 아이템 갯수 텍스트 등록 및 제거
            - 슬롯 강조효과
*/
public class ItemSlotUI : MonoBehaviour
{
    #region ** Serialized Fields **
    [SerializeField] private Image iconImage;           // 아이템 아이콘 이미지
    [SerializeField] private Text amountText;           // 아이템 수량
    [SerializeField] private Image highlightImage;      // 하이라이트 이미지
    #endregion

    #region ** Fields **
    private Image slotImage;
    private InventoryUI inventoryUI;

    private RectTransform slotRect;                     // 슬롯의 RT
    private RectTransform iconRect;                     // 슬롯의 아이템 아이콘 RT
    private RectTransform highlightRect;                // 슬롯의 하이라이트 RT

    private GameObject iconGo;                          
    private GameObject textGo;
    private GameObject highlightGo;


    private float padding = 4f;                         // 슬롯 내 아이콘과 슬롯 사이 여백
    private float maxHighlightAlpha = 0.5f;             // 하이라이트 이미지 최대 알파값
    private float currentHighlightAlpha = 0f;           // 현재 하이라이트 이미지 알파값
    private float highlightFadeDuration = 0.2f;         // 하이라이트 소요 시간
    private string iconName = "";                       // 현재 아이템 아이콘 이름

    private bool isAccessibleSlot = true;               // 슬롯 접근가능 여부
    private bool isAccessibleItem = true;               // 아이템 접근가능 여부

    // 비활성화된 슬롯 색상
    private Color InAccessibleSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    // 비활성화된 아이콘 색상
    private Color InAccessibleIconColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    #endregion

    #region ** Properties **
    public int Index { get; private set; }              // 슬롯 인덱스
    public bool HasItem => iconImage.sprite != null;    // 슬롯에 아이템이 있는지 여부(sprite 여부로 확인)

    public bool IsAccessible => isAccessibleItem && isAccessibleSlot;

    public RectTransform SlotRect => slotRect;

    public RectTransform IconRect => iconRect;
    #endregion

    #region ** Unity Events **
    private void Awake()
    {
        InitComponents();
        InitValues();
    }
    #endregion

    #region ** Private Methods **
    // 초기화
    private void InitComponents()
    {
        inventoryUI = GetComponentInParent<InventoryUI>();

        slotRect = GetComponent<RectTransform>();
        iconRect = iconImage.rectTransform;
        highlightRect = highlightImage.rectTransform;

        iconGo = iconRect.gameObject;
        textGo = amountText.gameObject;
        highlightGo = highlightImage.gameObject;

        slotImage = GetComponent<Image>();
    }

    // 초기화(아이콘 위치, 하이라이트 이미지 위치)
    private void InitValues()
    {
        // 아이콘 RT 설정(Pivot : 중앙, Anchor : Top Left)
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;

        // 아이콘 패딩 설정
        iconRect.offsetMin = Vector2.one * (padding);
        iconRect.offsetMax = Vector2.one * (-padding);

        // 아이콘과 하이라이트 RT를 동일하게 설정
        highlightRect.pivot = iconRect.pivot;
        highlightRect.anchorMin = iconRect.anchorMin;
        highlightRect.anchorMax = iconRect.anchorMax;
        highlightRect.offsetMin = iconRect.offsetMin;
        highlightRect.offsetMax = iconRect.offsetMax;

        // 아이콘 및 하이라이트 이미지는 클릭X
        iconImage.raycastTarget = false;
        highlightImage.raycastTarget = false;

        HideIcon();
        // 하이라이트 효과 꺼놓기
        highlightGo.SetActive(false);
    }

    // 아이템 아이콘 활성화
    private void ShowIcon() => iconGo.SetActive(true);
    
    // 아이템 아이콘 비활성화
    private void HideIcon() => iconGo.SetActive(false);
   
    // 수량 텍스트 활성화
    private void ShowText() => textGo.SetActive(true);
    
    // 수량 텍스트 비활성화
    private void HideText() => textGo.SetActive(false);

    #endregion

    #region ** Public Methods **
    // 슬롯 인덱스 설정
    public void SetSlotIndex(int index) => Index = index;
    
    // 슬롯 활성화/비활성화 여부 설정
    public void SetSlotAccessibleState(bool value)
    {
        // 현재 슬롯상태가 설정하고자하는 value와 같으면 무시
        if (isAccessibleSlot == value) return;

        // 활성화된 슬롯
        if(value)
        {
            slotImage.color = Color.black;
        }
        // 비활성화된 슬롯
        else
        {
            slotImage.color = InAccessibleSlotColor;
            HideIcon();
            HideText();
        }

        isAccessibleSlot = value;
    }

    // 아이템 활성화/비활성화 여부 설정
    public void SetItemAccessibleState(bool value)
    {
        // 현재 아이템상태가 설정하고자하는 value와 같으면 무시
        if (isAccessibleItem == value) return;

        // 활성화된 아이템 색상
        if(value)
        {
            iconImage.color = Color.white;
            amountText.color = Color.white;
        }
        // 비활성화된 아이템 색상
        else
        {
            iconImage.color = InAccessibleIconColor;
            amountText.color = InAccessibleIconColor;
        }

        isAccessibleItem = value;
    }

    // 아이템 아이콘 등록
    public void SetItemIcon(string itemSprite)
    {
        if (itemSprite != null)
        {
            // ResourceManager 코드 삭제하고 직접 설정
            iconName = itemSprite;
            iconImage.sprite = null; // TODO: 이후 직접 스프라이트 설정 가능
            ShowIcon();
        }
        else
        {
            RemoveItemIcon();
        }
    }

    // 아이템 아이콘 등록 및 수량 표시
    public void SetItemIconAndAmount(string itemSprite, int amount)
    {
        if (itemSprite != null)
        {
            iconName = itemSprite;
            iconImage.sprite = null; // TODO: 이후 직접 스프라이트 설정 가능

            if (amount > 1)
                ShowText();
            else
                HideText();

            ShowIcon();
            amountText.text = amount.ToString();
        }
        else
        {
            RemoveItemIcon();
        }
    }

    // 하이라이트 이미지를 상/하단으로 표시
    public void SetHighlightOnTop(bool value)
    {
        if (value)
            highlightRect.SetAsLastSibling();
        else
            highlightRect.SetAsFirstSibling();
    }

    // 슬롯에서 아이템 제거(아이콘, Text)
    public void RemoveItemIcon()
    {
        iconImage.sprite = null;
        HideIcon();
        HideText();
    }

    // 슬롯에 아이템 갯수 텍스트 설정
    public void SetItemAmount(int amount)
    {
        // 갯수가 2개이상일때만 표시
        if (HasItem && amount > 1)
        {
            ShowText();
        }
        else
        {
            HideText();
        }
        amountText.text = amount.ToString();
    }

    // 다른 슬롯으로 아이템 이동
    public void SwapOrMoveIcon(ItemSlotUI otherSlot)
    {
        // 1. 다른 슬롯이 지정되지 않았을 때
        if (otherSlot == null) return;
        // 2. 자기 자신일 때
        if (otherSlot == this) return;
        // 3. 비활성화 상태일 때
        if (!this.IsAccessible || !otherSlot.IsAccessible) return;

        // 1. 다른 슬롯에 아이템이 있을 때 -> 내 아이콘은 다른 슬롯의 아이콘으로 변경
        if (otherSlot.HasItem) SetItemIcon(otherSlot.iconName);
        // 2. 다른 슬롯에 아이템이 없을 때 -> 내 아이콘만 지우기
        else RemoveItemIcon();

        otherSlot.SetItemIcon(iconName);
    }

    // 슬롯 하이라이트 표시 및 해제
    public void Highlight(bool show)
    {
        // 비활성화 슬롯은 무시
        if (!this.IsAccessible) return;

        if (show)
            StartCoroutine(nameof(HighlightFadeIn));
        else
            StartCoroutine(nameof(HighlightFadeOut));
    }

    #endregion

    #region ** Coroutines **
    // 하이라이트 Fade-in
    private IEnumerator HighlightFadeIn()
    {
        // 실행중인 fade-out 멈추기
        StopCoroutine(nameof(HighlightFadeOut));

        // 하이라이트 이미지 활성화
        highlightGo.SetActive(true);

        float timer = maxHighlightAlpha / highlightFadeDuration;

        // 하이라이트 이미지 알파값을 서서히 증가시키기
        for(; currentHighlightAlpha <= maxHighlightAlpha; currentHighlightAlpha += timer * Time.deltaTime)
        {
            highlightImage.color = new Color(
                     highlightImage.color.r,
                     highlightImage.color.g,
                     highlightImage.color.b,
                     currentHighlightAlpha
                );

            yield return null;
        }
    }

    // 하이라이트 Fade-out
    private IEnumerator HighlightFadeOut()
    {
        StopCoroutine(nameof(HighlightFadeIn));

        float timer = maxHighlightAlpha / highlightFadeDuration;

        // 하이라이트 이미지 알파값을 서서히 감소시키기
        for(; currentHighlightAlpha >= 0f; currentHighlightAlpha -= timer * Time.deltaTime)
        {
            highlightImage.color = new Color(
                     highlightImage.color.r,
                     highlightImage.color.g,
                     highlightImage.color.b,
                     currentHighlightAlpha
                );

            yield return null;
        }

        highlightGo.SetActive(false);
    }
    #endregion

}
