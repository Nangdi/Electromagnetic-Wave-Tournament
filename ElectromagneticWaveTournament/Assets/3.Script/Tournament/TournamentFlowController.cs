using System.Collections;
using UnityEngine;

// 전자파 토너먼트 전체 흐름 (상태 머신).
// 대기 → 공간 선택 → 토너먼트 → 최종 결과 → 전체 순위 → 기준‧측정 이해 → 대기
// 입력은 SwitchInput의 좌/우 신호만 받고, 단계별 화면은 각 Panel에 위임한다.
public class TournamentFlowController : MonoBehaviour
{
    public enum FlowState
    {
        Idle,
        MapSelect,
        Match,
        Result,
        Ranking,
        Guide,
    }

    [Header("단계별 화면")]
    [SerializeField] private IdlePanel idlePanel;
    [SerializeField] private MapSelectPanel mapSelectPanel;
    [SerializeField] private MatchPanel matchPanel;
    [SerializeField] private ResultPanel resultPanel;
    [SerializeField] private RankingPanel rankingPanel;
    [SerializeField] private GuidePanel guidePanel;

    [Header("디버그")]
    [SerializeField] private FlowState state = FlowState.Idle;

    public FlowState State => state;

    private TournamentBracket bracket;
    private bool isSelecting;      // 경기 선택 효과 재생 중 (입력 무시)
    private float lastInputTime;

    private void Start()
    {
        if (SwitchInput.Instance != null)
            SwitchInput.Instance.OnPressed += HandleSwitch;
        else
            Debug.LogError("SwitchInput이 씬에 없습니다.");

        if (mapSelectPanel != null)
            mapSelectPanel.OnSpaceConfirmed += HandleSpaceConfirmed;

        GoTo(FlowState.Idle);
    }

    private void OnDestroy()
    {
        if (SwitchInput.Instance != null)
            SwitchInput.Instance.OnPressed -= HandleSwitch;

        if (mapSelectPanel != null)
            mapSelectPanel.OnSpaceConfirmed -= HandleSpaceConfirmed;
    }

    private void Update()
    {
        // 체험 도중 관람객이 떠난 경우 대기 화면으로 복귀
        float timeout = TournamentConfig.Current.idleReturnSeconds;
        if (state != FlowState.Idle && timeout > 0f && Time.unscaledTime - lastInputTime >= timeout)
        {
            Debug.Log("입력 없음 - 대기 화면으로 복귀");
            ReturnToIdle();
        }
    }

    private void HandleSwitch(SwitchSide side)
    {
        lastInputTime = Time.unscaledTime;

        switch (state)
        {
            case FlowState.Idle:
                StartExperience();
                break;

            case FlowState.MapSelect:
                mapSelectPanel.Move(side == SwitchSide.Left ? -1 : 1);
                break;

            case FlowState.Match:
                if (!isSelecting)
                    StartCoroutine(SelectRoutine(side));
                break;

            case FlowState.Result:
                if (resultPanel.IsRevealing)
                    resultPanel.SkipReveal();
                else
                    GoTo(FlowState.Ranking);
                break;

            case FlowState.Ranking:
                GoTo(FlowState.Guide);
                break;

            case FlowState.Guide:
                // 우 스위치 → 체험 종료 / 좌 스위치 → 순위 다시 보기
                if (side == SwitchSide.Right)
                    ReturnToIdle();
                else
                    GoTo(FlowState.Ranking);
                break;
        }
    }

    private void StartExperience()
    {
        var db = CandidateDatabase.Instance;
        if (db == null || db.Spaces.Count == 0)
        {
            Debug.LogError("후보 DB가 비어 있어 체험을 시작할 수 없습니다.");
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();

        GoTo(FlowState.MapSelect);
        mapSelectPanel.Begin(db.Spaces);
    }

    // 공간이 정해지면 후보를 추출하고 대진을 만든 뒤 바로 첫 경기로 넘어간다. (별도 화면 없음)
    private void HandleSpaceConfirmed(SpaceData space)
    {
        if (state != FlowState.MapSelect) return;

        var pool = CandidateDatabase.Instance.GetCandidates(space.id);
        if (pool.Count < 2)
        {
            Debug.LogError($"[{space.name}] 후보가 부족합니다: {pool.Count}개");
            ReturnToIdle();
            return;
        }

        bracket = new TournamentBracket(pool, TournamentConfig.Current.bracketSize);
        Debug.Log($"[{space.name}] {bracket.Size}강 대진 생성: " + string.Join(", ", EntrantNames()));

        GoTo(FlowState.Match);
        matchPanel.ShowMatch(bracket);
    }

    private IEnumerator SelectRoutine(SwitchSide side)
    {
        isSelecting = true;
        yield return matchPanel.PlaySelectEffect(side, TournamentConfig.Current.selectEffectSeconds);
        isSelecting = false;

        // 효과 재생 중 대기 화면으로 복귀한 경우
        if (state != FlowState.Match) yield break;

        bracket.Choose(side);

        if (bracket.IsFinished)
        {
            GoTo(FlowState.Result);
            resultPanel.ShowResult(bracket);
        }
        else
        {
            matchPanel.ShowMatch(bracket);
        }
    }

    private void ReturnToIdle()
    {
        StopAllCoroutines();
        isSelecting = false;
        bracket = null;

        if (GameManager.Instance != null)
            GameManager.Instance.EndGame();

        GoTo(FlowState.Idle);
    }

    private void GoTo(FlowState next)
    {
        state = next;
        lastInputTime = Time.unscaledTime;

        SetPanel(idlePanel, next == FlowState.Idle);
        SetPanel(mapSelectPanel, next == FlowState.MapSelect);
        SetPanel(matchPanel, next == FlowState.Match);
        SetPanel(resultPanel, next == FlowState.Result);
        SetPanel(rankingPanel, next == FlowState.Ranking);
        SetPanel(guidePanel, next == FlowState.Guide);

        if (next == FlowState.Ranking && bracket != null)
            rankingPanel.ShowRanking(bracket);
    }

    private static void SetPanel(TournamentPanel panel, bool visible)
    {
        if (panel == null) return;

        if (visible) panel.Show();
        else panel.Hide();
    }

    private string[] EntrantNames()
    {
        var names = new string[bracket.Size];
        for (int i = 0; i < names.Length; i++)
            names[i] = bracket.Entrants[i].name;
        return names;
    }
}
