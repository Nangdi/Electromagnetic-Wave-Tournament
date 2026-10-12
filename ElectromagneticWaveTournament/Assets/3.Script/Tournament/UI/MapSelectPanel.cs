using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 2단계 공간(맵) 선택 화면.
// 리듬게임 곡 선택처럼 좌/우 스위치로 공간을 넘기고, 일정 시간 입력이 없으면 가운데 공간이 자동 선택된다.
public class MapSelectPanel : TournamentPanel
{
    [Header("공간 카드 (이전 / 가운데 / 다음)")]
    [SerializeField] private SpaceCardView prevCard;
    [SerializeField] private SpaceCardView centerCard;
    [SerializeField] private SpaceCardView nextCard;

    [Header("페이지 표시")]
    [Tooltip("페이지 점 프리팹. 공간 수만큼 dotRoot 아래에 만들어 쓴다.")]
    [SerializeField] private Image dotPrefab;
    [SerializeField] private Transform dotRoot;
    [SerializeField] private Color dotOnColor = new Color(1f, 0.83f, 0.3f, 1f);
    [SerializeField] private Color dotOffColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("자동 선택 게이지")]
    [Tooltip("anchorMax.x 를 줄여 남은 시간을 표시하는 막대")]
    [SerializeField] private RectTransform gaugeFill;
    [SerializeField] private TMP_Text gaugeText;

    [Header("연출")]
    [SerializeField] private float moveScalePunch = 1.05f;
    [SerializeField] private float moveEffectSeconds = 0.15f;

    // 자동 선택된 공간을 알린다.
    public event Action<SpaceData> OnSpaceConfirmed;

    private readonly List<SpaceData> spaces = new List<SpaceData>();
    private readonly List<Image> dots = new List<Image>();
    private int index;
    private float elapsed;
    private bool confirmed;
    private Coroutine moveEffect;

    public void Begin(IReadOnlyList<SpaceData> source)
    {
        spaces.Clear();
        spaces.AddRange(source);
        index = 0;
        elapsed = 0f;
        confirmed = false;
        EnsureDots(spaces.Count);
        Refresh();
        UpdateGauge();
    }

    // dir: -1 이전, +1 다음. 처음/끝에서는 반대쪽으로 순환한다.
    public void Move(int dir)
    {
        if (confirmed || spaces.Count == 0) return;

        index = Wrap(index + dir);
        elapsed = 0f;
        Refresh();
        UpdateGauge();

        if (moveEffect != null) StopCoroutine(moveEffect);
        moveEffect = StartCoroutine(PunchCenter());
    }

    private void Update()
    {
        if (confirmed || spaces.Count == 0) return;

        elapsed += Time.unscaledDeltaTime;
        UpdateGauge();

        if (elapsed >= TournamentConfig.Current.mapAutoSelectSeconds)
        {
            confirmed = true;
            OnSpaceConfirmed?.Invoke(spaces[index]);
        }
    }

    private void Refresh()
    {
        bool hasSpaces = spaces.Count > 0;
        if (centerCard != null) centerCard.Set(hasSpaces ? spaces[index] : null);
        if (prevCard != null) prevCard.Set(hasSpaces ? spaces[Wrap(index - 1)] : null);
        if (nextCard != null) nextCard.Set(hasSpaces ? spaces[Wrap(index + 1)] : null);

        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].gameObject.SetActive(i < spaces.Count);
            dots[i].color = i == index ? dotOnColor : dotOffColor;
        }
    }

    private void EnsureDots(int count)
    {
        if (dotPrefab == null || dotRoot == null) return;

        while (dots.Count < count)
            dots.Add(Instantiate(dotPrefab, dotRoot));
    }

    private void UpdateGauge()
    {
        float limit = Mathf.Max(0.01f, TournamentConfig.Current.mapAutoSelectSeconds);
        float remain = Mathf.Clamp01(1f - elapsed / limit);

        if (gaugeFill != null)
            gaugeFill.anchorMax = new Vector2(remain, gaugeFill.anchorMax.y);

        if (gaugeText != null)
            gaugeText.text = $"선택 시간 {Mathf.CeilToInt(Mathf.Max(0f, limit - elapsed))}초";
    }

    private IEnumerator PunchCenter()
    {
        if (centerCard == null) yield break;

        Transform target = centerCard.transform;
        for (float t = 0f; t < moveEffectSeconds; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.Sin(t / moveEffectSeconds * Mathf.PI);
            target.localScale = Vector3.one * Mathf.Lerp(1f, moveScalePunch, k);
            yield return null;
        }
        target.localScale = Vector3.one;
        moveEffect = null;
    }

    private int Wrap(int i)
    {
        int n = spaces.Count;
        return n == 0 ? 0 : ((i % n) + n) % n;
    }
}
