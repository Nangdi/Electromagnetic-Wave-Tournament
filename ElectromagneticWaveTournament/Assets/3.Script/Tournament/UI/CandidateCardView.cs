using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 후보 1개를 보여주는 카드 (이미지, 이름, 측정 정보).
// 토너먼트 경기, 최종 결과, 상세 정보 화면에서 공용으로 쓴다.
public class CandidateCardView : MonoBehaviour
{
    [SerializeField] private Image picture;
    [Tooltip("이미지가 없을 때 대신 보여줄 텍스트")]
    [SerializeField] private TMP_Text placeholderText;
    [SerializeField] private TMP_Text nameText;
    [Tooltip("주파수/측정거리/측정위치/작동조건. 비워두면 표시하지 않는다.")]
    [SerializeField] private TMP_Text infoText;
    [Tooltip("선택 시 켜지는 테두리/하이라이트")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("선택 효과")]
    [SerializeField] private float selectedScale = 1.06f;
    [SerializeField] private float dimmedAlpha = 0.35f;

    public CandidateData Data { get; private set; }

    public void Set(CandidateData data, bool showRatio)
    {
        Data = data;
        ResetEffect();

        if (data == null)
        {
            if (nameText != null) nameText.text = "";
            if (infoText != null) infoText.text = "";
            SetPicture(null);
            return;
        }

        if (nameText != null) nameText.text = data.name;
        if (infoText != null) infoText.text = CandidateFormat.Info(data, showRatio);

        var db = CandidateDatabase.Instance;
        SetPicture(db != null ? db.LoadSprite(data.image) : null);
    }

    public void ResetEffect()
    {
        transform.localScale = Vector3.one;
        if (highlight != null) highlight.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void SetHighlight(bool on)
    {
        if (highlight != null) highlight.SetActive(on);
    }

    // 선택된 카드: 하이라이트 + 확대
    public IEnumerator PlaySelected(float duration)
    {
        SetHighlight(true);
        float half = Mathf.Max(0.01f, duration * 0.5f);
        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            float s = Mathf.Lerp(1f, selectedScale, Mathf.SmoothStep(0f, 1f, t / half));
            transform.localScale = Vector3.one * s;
            yield return null;
        }
        transform.localScale = Vector3.one * selectedScale;
        yield return new WaitForSecondsRealtime(duration - half);
    }

    // 선택되지 않은 카드: 흐리게
    public IEnumerator PlayDimmed(float duration)
    {
        if (canvasGroup == null) yield break;

        float fade = Mathf.Max(0.01f, duration * 0.5f);
        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, dimmedAlpha, t / fade);
            yield return null;
        }
        canvasGroup.alpha = dimmedAlpha;
    }

    private void SetPicture(Sprite sprite)
    {
        if (picture != null)
        {
            picture.sprite = sprite;
            picture.enabled = sprite != null;
            picture.preserveAspect = true;
        }

        if (placeholderText != null)
            placeholderText.gameObject.SetActive(sprite == null);
    }
}
