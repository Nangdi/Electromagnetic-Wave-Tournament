using System.Collections.Generic;
using UnityEngine;

// 토너먼트 1회분의 대진과 진행 상태.
// 공간의 전체 후보 중 필수 후보를 먼저 넣고 나머지를 무작위로 채운 뒤, 대진도 무작위로 섞는다.
// 관람객 선택(좌/우)만 받아 16강 → 8강 → 4강 → 결승을 진행한다.
public class TournamentBracket
{
    private readonly List<CandidateData> entrants;   // 이번 체험에 선정된 후보 (대진 순서)
    private readonly List<CandidateData> ranking;    // 실제 순위 (인체보호기준 대비 % 내림차순)
    private List<CandidateData> currentRound;
    private readonly List<CandidateData> winners = new List<CandidateData>();
    private int matchIndex;

    public IReadOnlyList<CandidateData> Entrants => entrants;
    public IReadOnlyList<CandidateData> Ranking => ranking;
    public int Size => entrants.Count;
    public bool IsFinished { get; private set; }
    public CandidateData Champion { get; private set; }

    // 현재 라운드 참가 수 (16, 8, 4, 2)
    public int RoundSize => currentRound.Count;
    public int MatchesInRound => currentRound.Count / 2;
    // 1부터 시작하는 현재 라운드의 경기 번호
    public int MatchNumber => matchIndex + 1;
    public CandidateData Left => currentRound[matchIndex * 2];
    public CandidateData Right => currentRound[matchIndex * 2 + 1];

    public string RoundName => RoundSize == 2 ? "결승" : $"{RoundSize}강";

    public TournamentBracket(List<CandidateData> pool, int size, System.Random random = null)
    {
        random ??= new System.Random();
        size = ClampSize(size, pool.Count);

        // 필수 후보 먼저, 나머지는 섞어서 채운다.
        var required = pool.FindAll(c => c.required);
        var others = pool.FindAll(c => !c.required);
        Shuffle(required, random);
        Shuffle(others, random);

        entrants = new List<CandidateData>(size);
        foreach (var c in required)
        {
            if (entrants.Count >= size) break;
            entrants.Add(c);
        }
        foreach (var c in others)
        {
            if (entrants.Count >= size) break;
            entrants.Add(c);
        }

        // 필수 후보가 항상 1경기에 몰리지 않도록 대진도 섞는다.
        Shuffle(entrants, random);

        ranking = new List<CandidateData>(entrants);
        ranking.Sort((a, b) =>
        {
            int cmp = b.ratio.CompareTo(a.ratio);
            return cmp != 0 ? cmp : string.CompareOrdinal(a.name, b.name);
        });

        currentRound = new List<CandidateData>(entrants);
        matchIndex = 0;
    }

    // 현재 경기에서 관람객이 고른 쪽을 다음 라운드로 올린다.
    public void Choose(SwitchSide side)
    {
        if (IsFinished) return;

        winners.Add(side == SwitchSide.Left ? Left : Right);
        matchIndex++;

        if (matchIndex < MatchesInRound) return;

        if (winners.Count == 1)
        {
            Champion = winners[0];
            IsFinished = true;
            return;
        }

        currentRound = new List<CandidateData>(winners);
        winners.Clear();
        matchIndex = 0;
    }

    // 실제 순위 (1부터). 이번 대진에 없는 후보면 0.
    public int GetRank(CandidateData candidate)
    {
        return ranking.IndexOf(candidate) + 1;
    }

    // 후보 수가 부족하면 가능한 가장 큰 2의 거듭제곱으로 줄인다.
    private static int ClampSize(int size, int available)
    {
        int target = Mathf.Max(2, Mathf.Min(size, available));
        int pow = 2;
        while (pow * 2 <= target) pow *= 2;

        if (pow != size)
            Debug.LogWarning($"토너먼트 크기 조정: 요청 {size}, 후보 {available} → {pow}강");

        return pow;
    }

    private static void Shuffle<T>(List<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
