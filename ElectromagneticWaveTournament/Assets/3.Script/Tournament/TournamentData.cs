using System;
using System.Collections.Generic;

// 전자파 토너먼트에서 쓰는 데이터 정의.
// StreamingAssets/candidates.json 과 1:1로 대응한다.

// 공간(맵) 1개. 거실/침실/서재/주방‧다용도실/욕실‧개인관리/우리 동네
[Serializable]
public class SpaceData
{
    public string id;    // candidates.spaceId 와 연결되는 키
    public string name;  // 화면 표시 이름
    public string image; // StreamingAssets/Images 기준 상대 경로 (없으면 비워둔다)
}

// 토너먼트 후보(전자제품/시설) 1개.
[Serializable]
public class CandidateData
{
    public string id;
    public string spaceId;
    public string name;      // 제품/시설명
    public string frequency; // 주파수 (예: "60 Hz")
    public string distance;  // 측정거리 (예: "30 cm")
    public string position;  // 측정위치 (예: "정면")
    public string condition; // 작동조건 (예: "강풍")
    public float ratio;      // 인체보호기준 대비 % (0.91 = 0.91%)
    public string image;     // StreamingAssets/Images 기준 상대 경로
    public bool required;    // true면 해당 공간 토너먼트에 항상 포함 (예: 우리 동네의 송전탑/옥외용 변전소)
}

[Serializable]
public class CandidateDbJson
{
    public List<SpaceData> spaces = new List<SpaceData>();
    public List<CandidateData> candidates = new List<CandidateData>();
}

public enum SwitchSide
{
    Left,
    Right,
}

// Setting.json(GameDynamicData) 접근용. JsonManager가 없으면 기본값을 돌려준다.
public static class TournamentConfig
{
    private static GameDynamicData fallback;

    public static GameDynamicData Current
    {
        get
        {
            if (JsonManager.instance != null && JsonManager.instance.gameDynamicData != null)
                return JsonManager.instance.gameDynamicData;

            return fallback ??= new GameDynamicData();
        }
    }
}

public static class CandidateFormat
{
    // 비율 표기 (예: 0.91%, 12.00%)
    public static string Ratio(float ratio)
    {
        return $"{ratio:0.00}%";
    }

    // 카드에 표시할 측정 정보 여러 줄
    public static string Info(CandidateData c, bool showRatio)
    {
        // <pos>로 값의 시작 위치를 맞춘다 (라벨 글자 수가 달라도 세로 정렬 유지)
        string info =
            $"주파수<pos=5em>{c.frequency}\n" +
            $"측정거리<pos=5em>{c.distance}\n" +
            $"측정위치<pos=5em>{c.position}\n" +
            $"작동조건<pos=5em>{c.condition}";

        if (showRatio)
            info += $"\n<color=#FFD34D>인체보호기준 대비  {Ratio(c.ratio)}</color>";

        return info;
    }
}
