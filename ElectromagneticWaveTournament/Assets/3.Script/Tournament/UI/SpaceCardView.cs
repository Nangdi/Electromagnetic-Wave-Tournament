using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 공간(맵) 선택 화면의 공간 카드 1개.
public class SpaceCardView : MonoBehaviour
{
    [SerializeField] private Image picture;
    [SerializeField] private TMP_Text nameText;

    public void Set(SpaceData space)
    {
        if (nameText != null) nameText.text = space != null ? space.name : "";

        if (picture == null) return;

        var db = CandidateDatabase.Instance;
        Sprite sprite = space != null && db != null ? db.LoadSprite(space.image) : null;
        picture.sprite = sprite;
        picture.enabled = sprite != null;
        picture.preserveAspect = true;
    }
}
