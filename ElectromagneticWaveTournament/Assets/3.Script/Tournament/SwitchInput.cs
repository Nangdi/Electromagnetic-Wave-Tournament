using System;
using UnityEngine;

// 좌/우 선택 스위치 입력을 한곳으로 모은다.
// RS232(TournamentSerialManager)와 키보드(테스트용) 입력이 모두 Press()를 거쳐 OnPressed로 전달된다.
public class SwitchInput : MonoBehaviour
{
    public static SwitchInput Instance { get; private set; }

    public event Action<SwitchSide> OnPressed;

    [Header("키보드 입력 (테스트/비상용)")]
    [SerializeField] private bool useKeyboard = true;
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    private float lastLeftTime = -999f;
    private float lastRightTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!useKeyboard) return;

        if (Input.GetKeyDown(leftKey)) Press(SwitchSide.Left);
        if (Input.GetKeyDown(rightKey)) Press(SwitchSide.Right);
    }

    public void Press(SwitchSide side)
    {
        float now = Time.unscaledTime;
        float debounce = TournamentConfig.Current.inputDebounceSeconds;

        if (side == SwitchSide.Left)
        {
            if (now - lastLeftTime < debounce) return;
            lastLeftTime = now;
        }
        else
        {
            if (now - lastRightTime < debounce) return;
            lastRightTime = now;
        }

        OnPressed?.Invoke(side);
    }
}
