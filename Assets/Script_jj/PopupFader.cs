using UnityEngine;

public class PopupFader : MonoBehaviour
{
    public CanvasGroup popupGroup;  // 팝업 패널에 붙인 CanvasGroup
    public float duration = 0.5f;   // 페이드 아웃 시간

    void OnEnable()
    {
        StartCoroutine(FadeOutAndHide());
    }

    private System.Collections.IEnumerator FadeOutAndHide()
    {
        yield return new WaitForSeconds(1.0f);  // 1초 후 시작

        float t = 0f;
        float startAlpha = popupGroup.alpha;

        while (t < duration)
        {
            t += Time.deltaTime;
            popupGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }

        popupGroup.alpha = 0f;
        gameObject.SetActive(false);  // 완전히 숨김
        popupGroup.alpha = 1f;
    }
}
