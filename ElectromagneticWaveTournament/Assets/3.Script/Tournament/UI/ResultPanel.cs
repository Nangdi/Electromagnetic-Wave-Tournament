using System.Collections;
using TMPro;
using UnityEngine;

// 4단계 최종 결과 화면.
// ① 내가 선택한 우승자 → ② 해당 후보의 기준 대비 % → ③ 실제 1위 → ④ 내 선택의 실제 순위 순서로 공개한다.
public class ResultPanel : TournamentPanel
{
    [Header("① 내가 선택한 우승자")]
    [SerializeField] private CanvasGroup myPickGroup;
    [SerializeField] private CandidateCardView myPickCard;

    [Header("② 내 선택의 기준 대비 %")]
    [SerializeField] private CanvasGroup myRatioGroup;
    [SerializeField] private TMP_Text myRatioText;

    [Header("③ 실제 1위")]
    [SerializeField] private CanvasGroup topGroup;
    [SerializeField] private CandidateCardView topCard;
    [SerializeField] private TMP_Text topRatioText;

    [Header("④ 내 선택의 실제 순위")]
    [SerializeField] private CanvasGroup myRankGroup;
    [SerializeField] private TMP_Text myRankText;

    [Header("다음 안내")]
    [SerializeField] private CanvasGroup nextHint;

    [SerializeField] private float fadeSeconds = 0.3f;

    public bool IsRevealing { get; private set; }

    private Coroutine revealRoutine;

    public void ShowResult(TournamentBracket bracket)
    {
        CandidateData mine = bracket.Champion;
        CandidateData top = bracket.Ranking[0];
        int myRank = bracket.GetRank(mine);

        if (myPickCard != null) myPickCard.Set(mine, false);
        if (myRatioText != null)
            myRatioText.text = $"{CandidateFormat.Ratio(mine.ratio)}\n<size=60%>(실제 {myRank}위)</size>";

        if (topCard != null) topCard.Set(top, false);
        if (topRatioText != null)
            topRatioText.text = $"{CandidateFormat.Ratio(top.ratio)}\n<size=60%>(실제 1위)</size>";

        if (myRankText != null)
        {
            myRankText.text = myRank == 1
                ? "축하합니다!\n<size=150%><color=#FFD34D>실제 1위</color></size>\n를 찾았어요!"
                : $"내 선택은\n{bracket.Size}개 중\n<size=180%><color=#FFD34D>{myRank}위</color></size>\n였습니다!";
        }

        SetAlpha(0f);
        if (revealRoutine != null) StopCoroutine(revealRoutine);
        revealRoutine = StartCoroutine(Reveal());
    }

    // 공개 연출 중 입력이 들어오면 남은 단계를 한 번에 보여준다.
    public void SkipReveal()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        SetAlpha(1f);
        IsRevealing = false;
    }

    public override void Hide()
    {
        SkipReveal();
        base.Hide();
    }

    private IEnumerator Reveal()
    {
        IsRevealing = true;
        float interval = TournamentConfig.Current.resultRevealInterval;

        yield return Fade(myPickGroup);
        yield return new WaitForSecondsRealtime(interval);
        yield return Fade(myRatioGroup);
        yield return new WaitForSecondsRealtime(interval);
        yield return Fade(topGroup);
        yield return new WaitForSecondsRealtime(interval);
        yield return Fade(myRankGroup);
        yield return new WaitForSecondsRealtime(interval);
        yield return Fade(nextHint);

        IsRevealing = false;
        revealRoutine = null;
    }

    private IEnumerator Fade(CanvasGroup group)
    {
        if (group == null) yield break;

        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / fadeSeconds;
            yield return null;
        }
        group.alpha = 1f;
    }

    private void SetAlpha(float alpha)
    {
        if (myPickGroup != null) myPickGroup.alpha = alpha;
        if (myRatioGroup != null) myRatioGroup.alpha = alpha;
        if (topGroup != null) topGroup.alpha = alpha;
        if (myRankGroup != null) myRankGroup.alpha = alpha;
        if (nextHint != null) nextHint.alpha = alpha;
    }
}
