using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Comments : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool PointerOnComment = false;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerOnComment = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerOnComment = false;
    }
}
