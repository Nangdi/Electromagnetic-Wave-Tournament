# Unity UI 프로젝트 구성 가이드

> 전시‧체험형 Unity 앱(고정 해상도, 스위치/시리얼 입력, 디자이너가 그림을 따로 주는 구조)을 새로 시작할 때 쓰는 기준 문서.
> 전자파 토너먼트 프로젝트(Unity 2022.3, UGUI + TextMeshPro, 3840x2160)에서 정리했다.

---

## 1. 핵심 원칙

1. **화면 = 정적 레이어 1장 + 동적 요소들**
   - 변하지 않는 것(배경, 제목, 설명글, 고정 안내 문구, 장식)은 화면마다 **PNG 한 장**에 넣는다.
   - 런타임에 바뀌거나, 효과가 들어가거나, 따로 나타나는 것만 개별 오브젝트로 둔다.
2. **이미지 교체 = 같은 경로에 같은 파일명으로 덮어쓰기**
   - Unity는 `.meta`의 GUID로 연결하므로, 덮어쓰면 씬이나 프리팹을 수정하지 않아도 반영된다.
   - 파일을 지웠다가 새로 넣으면 GUID가 바뀌어 연결이 끊긴다. **절대 삭제 후 재생성하지 않는다.**
3. **재사용 단위는 프리팹**: 부품(Parts) → 패널(Panels) → 캔버스(Canvas) 3단 중첩.
4. **처음부터 끝까지 도구로 생성**: 에디터 스크립트가 임시 이미지, 프리팹, 씬 배치를 만든다. 디자인이 없어도 바로 실행해 볼 수 있다.
5. **도구는 이미 있는 것을 덮어쓰지 않는다**: 사람이 손으로 고친 프리팹과 디자이너가 넣은 PNG를 보호한다.
6. **런타임 코드에는 레이아웃 값을 두지 않는다**: 위치, 크기, 색은 프리팹에 두고 코드는 데이터 표시와 흐름만 담당한다.

---

## 2. 폴더 구조 (템플릿)

```
Assets/
├─ 1.Scene/                      StartScene.unity  (씬에는 Canvas 프리팹 1개 + 시스템 오브젝트)
├─ 2.Model/Prefabs/<프로젝트>/
│   ├─ Parts/                    재사용 부품 (카드, 리스트 한 줄, 점 …)
│   ├─ Panels/                   화면 단위 Panel_xxx.prefab
│   └─ Canvas_<프로젝트>.prefab  패널 전체를 중첩
├─ 3.Script/
│   ├─ <기능>/                   런타임 (Flow, Data, Input …)
│   │   └─ UI/                   Panel, View 스크립트
│   └─ Editor/<프로젝트>UI/      에디터 도구 (아래 6장)
├─ 4.Sprite/
│   ├─ Static/                   Static_<패널>.png  (전체 화면 해상도)
│   ├─ Parts/                    동적 요소용 부품 이미지
│   ├─ _Guide/                   디자이너 작업 가이드 (빌드 제외: 아무도 참조하지 않음)
│   └─ README.md                 디자이너용 교체 안내
├─ 10.Font/                      TMP 폰트 에셋
└─ StreamingAssets/              외부 데이터(JSON, 이미지)
```

- 이름 규칙: `Panel_<이름>.prefab`, `Static_<이름>.png`, `Guide_<이름>.png`처럼 **패널 이름 하나로 세 파일이 연결**되게 한다.

---

## 3. 정적 / 동적 분류 기준

아래 질문 중 **하나라도 "예"면 동적 요소**, 모두 "아니오"면 정적 레이어에 굽는다.

| 질문 | 예시 |
|---|---|
| 런타임 데이터로 내용이 바뀌나? | 후보 이름, 점수, 순위, 남은 시간 |
| 애니메이션이나 효과가 들어가나? | 깜빡이는 시작 버튼, 선택 시 확대되는 카드 |
| 화면의 나머지와 **다른 시점에** 나타나거나 사라지나? | 결과 화면에서 순서대로 나타나는 그룹과 그 라벨 |
| 개수가 데이터에 따라 달라지나? | 순위 줄, 페이지 점 |
| 사용자가 직접 누르거나 조작하나? | 버튼, 슬라이더 |

**주의할 것**
- 숫자가 박힌 고정 문구(예: "6개 공간 중 하나를 선택하세요")는 정적으로 구워도 된다. 다만 데이터 개수가 바뀌면 PNG도 바꿔야 한다는 점을 README에 적어 둔다.
- 순차 등장 그룹 안의 라벨은 글자가 고정이어도 **그룹과 같이 나타나야** 하므로 개별 스프라이트로 뺀다.
- 동적 요소라도 **안의 글자가 고정**이면(시작 버튼 문구 등) 글자까지 이미지 한 장으로 구운 스프라이트를 쓴다.

---

## 4. 프리팹 구조

```
Canvas_<프로젝트>.prefab            Canvas(Overlay) + CanvasScaler(기준 해상도, match 0.5) + GraphicRaycaster
 ├ Panel_Idle.prefab                 (편집 편의상 첫 패널만 활성)
 │   ├ StaticLayer                   Image, 전체 Stretch, Static_Idle.png, raycastTarget off
 │   └ StartPrompt                   동적 요소
 ├ Panel_Match.prefab
 │   ├ StaticLayer
 │   ├ RoundText (TMP)
 │   ├ Card_Left   ← Parts/CandidateCard.prefab 인스턴스
 │   └ Card_Right  ← Parts/CandidateCard.prefab 인스턴스
 └ …
```

- **StaticLayer는 항상 패널의 첫 번째 자식**(맨 뒤에 그려짐)이고 이름을 고정한다. 검사 도구와 가이드 생성이 이 이름을 찾는다.
- **패널 루트에는 Image를 두지 않는다.** 배경은 StaticLayer가 맡는다.
- **부품 변형은 Prefab Variant로 만든다.** Variant에서는 자식을 삭제할 수 없으므로 **비활성화**로 처리한다. 예: 측정정보 없는 카드 = `Info` 비활성 + 사진 영역 앵커 조정.
- **배치마다 다른 값은 인스턴스 오버라이드로 둔다.** 예: 가운데 공간 카드만 크기, 테두리, 글자색이 다름.
- **개수가 변하는 목록은 런타임에 생성한다.** `rowPrefab` + `rowRoot`(LayoutGroup)를 받아 필요한 만큼 `Instantiate`하고, 남는 것은 숨긴다.

```csharp
[SerializeField] private RankRowView rowPrefab;
[SerializeField] private Transform rowRoot;
private readonly List<RankRowView> rows = new List<RankRowView>();

private void EnsureRows(int count)
{
    while (rows.Count < count)
        rows.Add(Instantiate(rowPrefab, rowRoot));
}
```

---

## 5. 런타임 스크립트 구성

| 역할 | 클래스 예 | 규칙 |
|---|---|---|
| 흐름(상태 머신) | `TournamentFlowController` | `enum FlowState` + `GoTo(state)`. 패널 켜고 끄기를 한곳에서 처리 |
| 패널 베이스 | `TournamentPanel` | `virtual Show()/Hide()`. 전환 연출을 넣을 때 여기만 바꾸면 됨 |
| 패널 | `MatchPanel` 등 | 데이터를 받아 View에 넘기고, 연출은 코루틴으로 처리 |
| 뷰(부품) | `CandidateCardView` 등 | `[SerializeField]` 참조만 두고 `Set(data)`로 표시 |
| 입력 추상화 | `SwitchInput` | 키보드(테스트용)와 시리얼 입력을 모두 `Press(side)` 한 곳으로 모음. 디바운스 포함 |
| 데이터 | `CandidateDatabase` | `StreamingAssets` JSON을 로드하고, 이미지는 경로로 지연 로드 |
| 설정 | `TournamentConfig.Current` | 시간, 개수 등 운영 값은 외부 JSON으로 관리 |

- 상호작용이 없을 때 대기 화면으로 돌아가는 기능(타임아웃)은 컨트롤러 `Update`에 둔다. 전시장에는 필수다.
- 카드 정보처럼 라벨과 값이 섞인 텍스트는 TMP `<pos=5em>` 태그로 값의 시작 위치를 맞춘다.

---

## 6. 에디터 도구 (`Assets/3.Script/Editor/<프로젝트>UI/`)

| 파일 | 역할 |
|---|---|
| `XxxUIKit.cs` | 경로 상수, 색, 폰트, `CreateRect/Place/Stretch/AddText/AddSprite/Wire` 생성 헬퍼 |
| `XxxStaticLayouts.cs` | 패널별 정적 내용과, 글자가 들어간 부품 스프라이트의 **임시 디자인** 정의 (PNG로 굽기만 함) |
| `XxxSpriteExporter.cs` | 임시 PNG 렌더링, 둥근 박스 생성, 가이드 PNG 생성 |
| `XxxPrefabBuilder.cs` | 부품 → 패널 → 캔버스 프리팹 생성 및 씬 적용 (**없는 것만 생성**) |
| `XxxUIValidator.cs` | 정적 PNG 누락, 비어 있는 SerializeField, Missing 스크립트 검사 |
| `XxxSpriteImporter.cs` | `AssetPostprocessor`: `4.Sprite` 아래 이미지 임포트 설정 고정 |

**메뉴 구성** (`Tools/<프로젝트>/…`)
1. 프리팹 생성 및 씬 적용
2. 가이드 PNG 다시 만들기
3. 임시 스프라이트 다시 내보내기 (덮어쓰기, 확인 창 표시)
4. 연결 검사

> 메뉴 함수는 `public static`으로 열어 둔다. 확인 창(`DisplayDialog`)이 있는 메뉴는 자동화(MCP 등)에서 멈추므로, **확인 창 없는 본체 함수를 따로** 둔다.
> 예: `Export(bool overwrite)` / 메뉴용 `ExportAllMenu()`

### 6-1. 없는 것만 만드는 프리팹 생성

```csharp
private static GameObject GetOrCreate(string path, Func<GameObject> build)
{
    var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    if (existing != null) return existing;          // 손으로 고친 프리팹 보호

    GameObject temp = build();
    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
    Object.DestroyImmediate(temp);
    return prefab;
}
```
- 중첩 프리팹은 `PrefabUtility.InstantiatePrefab(partPrefab, parent)`로 넣어야 링크가 유지된다. `Object.Instantiate`를 쓰면 링크가 끊긴다.
- Variant는 base 인스턴스를 수정한 뒤 `SaveAsPrefabAsset(instance, path)`로 저장하면 자동으로 Variant가 된다.
- 씬 적용 시, 프리팹이 아닌 예전 캔버스가 있으면 교체하고 이미 프리팹 인스턴스면 그대로 둔다. 컨트롤러의 패널 참조는 `GetComponentInChildren<T>(true)`로 다시 연결한다.
- 프리팹을 초기화하고 싶으면 `.prefab` 파일을 지우고 메뉴를 다시 실행한다. PNG는 지우지 않는다.

### 6-2. UI를 PNG로 굽기 (프리뷰 씬 + 월드 스페이스 캔버스)

현재 씬을 건드리지 않도록 프리뷰 씬에서 렌더링한다.

```csharp
Scene scene = EditorSceneManager.NewPreviewScene();
// 카메라: orthographic, orthographicSize = height/2, cam.scene = scene, targetTexture = RT(width,height)
// 캔버스: RenderMode.WorldSpace, sizeDelta = (width,height), 위치 (0,0,0)
build(root);
LayoutRebuilder.ForceRebuildLayoutImmediate(root);
Canvas.ForceUpdateCanvases();
foreach (var t in root.GetComponentsInChildren<TMP_Text>(true)) t.ForceMeshUpdate(true, true);
cam.Render();  // → ReadPixels → EncodeToPNG
EditorSceneManager.ClosePreviewScene(scene);
```

**투명 배경 부품**(글자 라벨 등)은 검은 배경과 흰 배경에 두 번 찍어서 정확한 알파를 복원한다.
```csharp
// 결과색 = c*a + bg*(1-a)  →  (흰배경 - 검은배경) = 1 - a
float a = 1f - ((w.r - b.r) + (w.g - b.g) + (w.b - b.b)) / (3f * 255f);
color = b / a;   // a가 0에 가까우면 완전 투명 처리
```

- 둥근 박스 부품은 렌더링하지 않고 SDF로 직접 그린다(안티에일리어싱 포함). 반지름 = 9-slice Border 크기.
- 9-slice Border는 **파일을 처음 만들 때만** 설정한다. 이후 디자이너가 Sprite Editor에서 바꾼 값을 지키기 위해서다.
- 코드가 색을 칠하는 부품(점, 리스트 줄 배경)은 **흰색**으로 만들고 `Image.color`로 칠한다.

### 6-3. 가이드 PNG

패널 프리팹을 프리뷰 씬에 띄운 뒤, `StaticLayer`를 뺀 직속 자식마다 반투명 분홍 박스, 테두리, 이름 라벨을 덮어 찍는다.
- 박스는 대상의 자식으로 Stretch해서 붙이고, `LayoutElement.ignoreLayout = true`를 준다. LayoutGroup 안에서 한 줄로 끼어드는 것을 막기 위해서다.
- 디자이너는 이 이미지 위에서 작업하면 동적 요소 자리를 실수로 덮지 않는다.

### 6-4. 임포트 설정 고정

```csharp
public class XxxSpriteImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/4.Sprite/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.maxTextureSize = 4096;
        // 전체 화면 이미지는 압축, 작은 부품은 무압축 (크기가 4의 배수가 아니면 블록 압축이 안 됨)
        ti.textureCompression = assetPath.StartsWith("Assets/4.Sprite/Parts/")
            ? TextureImporterCompression.Uncompressed
            : TextureImporterCompression.CompressedHQ;
    }
}
```

### 6-5. 연결 검사

- 프로젝트 스크립트(`Assembly-CSharp`)의 컴포넌트만 `SerializedObject`로 순회해서, 비어 있는 `ObjectReference`를 경로와 함께 경고로 출력한다.
- 패널마다 `StaticLayer` Image에 스프라이트가 있는지, 캔버스가 프리팹 인스턴스인지도 확인한다.
- 문제 수를 `int`로 반환해서 자동화에서도 판단할 수 있게 한다.

---

## 7. 디자이너 전달물

`4.Sprite/README.md`에 아래 내용을 표로 정리해 둔다.
- 파일명별 용도와 **정확한 픽셀 크기** (스프라이트 크기와 배치 영역 크기를 맞춘다. `preserveAspect`로 줄어드는 것을 막기 위해서다)
- "덮어쓰기만 하고 지우지 말 것"
- 9-slice 부품과 흰색으로 그려야 하는 부품 표시
- 가이드 PNG의 분홍 박스 자리는 비워 둘 것

---

## 8. 새 프로젝트 초기 구성 순서

1. **프로젝트 설정**
   - 기준 해상도를 정한다 (예: 3840x2160). `CanvasScaler`는 ScaleWithScreenSize, match 0.5.
   - TMP Essentials를 임포트하고, 한글 폰트 SDF 에셋을 만든다(사용 글자 범위 확인).
   - `TMP Settings` → **Use Modern Hangul Line Breaking Rules 켜기** (한글이 글자 단위로 끊기는 것을 막음).
   - Color Space를 확인한다. PNG 렌더링 결과가 의도대로 나오는지 첫 내보내기 후 비교한다.
2. **폴더 생성** (2장 구조)
3. **런타임 뼈대**: 상태 머신 컨트롤러, 패널 베이스, 입력 추상화, 데이터 로더, 설정
4. **화면 목록과 분류표 작성** (3장 기준으로 패널별 정적/동적 표)
5. **에디터 도구 6종 작성** (6장). 이전 프로젝트에서 복사하고 경로 상수와 레이아웃만 수정한다.
6. **1번 메뉴 실행** → 임시 PNG, 프리팹, 씬 배치, 가이드 생성
7. **4번 메뉴로 검사** → 문제 0건 확인
8. **플레이 테스트** (9장)
9. 디자이너에게 `_Guide/`와 `README.md` 전달 → 받은 PNG를 덮어쓰기

---

## 9. 검증 체크리스트

- [ ] 컴파일 에러, 경고 없음
- [ ] 연결 검사 0건
- [ ] 키보드 입력으로 전체 흐름 한 바퀴 (모든 상태 → 대기 복귀)
- [ ] 타임아웃으로 대기 화면 자동 복귀
- [ ] 화면별 캡처를 이전 디자인과 비교 (Overlay 캔버스는 **플레이 모드에서만** 화면 캡처에 찍힘)
- [ ] 정적 PNG 하나를 다른 이미지로 덮어써서 반영되는지 확인 → 원래대로 복원
- [ ] 생성 메뉴를 다시 실행해도 프리팹이 덮어써지지 않고 캔버스가 하나만 있는지 확인
- [ ] 런타임 생성 목록(줄, 점)의 개수가 데이터와 같은지 확인

---

## 10. 겪었던 문제와 해결

| 증상 | 원인 / 해결 |
|---|---|
| 한글이 "작/동상태"처럼 글자 중간에서 줄바꿈됨 | TMP Settings의 Modern Hangul Line Breaking Rules가 꺼져 있었음 → 켬 |
| 가운뎃점(·)이 □로 나옴 | NotoSansKR SDF 에셋에 U+00B7가 없음 → `‧`(U+2027) 사용 또는 폰트 에셋에 글자 추가 |
| 선택 시 커진 카드가 가운데 "VS"를 가림 | 확대 배율(1.06)을 고려해 카드 간격을 둠 |
| 라벨과 값의 세로 정렬이 어긋남 | 공백 정렬 대신 TMP `<pos=5em>` 사용 |
| 라벨 스프라이트가 작게 보임 | 스프라이트 크기(150)와 배치 영역(125)이 달라 `preserveAspect`로 축소됨 → 크기를 일치시킴 |
| 작은 PNG 압축 경고 | 블록 압축은 4의 배수 크기만 가능 → 부품은 무압축 |
| 에디터에서 찍은 스크린샷에 UI가 없음 | Overlay 캔버스는 편집 모드 카메라 캡처에 나오지 않음 → 플레이 모드에서 캡처 |
| 메뉴 자동 실행이 멈춤 | `DisplayDialog`가 응답을 기다림 → 확인 창 없는 public static 함수를 따로 둠 |
| Variant에서 자식 삭제 불가 | 비활성화로 처리 |
| 이미지를 바꿨더니 연결이 끊김 | 삭제 후 새로 추가해서 GUID가 바뀜 → 반드시 덮어쓰기 |

---

## 11. (선택) Claude Code + Unity MCP로 작업할 때

- 스크립트를 수정한 뒤에는 `refresh_unity(compile=request)` → `read_console`로 에러를 먼저 확인한다.
- 에디터 도구 함수는 `execute_code`로 직접 호출한다. 메뉴 실행보다 결과값을 바로 받을 수 있다.
- 플레이 테스트에서는 `SwitchInput.Instance.Press(...)`로 입력을 흉내 낸다. 연속 입력은 `EditorApplication.update` 콜백으로 간격을 두고 넣는다(디바운스 때문).
- 플레이 모드에 들어간 직후에는 도메인 리로드 때문에 게임 시간이 늦게 흐를 수 있다. 시간 기반 테스트는 `Time.unscaledTime`으로 실제로 흐르는지 확인한다.
