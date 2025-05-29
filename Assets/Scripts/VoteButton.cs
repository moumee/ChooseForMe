using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VoteButton : MonoBehaviour
{
    public float Percent { get; set;}
    private Button _button;
    private Image _buttonImage;

    private Color _fillColor = Color.gray;
    void Awake()
    {
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>();
        
        _button.onClick.AddListener(OnVoteButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Button GetButton()
    {
        return _button;
    }

    public void OnVoteButtonClick()
    {
        _button.interactable = false;
        _buttonImage.color = _fillColor;
        _buttonImage.type = Image.Type.Filled;
        _buttonImage.fillMethod = Image.FillMethod.Vertical;
        _buttonImage.fillAmount = 0;
        StartCoroutine(FillButton());

    }

    private IEnumerator FillButton()
    {
        float elapsedTime = 0f;
        float startFillAmount = 0;

        while (elapsedTime < 0.6f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / 0.6f);
            
            _buttonImage.fillAmount = Mathf.SmoothStep(startFillAmount, Percent, t);
            yield return null;
        }

        _buttonImage.fillAmount = Percent;
    }

    
}
