using UnityEngine;

// 단계별 화면의 공통 베이스. 화면 전환은 TournamentFlowController가 Show/Hide로 처리한다.
public abstract class TournamentPanel : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
