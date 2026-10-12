using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TournamentUIKit;

// 정적 레이어(화면당 PNG 한 장)와 글자가 들어간 부품 스프라이트의 "임시 디자인".
// 여기 있는 내용은 PNG로 구워질 뿐 씬/프리팹에는 오브젝트로 남지 않는다.
// 문구나 배치를 바꾸려면 여기를 고친 뒤 "임시 스프라이트 다시 내보내기"를 실행하거나, 디자인 PNG로 덮어쓴다.
public static class TournamentStaticLayouts
{
    // 화면 전체(3840x2160)에 그려지는 패널별 정적 내용
    public static readonly Dictionary<string, Action<RectTransform>> Panels = new Dictionary<string, Action<RectTransform>>
    {
        { "Idle", Idle },
        { "MapSelect", MapSelect },
        { "Match", Match },
        { "Result", Result },
        { "Ranking", Ranking },
        { "Guide", Guide },
    };

    // 글자가 들어간 부품 스프라이트 (이름, 크기, 그리기). 배경은 투명.
    public static readonly (string name, Vector2Int size, Action<RectTransform> draw)[] TextParts =
    {
        ("Button_Start", new Vector2Int(1400, 200), root =>
        {
            AddRoundBox(root, AccentColor);
            FillText(root, "Text", "스위치를 눌러 시작", 88, DarkTextColor, true);
        }),
        ("Result_Label_MyPick", new Vector2Int(1050, 125), root => FillText(root, "Text", "① 내가 선택한 우승자", 64, SubTextColor, true)),
        ("Result_Label_Top", new Vector2Int(1050, 125), root => FillText(root, "Text", "② 실제 1위", 64, SubTextColor, true)),
        ("Result_Label_MyRank", new Vector2Int(1050, 125), root => FillText(root, "Text", "③ 내 선택의 실제 순위", 64, SubTextColor, true)),
        ("Result_NextHint", new Vector2Int(1600, 120), root => FillText(root, "Text", "스위치를 눌러 다음 ▶", 64, TextColor, true)),
    };

    private static void Background(RectTransform root)
    {
        AddImage(StretchFull(CreateRect("Background", root)), BgColor);
    }

    private static void Idle(RectTransform root)
    {
        Background(root);
        CreateText(root, "Title", "전자파 토너먼트", new Vector2(0, 420), new Vector2(3200, 320), 230, AccentColor, true);
        CreateText(root, "Description", "우리나라 인체보호기준 대비 전자파 비율이 더 높은\n전자제품‧시설을 찾아보세요!",
            new Vector2(0, 60), new Vector2(3200, 340), 100, TextColor);

        RectTransform notice = CreateRoundBox(root, "NoticeBox", new Vector2(0, -260), new Vector2(2400, 200), SoftBoxColor);
        FillText(notice, "Text", "※ 전자파 인체보호기준은 주파수에 따라 다르게 적용됩니다.", 64, SubTextColor);

        CreateText(root, "Guide", "왼쪽 또는 오른쪽 스위치 중 하나를 누르세요", new Vector2(0, -820), new Vector2(2400, 100), 56, SubTextColor);
    }

    private static void MapSelect(RectTransform root)
    {
        Background(root);
        CreateText(root, "Header", "6개 공간 중 하나를 선택하세요", new Vector2(0, 880), new Vector2(3000, 160), 110, TextColor, true);
        CreateText(root, "SubHeader", "좌/우 스위치로 공간을 이동하세요. 시간이 지나면 가운데 공간이 선택됩니다.",
            new Vector2(0, 760), new Vector2(3400, 100), 58, SubTextColor);

        CreateText(root, "Arrow_Left", "◀", new Vector2(-1820, 60), new Vector2(160, 200), 140, AccentColor);
        CreateText(root, "Arrow_Right", "▶", new Vector2(1820, 60), new Vector2(160, 200), 140, AccentColor);

        CreateText(root, "Hint_Prev", "◀ 이전 공간", new Vector2(-1450, -840), new Vector2(700, 100), 60, TextColor);
        CreateText(root, "Hint_Next", "다음 공간 ▶", new Vector2(1450, -840), new Vector2(700, 100), 60, TextColor);
    }

    private static void Match(RectTransform root)
    {
        Background(root);
        CreateText(root, "Instruction", "인체보호기준 대비 전자파 비율이 더 높을 것 같은 쪽을 고르세요!",
            new Vector2(0, 690), new Vector2(3400, 100), 66, TextColor);
        CreateText(root, "VS", "VS", new Vector2(0, -50), new Vector2(320, 260), 200, AccentColor, true);

        CreateText(root, "Hint_Left", "◀ 왼쪽 선택", new Vector2(-950, -870), new Vector2(1000, 110), 70, TextColor, true);
        CreateText(root, "Hint_Right", "오른쪽 선택 ▶", new Vector2(950, -870), new Vector2(1000, 110), 70, TextColor, true);
        CreateText(root, "Notice", "※ 실제 측정값과 순위는 토너먼트가 끝난 뒤 공개됩니다.",
            new Vector2(0, -1000), new Vector2(3000, 80), 48, SubTextColor);
    }

    private static void Result(RectTransform root)
    {
        Background(root);
        CreateText(root, "Title", "최종 결과", new Vector2(0, 930), new Vector2(2000, 160), 120, TextColor, true);
        CreateText(root, "Arrow_1", "▶", new Vector2(-600, 150), new Vector2(150, 150), 100, AccentColor);
        CreateText(root, "Arrow_2", "▶", new Vector2(600, 150), new Vector2(150, 150), 100, AccentColor);
    }

    private static void Ranking(RectTransform root)
    {
        Background(root);
        CreateRoundBox(root, "RankListBox", new Vector2(-900, -40), new Vector2(1800, 1700), CardColor);
        CreateText(root, "TopLabel", "실제 1위", new Vector2(500, 800), new Vector2(820, 120), 72, AccentColor, true);
        CreateText(root, "Source", "※ 국립전파연구원 측정자료 기준", new Vector2(950, -880), new Vector2(1700, 80), 44, SubTextColor);
        CreateText(root, "NextHint", "스위치를 눌러 다음 ▶", new Vector2(950, -990), new Vector2(1700, 100), 60, TextColor, true);
    }

    private static void Guide(RectTransform root)
    {
        Background(root);
        CreateText(root, "Title", "전자파 기준과 측정 이해", new Vector2(0, 940), new Vector2(3000, 150), 120, TextColor, true);

        // 왼쪽: 주파수별 인체보호기준
        RectTransform freqBox = CreateRoundBox(root, "Box_Frequency", new Vector2(-940, -40), new Vector2(1780, 1640), CardColor);
        CreateText(freqBox, "Header", "왜 주파수마다 인체보호기준이 다를까요?", new Vector2(0, 700), new Vector2(1700, 120), 76, AccentColor, true);

        CreateText(freqBox, "Label_Low", "저주파", new Vector2(-650, 420), new Vector2(300, 80), 56, SubTextColor, true);
        CreateText(freqBox, "Label_High", "고주파", new Vector2(650, 420), new Vector2(300, 80), 56, SubTextColor, true);

        RectTransform spectrum = Place(CreateRect("Spectrum", freqBox), new Vector2(0, 320), new Vector2(1500, 60));
        Color[] spectrumColors =
        {
            new Color32(229, 72, 77, 255),
            new Color32(245, 158, 66, 255),
            new Color32(250, 214, 80, 255),
            new Color32(94, 196, 120, 255),
            new Color32(77, 148, 235, 255),
            new Color32(150, 100, 230, 255),
        };
        for (int i = 0; i < spectrumColors.Length; i++)
        {
            float from = (float)i / spectrumColors.Length;
            float to = (float)(i + 1) / spectrumColors.Length;
            AddImage(Stretch(CreateRect($"Segment_{i + 1}", spectrum), new Vector2(from, 0), new Vector2(to, 1)), spectrumColors[i]);
        }

        string[] ticks = { "60 Hz", "1 kHz", "1 MHz", "1 GHz" };
        for (int i = 0; i < ticks.Length; i++)
            CreateText(freqBox, $"Tick_{i + 1}", ticks[i], new Vector2(-600 + i * 400, 220), new Vector2(360, 80), 52, TextColor, true);

        string[] examples = { "전력설비\n(전기‧가전)", "생활가전\n(인덕션 등)", "통신‧무선기기" };
        for (int i = 0; i < examples.Length; i++)
            CreateText(freqBox, $"Example_{i + 1}", examples[i], new Vector2(-550 + i * 550, 50), new Vector2(500, 180), 52, SubTextColor);

        CreateText(freqBox, "Body",
            "전자파는 주파수에 따라 인체와 상호작용하는 방식이 다릅니다.\n\n" +
            "낮은 주파수의 전자파는 몸속에 약한 전류를 유도하고, " +
            "높은 주파수의 전자파는 몸의 온도를 높이는 열작용을 일으킵니다.\n\n" +
            "그래서 인체보호기준도 주파수별로 다르게 정해져 있습니다.",
            new Vector2(0, -420), new Vector2(1600, 620), 60, TextColor, false, TextAlignmentOptions.TopLeft);

        // 오른쪽: 측정할 때 알아둘 점
        RectTransform measureBox = CreateRoundBox(root, "Box_Measure", new Vector2(940, -40), new Vector2(1780, 1640), CardColor);
        CreateText(measureBox, "Header", "측정할 때 알아둘 점", new Vector2(0, 700), new Vector2(1700, 120), 76, AccentColor, true);

        string[] factors = { "주파수", "측정거리", "측정위치", "작동상태" };
        for (int i = 0; i < factors.Length; i++)
        {
            RectTransform item = CreateRoundBox(measureBox, $"Factor_{i + 1}", new Vector2(-600 + i * 400, 400), new Vector2(360, 260), SoftBoxColor);
            FillText(item, "Text", factors[i], 62, TextColor, true);
        }

        CreateText(measureBox, "Body",
            "전자파 측정값은 주파수뿐 아니라 측정거리, 측정위치, 제품의 작동상태에 따라 달라질 수 있습니다.\n\n" +
            "같은 제품이라도 가까이에서, 강하게 작동할 때 더 높게 측정됩니다.",
            new Vector2(0, -80), new Vector2(1600, 480), 60, TextColor, false, TextAlignmentOptions.TopLeft);

        RectTransform summary = CreateRoundBox(measureBox, "Summary", new Vector2(0, -590), new Vector2(1600, 300), new Color(1f, 0.83f, 0.3f, 0.15f));
        FillText(summary, "Text",
            "이번 체험은 서로 다른 주파수의 제품을 공정하게 비교하기 위해\n<color=#FFD34D>인체보호기준 대비 %</color>를 공통 비교 기준으로 사용했습니다.",
            56, TextColor, true);

        CreateText(root, "Hint_Prev", "◀ 순위 다시 보기", new Vector2(-940, -990), new Vector2(1000, 100), 60, TextColor, true);
        CreateText(root, "Hint_End", "체험 종료 ▶", new Vector2(940, -990), new Vector2(1000, 100), 60, AccentColor, true);
    }
}
