using UnityEngine;

public class PopupFader : MonoBehaviour
{
    public CanvasGroup popupGroup;  // �˾� �гο� ���� CanvasGroup
    public float duration = 0.5f;   // ���̵� �ƿ� �ð�

    void OnEnable()
    {
        StartCoroutine(FadeOutAndHide());
    }

    private System.Collections.IEnumerator FadeOutAndHide()
    {
        yield return new WaitForSeconds(1.0f);  // 1�� �� ����

        float t = 0f;
        float startAlpha = popupGroup.alpha;

        while (t < duration)
        {
            t += Time.deltaTime;
            popupGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }

        popupGroup.alpha = 0f;
        gameObject.SetActive(false);  // ������ ����
        popupGroup.alpha = 1f;
    }
}
