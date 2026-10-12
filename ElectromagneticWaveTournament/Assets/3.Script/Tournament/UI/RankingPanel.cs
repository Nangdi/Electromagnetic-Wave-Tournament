using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 5단계 전체 순위‧측정정보 화면.
// 이번 체험에 선정된 후보의 실제 순위와, 실제 1위/내 선택의 상세 정보를 보여준다.
public class RankingPanel : TournamentPanel
{
    [Tooltip("순위 줄 프리팹. 토너먼트 크기만큼 rowRoot 아래에 만들어 쓰고, 남는 줄은 숨긴다.")]
    [SerializeField] private RankRowView rowPrefab;
    [SerializeField] private Transform rowRoot;
    [SerializeField] private TMP_Text titleText;

    [Header("상세 정보")]
    [SerializeField] private CandidateCardView topDetail;
    [SerializeField] private CandidateCardView myDetail;
    [SerializeField] private TMP_Text myDetailLabel;

    private readonly List<RankRowView> rows = new List<RankRowView>();

    public void ShowRanking(TournamentBracket bracket)
    {
        CandidateData mine = bracket.Champion;
        var ranking = bracket.Ranking;

        if (titleText != null)
            titleText.text = $"실제 측정 순위 (전체 {ranking.Count}개)";

        EnsureRows(ranking.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            bool visible = i < ranking.Count;
            rows[i].gameObject.SetActive(visible);
            if (visible)
                rows[i].Set(i + 1, ranking[i], ranking[i] == mine);
        }

        if (topDetail != null) topDetail.Set(ranking[0], true);
        if (myDetail != null) myDetail.Set(mine, true);
        if (myDetailLabel != null)
            myDetailLabel.text = ranking[0] == mine ? "나의 선택 (실제 1위!)" : $"나의 선택 (실제 {bracket.GetRank(mine)}위)";
    }

    private void EnsureRows(int count)
    {
        if (rowPrefab == null || rowRoot == null)
        {
            Debug.LogError("RankingPanel: rowPrefab 또는 rowRoot가 비어 있습니다.");
            return;
        }

        while (rows.Count < count)
            rows.Add(Instantiate(rowPrefab, rowRoot));
    }
}
