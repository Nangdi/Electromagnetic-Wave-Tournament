using System.Collections;
using TMPro;
using UnityEngine;

// 3단계 토너먼트 진행 화면.
// 실제 값(인체보호기준 대비 %, 순위)은 이 화면에서 보여주지 않는다.
public class MatchPanel : TournamentPanel
{
    [SerializeField] private TMP_Text roundText;    // 예: 16강 1경기
    [SerializeField] private TMP_Text progressText; // 예: 1 / 8
    [SerializeField] private CandidateCardView leftCard;
    [SerializeField] private CandidateCardView rightCard;

    public void ShowMatch(TournamentBracket bracket)
    {
        if (roundText != null)
            roundText.text = bracket.RoundSize == 2 ? "결승" : $"{bracket.RoundName} {bracket.MatchNumber}경기";
        if (progressText != null)
            progressText.text = $"{bracket.MatchNumber} / {bracket.MatchesInRound}";

        if (leftCard != null) leftCard.Set(bracket.Left, false);
        if (rightCard != null) rightCard.Set(bracket.Right, false);
    }

    // 별도 '진출' 화면 없이 현재 화면에서 짧게 선택 효과를 보여준다.
    public IEnumerator PlaySelectEffect(SwitchSide side, float duration)
    {
        CandidateCardView chosen = side == SwitchSide.Left ? leftCard : rightCard;
        CandidateCardView other = side == SwitchSide.Left ? rightCard : leftCard;

        if (other != null) StartCoroutine(other.PlayDimmed(duration));
        if (chosen != null) yield return chosen.PlaySelected(duration);
        else yield return new WaitForSecondsRealtime(duration);
    }
}
