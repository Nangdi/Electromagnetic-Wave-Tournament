using UnityEngine;

// 1단계 대기 화면. 고정된 내용은 StaticLayer 이미지에 있고, 여기서는 시작 버튼만 깜빡인다.
public class IdlePanel : TournamentPanel
{
    [SerializeField] private CanvasGroup startPrompt;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.35f;

    private void Update()
    {
        if (startPrompt == null) return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI) + 1f) * 0.5f;
        startPrompt.alpha = Mathf.Lerp(minAlpha, 1f, t);
    }
}
