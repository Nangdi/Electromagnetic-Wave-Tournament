using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TournamentUIKit;

// 전자파 토너먼트 UI 프리팹을 만들고 현재 씬에 배치한다.
// 메뉴: Tools > 전자파 토너먼트 > 1. 프리팹 생성 및 씬 적용
//
// 구조
//  Canvas_Tournament.prefab
//   └ Panel_xxx.prefab (패널 6개)
//      ├ StaticLayer  : 정적 PNG 한 장 (4.Sprite/Static/Static_xxx.png)
//      └ 동적 요소    : 텍스트, 카드(Parts 프리팹), 게이지 등
//
// 이미 있는 프리팹은 덮어쓰지 않는다. 직접 수정한 프리팹을 지키기 위해서다.
// 프리팹을 처음 상태로 되돌리고 싶으면 해당 .prefab 파일을 지운 뒤 다시 실행한다.
public static class TournamentPrefabBuilder
{
    private const string CanvasName = "Canvas_Tournament";
    private const string SystemName = "TournamentSystem";

    private static GameObject dotPrefab;
    private static GameObject rankRowPrefab;
    private static GameObject cardPrefab;
    private static GameObject cardSimplePrefab;
    private static GameObject spaceCardPrefab;

    [MenuItem("Tools/전자파 토너먼트/1. 프리팹 생성 및 씬 적용", priority = 1)]
    public static void BuildAll()
    {
        TournamentSpriteExporter.Export(overwrite: false);
        EnsureFolder(PartPrefabDir);
        EnsureFolder(PanelPrefabDir);

        // 부품
        dotPrefab = GetOrCreate(PartPrefabPath("Dot"), CreateDot);
        rankRowPrefab = GetOrCreate(PartPrefabPath("RankRow"), CreateRankRow);
        cardPrefab = GetOrCreate(PartPrefabPath("CandidateCard"), CreateCandidateCard);
        cardSimplePrefab = GetOrCreate(PartPrefabPath("CandidateCard_Simple"), CreateCandidateCardSimple);
        spaceCardPrefab = GetOrCreate(PartPrefabPath("SpaceCard"), CreateSpaceCard);

        // 패널
        var panelBuilders = new Dictionary<string, Func<GameObject>>
        {
            { "Idle", CreateIdlePanel },
            { "MapSelect", CreateMapSelectPanel },
            { "Match", CreateMatchPanel },
            { "Result", CreateResultPanel },
            { "Ranking", CreateRankingPanel },
            { "Guide", CreateGuidePanel },
        };
        var panels = new List<GameObject>();
        foreach (string panel in PanelNames)
            panels.Add(GetOrCreate(PanelPrefabPath(panel), panelBuilders[panel]));

        GameObject canvasPrefab = GetOrCreate(CanvasPrefabPath, () => CreateCanvas(panels));

        ApplyToScene(canvasPrefab);
        TournamentSpriteExporter.ExportGuides();
        AssetDatabase.SaveAssets();
        Debug.Log("전자파 토너먼트 프리팹 적용 완료. 씬을 저장(Ctrl+S)하세요.");
    }

    private static GameObject GetOrCreate(string path, Func<GameObject> build)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject temp = build();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        UnityEngine.Object.DestroyImmediate(temp);
        Debug.Log($"프리팹 생성: {path}");
        return prefab;
    }

    private static RectTransform Instantiate(GameObject prefab, RectTransform parent, string name)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;
        return (RectTransform)go.transform;
    }

    private static Sprite Part(string name) => LoadSprite(PartSpritePath(name));

    #region 부품 프리팹

    private static GameObject CreateDot()
    {
        RectTransform root = CreateRect("Dot", null);
        root.sizeDelta = new Vector2(40, 40);
        AddSprite(root, Part("Dot"), DotOffColor);
        return root.gameObject;
    }

    private static GameObject CreateRankRow()
    {
        RectTransform root = CreateRect("RankRow", null);
        root.sizeDelta = new Vector2(1740, 96);
        Image background = AddSprite(root, Part("RankRow_Bg"), RankRowColor);

        TMP_Text rankText = AddText(Stretch(CreateRect("Rank", root), Vector2.zero, new Vector2(0.14f, 1f)), "1위", 54, AccentColor, true);
        TMP_Text nameText = AddText(Stretch(CreateRect("Name", root), new Vector2(0.15f, 0f), new Vector2(0.62f, 1f)),
            "제품명", 54, TextColor, false, TextAlignmentOptions.MidlineLeft);
        TMP_Text ratioText = AddText(Stretch(CreateRect("Ratio", root), new Vector2(0.62f, 0f), new Vector2(0.8f, 1f)),
            "0.00%", 54, TextColor, true, TextAlignmentOptions.MidlineRight);
        TMP_Text mark = AddText(Stretch(CreateRect("MyPickMark", root), new Vector2(0.81f, 0f), Vector2.one),
            "← 내 선택", 44, AccentColor, true, TextAlignmentOptions.MidlineLeft);
        mark.gameObject.SetActive(false);

        var view = root.gameObject.AddComponent<RankRowView>();
        Wire(view,
            ("background", background),
            ("rankText", rankText),
            ("nameText", nameText),
            ("ratioText", ratioText),
            ("myPickMark", mark.gameObject));
        return root.gameObject;
    }

    // 후보 카드: Highlight(선택 테두리) / Background / Name / PictureArea(Picture, Placeholder) / Info
    private static GameObject CreateCandidateCard()
    {
        RectTransform root = CreateRect("CandidateCard", null);
        root.sizeDelta = new Vector2(1400, 1300);
        var canvasGroup = root.gameObject.AddComponent<CanvasGroup>();

        RectTransform highlight = Stretch(CreateRect("Highlight", root), Vector2.zero, Vector2.one, -16);
        AddSprite(highlight, Part("Card_Highlight"));
        highlight.gameObject.SetActive(false);

        AddSprite(StretchFull(CreateRect("Background", root)), Part("Card_Bg"));

        TMP_Text nameText = AddText(Stretch(CreateRect("Name", root), new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f)),
            SampleCandidate.name, 90, TextColor, true);
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 40;
        nameText.fontSizeMax = 90;

        RectTransform pictureArea = Stretch(CreateRect("PictureArea", root), new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.84f));
        AddSprite(pictureArea, Part("Card_PictureBg"));
        Image picture = AddImage(Stretch(CreateRect("Picture", pictureArea), Vector2.zero, Vector2.one, 20), Color.white);
        picture.preserveAspect = true;
        picture.enabled = false; // 이미지가 들어오면 런타임에 켜진다
        TMP_Text placeholder = FillText(pictureArea, "Placeholder", "이미지 준비 중", 56, SubTextColor);

        TMP_Text infoText = AddText(Stretch(CreateRect("Info", root), new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.4f)),
            CandidateFormat.Info(SampleCandidate, false), 60, TextColor, false, TextAlignmentOptions.MidlineLeft);
        infoText.enableAutoSizing = true;
        infoText.fontSizeMin = 28;
        infoText.fontSizeMax = 60;

        var view = root.gameObject.AddComponent<CandidateCardView>();
        Wire(view,
            ("picture", picture),
            ("placeholderText", placeholder),
            ("nameText", nameText),
            ("infoText", infoText),
            ("highlight", highlight.gameObject),
            ("canvasGroup", canvasGroup));
        return root.gameObject;
    }

    // 측정 정보 없이 이름과 이미지만 보이는 카드 (CandidateCard의 Variant)
    private static GameObject CreateCandidateCardSimple()
    {
        var root = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab);
        root.name = "CandidateCard_Simple";

        root.transform.Find("Info").gameObject.SetActive(false);

        var pictureArea = (RectTransform)root.transform.Find("PictureArea");
        pictureArea.anchorMin = new Vector2(0.1f, 0.06f);
        return root;
    }

    // 공간 카드. 가운데 카드는 패널에서 Highlight를 켜고 크기/글자색을 바꾼다.
    private static GameObject CreateSpaceCard()
    {
        RectTransform root = CreateRect("SpaceCard", null);
        root.sizeDelta = new Vector2(900, 1000);
        root.gameObject.AddComponent<CanvasGroup>();

        RectTransform highlight = Stretch(CreateRect("Highlight", root), Vector2.zero, Vector2.one, -16);
        AddSprite(highlight, Part("Card_Highlight"));
        highlight.gameObject.SetActive(false);

        AddSprite(StretchFull(CreateRect("Background", root)), Part("Card_Bg"));

        RectTransform pictureArea = Stretch(CreateRect("PictureArea", root), new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.94f));
        AddSprite(pictureArea, Part("Card_PictureBg"));
        Image picture = AddImage(Stretch(CreateRect("Picture", pictureArea), Vector2.zero, Vector2.one, 20), Color.white);
        picture.preserveAspect = true;
        picture.enabled = false;

        TMP_Text nameText = AddText(Stretch(CreateRect("Name", root), new Vector2(0f, 0.03f), new Vector2(1f, 0.2f)),
            "공간 이름", 96, TextColor, true);

        var view = root.gameObject.AddComponent<SpaceCardView>();
        Wire(view, ("picture", picture), ("nameText", nameText));
        return root.gameObject;
    }

    #endregion

    #region 패널 프리팹

    private static RectTransform CreatePanelRoot(string panel)
    {
        RectTransform root = StretchFull(CreateRect($"Panel_{panel}", null));
        Image staticLayer = AddSprite(StretchFull(CreateRect(StaticLayerName, root)), LoadSprite(StaticSpritePath(panel)));
        staticLayer.type = Image.Type.Simple;
        return root;
    }

    private static GameObject CreateIdlePanel()
    {
        RectTransform panel = CreatePanelRoot("Idle");

        RectTransform prompt = Place(CreateRect("StartPrompt", panel), new Vector2(0, -600), new Vector2(1400, 200));
        AddSprite(prompt, Part("Button_Start")).preserveAspect = true;
        var promptGroup = prompt.gameObject.AddComponent<CanvasGroup>();

        var view = panel.gameObject.AddComponent<IdlePanel>();
        Wire(view, ("startPrompt", promptGroup));
        return panel.gameObject;
    }

    private static GameObject CreateMapSelectPanel()
    {
        RectTransform panel = CreatePanelRoot("MapSelect");

        RectTransform prev = Place(Instantiate(spaceCardPrefab, panel, "Card_Prev"), new Vector2(-1250, 60), new Vector2(900, 1000));
        RectTransform center = Place(Instantiate(spaceCardPrefab, panel, "Card_Center"), new Vector2(0, 60), new Vector2(1200, 1200));
        RectTransform next = Place(Instantiate(spaceCardPrefab, panel, "Card_Next"), new Vector2(1250, 60), new Vector2(900, 1000));

        prev.GetComponent<CanvasGroup>().alpha = 0.55f;
        next.GetComponent<CanvasGroup>().alpha = 0.55f;
        center.Find("Highlight").gameObject.SetActive(true);
        var centerName = center.Find("Name").GetComponent<TMP_Text>();
        centerName.color = AccentColor;
        centerName.fontSize = 120;

        // 페이지 점 (공간 수만큼 런타임에 생성)
        RectTransform dotsRoot = Place(CreateRect("Dots", panel), new Vector2(0, -640), new Vector2(800, 50));
        var layout = dotsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 40;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // 자동 선택 게이지
        TMP_Text gaugeText = CreateText(panel, "GaugeLabel", "선택 시간 10초", new Vector2(0, -750), new Vector2(1000, 90), 56, TextColor);
        RectTransform gauge = Place(CreateRect("Gauge", panel), new Vector2(0, -840), new Vector2(2000, 60));
        AddSprite(gauge, Part("Gauge_Bg"));
        RectTransform gaugeFill = StretchFull(CreateRect("Fill", gauge));
        AddSprite(gaugeFill, Part("Gauge_Fill"));

        var view = panel.gameObject.AddComponent<MapSelectPanel>();
        Wire(view,
            ("prevCard", prev.GetComponent<SpaceCardView>()),
            ("centerCard", center.GetComponent<SpaceCardView>()),
            ("nextCard", next.GetComponent<SpaceCardView>()),
            ("dotPrefab", dotPrefab.GetComponent<Image>()),
            ("dotRoot", dotsRoot),
            ("gaugeFill", gaugeFill),
            ("gaugeText", gaugeText));
        return panel.gameObject;
    }

    private static GameObject CreateMatchPanel()
    {
        RectTransform panel = CreatePanelRoot("Match");

        TMP_Text roundText = CreateText(panel, "RoundText", "16강 1경기", new Vector2(0, 910), new Vector2(2000, 170), 130, AccentColor, true);
        TMP_Text progressText = CreateText(panel, "ProgressText", "1 / 8", new Vector2(0, 790), new Vector2(800, 90), 64, SubTextColor);

        // 선택 시 카드가 1.06배로 커져도 가운데 VS를 가리지 않도록 간격을 둔다
        RectTransform left = Place(Instantiate(cardPrefab, panel, "Card_Left"), new Vector2(-950, -50), new Vector2(1400, 1300));
        RectTransform right = Place(Instantiate(cardPrefab, panel, "Card_Right"), new Vector2(950, -50), new Vector2(1400, 1300));

        var view = panel.gameObject.AddComponent<MatchPanel>();
        Wire(view,
            ("roundText", roundText),
            ("progressText", progressText),
            ("leftCard", left.GetComponent<CandidateCardView>()),
            ("rightCard", right.GetComponent<CandidateCardView>()));
        return panel.gameObject;
    }

    private static GameObject CreateResultPanel()
    {
        RectTransform panel = CreatePanelRoot("Result");
        var columnSize = new Vector2(1050, 1250);

        // ① 내가 선택한 우승자
        RectTransform myGroup = Place(CreateRect("Group_MyPick", panel), new Vector2(-1200, 150), columnSize);
        var myGroupCg = myGroup.gameObject.AddComponent<CanvasGroup>();
        GroupLabel(myGroup, "Result_Label_MyPick");
        RectTransform myCard = Stretch(Instantiate(cardSimplePrefab, myGroup, "Card_MyPick"), Vector2.zero, new Vector2(1, 0.88f));

        // 내 선택의 기준 대비 %
        RectTransform myRatioGroup = Place(CreateRect("Group_MyRatio", panel), new Vector2(-1200, -700), new Vector2(1050, 320));
        var myRatioCg = myRatioGroup.gameObject.AddComponent<CanvasGroup>();
        TMP_Text myRatioText = FillText(myRatioGroup, "Ratio", "0.91%\n<size=60%>(실제 7위)</size>", 140, AccentColor, true);

        // ② 실제 1위
        RectTransform topGroup = Place(CreateRect("Group_Top", panel), new Vector2(0, 150), columnSize);
        var topCg = topGroup.gameObject.AddComponent<CanvasGroup>();
        GroupLabel(topGroup, "Result_Label_Top");
        RectTransform topCard = Stretch(Instantiate(cardSimplePrefab, topGroup, "Card_Top"), Vector2.zero, new Vector2(1, 0.88f));
        TMP_Text topRatioText = CreateText(topGroup, "Ratio", "12.00%\n<size=60%>(실제 1위)</size>",
            new Vector2(0, -850), new Vector2(1050, 320), 140, AccentColor, true);

        // ③ 내 선택의 실제 순위
        RectTransform rankGroup = Place(CreateRect("Group_MyRank", panel), new Vector2(1200, 150), columnSize);
        var rankCg = rankGroup.gameObject.AddComponent<CanvasGroup>();
        GroupLabel(rankGroup, "Result_Label_MyRank");
        RectTransform rankBox = Stretch(CreateRect("Box", rankGroup), Vector2.zero, new Vector2(1, 0.88f));
        AddSprite(rankBox, Part("Card_Bg"));
        TMP_Text myRankText = FillText(rankBox, "Text",
            "내 선택은\n16개 중\n<size=180%><color=#FFD34D>7위</color></size>\n였습니다!", 100, TextColor, true);

        RectTransform hint = Place(CreateRect("NextHint", panel), new Vector2(0, -960), new Vector2(1600, 120));
        AddSprite(hint, Part("Result_NextHint")).preserveAspect = true;
        var hintCg = hint.gameObject.AddComponent<CanvasGroup>();

        var view = panel.gameObject.AddComponent<ResultPanel>();
        Wire(view,
            ("myPickGroup", myGroupCg),
            ("myPickCard", myCard.GetComponent<CandidateCardView>()),
            ("myRatioGroup", myRatioCg),
            ("myRatioText", myRatioText),
            ("topGroup", topCg),
            ("topCard", topCard.GetComponent<CandidateCardView>()),
            ("topRatioText", topRatioText),
            ("myRankGroup", rankCg),
            ("myRankText", myRankText),
            ("nextHint", hintCg));
        return panel.gameObject;
    }

    // 결과 그룹 위쪽 라벨. 그룹과 함께 나타나야 해서 정적 레이어가 아닌 개별 스프라이트로 둔다.
    private static void GroupLabel(RectTransform group, string sprite)
    {
        RectTransform label = Stretch(CreateRect("Label", group), new Vector2(0, 0.9f), Vector2.one);
        AddSprite(label, Part(sprite)).preserveAspect = true;
    }

    private static GameObject CreateRankingPanel()
    {
        RectTransform panel = CreatePanelRoot("Ranking");

        TMP_Text title = CreateText(panel, "Title", "실제 측정 순위 (전체 16개)", new Vector2(-900, 930), new Vector2(1800, 140), 96, TextColor, true);

        // 순위 줄 (토너먼트 크기만큼 런타임에 생성). 박스 배경은 정적 레이어에 있다.
        RectTransform list = Place(CreateRect("RankList", panel), new Vector2(-900, -40), new Vector2(1800, 1700));
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        RectTransform topDetail = Place(Instantiate(cardPrefab, panel, "Detail_Top"), new Vector2(500, -40), new Vector2(820, 1500));
        TMP_Text myLabel = CreateText(panel, "MyLabel", "나의 선택 (실제 7위)", new Vector2(1400, 800), new Vector2(820, 120), 72, AccentColor, true);
        RectTransform myDetail = Place(Instantiate(cardPrefab, panel, "Detail_MyPick"), new Vector2(1400, -40), new Vector2(820, 1500));

        // 미리보기용: 상세 카드는 기준 대비 %까지 보여준다
        string sampleInfo = CandidateFormat.Info(SampleCandidate, true);
        topDetail.Find("Info").GetComponent<TMP_Text>().text = sampleInfo;
        myDetail.Find("Info").GetComponent<TMP_Text>().text = sampleInfo;

        var view = panel.gameObject.AddComponent<RankingPanel>();
        Wire(view,
            ("rowPrefab", rankRowPrefab.GetComponent<RankRowView>()),
            ("rowRoot", list),
            ("titleText", title),
            ("topDetail", topDetail.GetComponent<CandidateCardView>()),
            ("myDetail", myDetail.GetComponent<CandidateCardView>()),
            ("myDetailLabel", myLabel));
        return panel.gameObject;
    }

    // 안내 화면은 상호작용이 없어 정적 레이어 한 장뿐이다.
    private static GameObject CreateGuidePanel()
    {
        RectTransform panel = CreatePanelRoot("Guide");
        panel.gameObject.AddComponent<GuidePanel>();
        return panel.gameObject;
    }

    #endregion

    #region 캔버스와 씬

    private static GameObject CreateCanvas(List<GameObject> panels)
    {
        var go = new GameObject(CanvasName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0; // 설정창(SettingsCanvas, 100)보다 아래

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        // 편집하기 쉽도록 대기 화면만 켜 둔다. 실행 시에는 컨트롤러가 단계에 맞게 켜고 끈다.
        for (int i = 0; i < panels.Count; i++)
        {
            var panel = (GameObject)PrefabUtility.InstantiatePrefab(panels[i], go.transform);
            panel.SetActive(i == 0);
        }
        return go;
    }

    private static void ApplyToScene(GameObject canvasPrefab)
    {
        Undo.SetCurrentGroupName("Apply Tournament UI");
        int undoGroup = Undo.GetCurrentGroup();

        GameObject canvas = FindRoot(CanvasName);

        // 예전 방식(프리팹이 아닌 오브젝트)으로 만든 캔버스는 프리팹으로 교체한다
        if (canvas != null && PrefabUtility.GetCorrespondingObjectFromSource(canvas) != canvasPrefab)
        {
            Undo.DestroyObjectImmediate(canvas);
            canvas = null;
        }

        if (canvas == null)
        {
            canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab, SceneManager.GetActiveScene());
            Undo.RegisterCreatedObjectUndo(canvas, "Create Tournament Canvas");
        }

        TournamentFlowController controller = SetupSystem();
        Wire(controller,
            ("idlePanel", canvas.GetComponentInChildren<IdlePanel>(true)),
            ("mapSelectPanel", canvas.GetComponentInChildren<MapSelectPanel>(true)),
            ("matchPanel", canvas.GetComponentInChildren<MatchPanel>(true)),
            ("resultPanel", canvas.GetComponentInChildren<ResultPanel>(true)),
            ("rankingPanel", canvas.GetComponentInChildren<RankingPanel>(true)),
            ("guidePanel", canvas.GetComponentInChildren<GuidePanel>(true)));

        SetupSerialManager();

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(canvas.scene);
        Selection.activeGameObject = canvas;
    }

    private static GameObject FindRoot(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == name)
                return root;
        return null;
    }

    private static TournamentFlowController SetupSystem()
    {
        GameObject go = FindRoot(SystemName);
        if (go == null)
        {
            go = new GameObject(SystemName);
            Undo.RegisterCreatedObjectUndo(go, "Create Tournament System");
        }

        GetOrAdd<CandidateDatabase>(go);
        GetOrAdd<SwitchInput>(go);
        return GetOrAdd<TournamentFlowController>(go);
    }

    // 씬의 기본 SerialPortManager를 스위치 입력을 처리하는 TournamentSerialManager로 교체한다.
    private static void SetupSerialManager()
    {
        if (UnityEngine.Object.FindObjectOfType<TournamentSerialManager>(true) != null) return;

        foreach (var manager in UnityEngine.Object.FindObjectsOfType<SerialPortManager>(true))
        {
            if (manager.GetType() != typeof(SerialPortManager)) continue;

            var go = manager.gameObject;
            Undo.DestroyObjectImmediate(manager);
            Undo.AddComponent<TournamentSerialManager>(go);
            Debug.Log($"{go.name}: SerialPortManager → TournamentSerialManager 교체");
            return;
        }

        var newGo = new GameObject("SerialPortManager");
        Undo.RegisterCreatedObjectUndo(newGo, "Create TournamentSerialManager");
        newGo.AddComponent<TournamentSerialManager>();
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(go);
    }

    #endregion
}
