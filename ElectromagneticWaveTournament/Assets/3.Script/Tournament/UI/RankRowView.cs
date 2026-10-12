using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전체 순위 화면의 한 줄 (순위, 이름, 인체보호기준 대비 %, 내 선택 표시).
public class RankRowView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ratioText;
    [Tooltip("내 선택일 때만 보이는 표시 (예: ← 내 선택)")]
    [SerializeField] private GameObject myPickMark;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.05f);
    [SerializeField] private Color myPickColor = new Color(1f, 0.83f, 0.3f, 0.35f);

    public void Set(int rank, CandidateData data, bool isMyPick)
    {
        if (rankText != null) rankText.text = $"{rank}위";
        if (nameText != null) nameText.text = data.name;
        if (ratioText != null) ratioText.text = CandidateFormat.Ratio(data.ratio);
        if (myPickMark != null) myPickMark.SetActive(isMyPick);
        if (background != null) background.color = isMyPick ? myPickColor : normalColor;
    }
}
