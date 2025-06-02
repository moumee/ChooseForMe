using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour, IEndDragHandler
{
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    public float itemHeight; // Height of each content item
    public float snapThresholdVelocity = 10f; // Velocity needed to move to next/prev item
    public float snapSpeed = 10f;

    private int itemCount;
    private int currentItemIndex = 0;
    private bool isSnapping = false;
    private float targetNormalizedPosition;
    
    void Awake()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (contentPanel == null) contentPanel = scrollRect.content;

        itemCount = contentPanel.childCount;
        // Assuming all items have the same height, calculate itemHeight
        // or set it manually if items are dynamically sized but snap points are fixed.
        if (itemCount > 0 && itemHeight == 0)
        {
            // A more robust way would be to get the actual height of a child RectTransform
            // For example, if using a VerticalLayoutGroup with uniform child sizes:
            // itemHeight = contentPanel.GetChild(0).GetComponent<RectTransform>().rect.height;
            // For simplicity, assuming you set this or calculate based on viewport if each item fills it.
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSnapping)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition,
                targetNormalizedPosition, Time.deltaTime * snapSpeed);
            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetNormalizedPosition) < 0.001f)
            {
                scrollRect.verticalNormalizedPosition = targetNormalizedPosition;
                isSnapping = false;
                // Re-enable inertia if you want it for subsequent manual drags,
                // though for TikTok style, you might keep it off until the next drag starts.
                // scrollRect.inertia = true;
            }
        }
    }

    


    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemCount == 0) return;

        float currentNormalizedPos = scrollRect.verticalNormalizedPosition;
        float swipeVelocity = scrollRect.velocity.y;

        // Normalize item height relative to content height
        float normalizedItemHeight = 0;
        if (contentPanel.rect.height > itemHeight) // Avoid division by zero or negative content height
        {
             normalizedItemHeight = itemHeight / (contentPanel.rect.height - scrollRect.viewport.rect.height); // Approximation
        }


        // Determine target index based on current position and velocity
        int targetIndex = currentItemIndex;

        // Note: ScrollRect's verticalNormalizedPosition is 1 at the top and 0 at the bottom.
        // We need to invert this for easier page indexing (0 at top, N-1 at bottom)
        float invertedNormalizedPos = 1f - currentNormalizedPos;
        int closestItem = Mathf.RoundToInt(invertedNormalizedPos / normalizedItemHeight);


        if (Mathf.Abs(swipeVelocity) > snapThresholdVelocity)
        {
            if (swipeVelocity < 0) // Swiped Up (towards next item - content moves down)
            {
                targetIndex = Mathf.Clamp(currentItemIndex + 1, 0, itemCount - 1);
            }
            else if (swipeVelocity > 0) // Swiped Down (towards previous item - content moves up)
            {
                targetIndex = Mathf.Clamp(currentItemIndex - 1, 0, itemCount - 1);
            }
        } else {
             targetIndex = Mathf.Clamp(closestItem, 0, itemCount - 1);
        }


        currentItemIndex = targetIndex;
        targetNormalizedPosition = 1f - (currentItemIndex * normalizedItemHeight); // Convert back to ScrollRect's normalization
        // Handle edge cases for the very first and last items
        if (currentItemIndex == 0) targetNormalizedPosition = 1f;
        else if (currentItemIndex == itemCount -1 ) targetNormalizedPosition = 0f;


        isSnapping = true;
        scrollRect.inertia = false; // Disable inertia while snapping
    }
}
