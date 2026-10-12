# 전자파 토너먼트 UI 이미지 교체 가이드

같은 경로에 **같은 파일명으로 덮어쓰면** 씬/프리팹 수정 없이 바로 반영됩니다.
(파일을 지웠다가 새로 넣으면 연결이 끊어지니 반드시 덮어쓰기)

## Static/ — 화면별 정적 레이어 (3840 x 2160)
| 파일 | 화면 |
|---|---|
| Static_Idle.png | 대기 |
| Static_MapSelect.png | 공간 선택 |
| Static_Match.png | 토너먼트 경기 |
| Static_Result.png | 최종 결과 |
| Static_Ranking.png | 전체 순위 |
| Static_Guide.png | 기준‧측정 이해 (화면 전체) |

배경, 제목, 설명글, 고정 안내 문구 등 변하지 않는 것은 모두 이 한 장에 그립니다.
**_Guide/Guide_xxx.png** 의 분홍 박스 자리는 동적 요소(카드, 점수, 게이지 등)가 올라가는 곳이니 비워 두세요.

## Parts/ — 동적 요소 부품
| 파일 | 용도 | 비고 |
|---|---|---|
| Button_Start.png | 대기 화면 시작 버튼 (깜빡임) | 1400 x 200 |
| Result_Label_MyPick / Top / MyRank.png | 결과 화면 그룹 라벨 (순서대로 등장) | 1050 x 125 |
| Result_NextHint.png | 결과 화면 다음 안내 | 1600 x 120 |
| Card_Bg / Card_Highlight / Card_PictureBg.png | 후보‧공간 카드 배경, 선택 테두리, 사진 영역 | 9-slice |
| Gauge_Bg / Gauge_Fill.png | 공간 자동 선택 게이지 | 9-slice |
| RankRow_Bg.png | 순위 한 줄 배경 | 9-slice, **흰색**으로 (색은 코드가 칠함) |
| Dot.png | 공간 페이지 점 | **흰색**으로 (색은 코드가 칠함) |

9-slice 이미지는 모서리 크기가 바뀌면 Sprite Editor에서 Border 값을 맞춰 주세요.

## Unity 메뉴 (Tools > 전자파 토너먼트)
1. **프리팹 생성 및 씬 적용** — 없는 이미지/프리팹만 만들고 씬에 배치 (기존 것은 덮어쓰지 않음)
2. **가이드 PNG 다시 만들기** — 레이아웃을 바꾼 뒤 _Guide 이미지 갱신
3. **임시 스프라이트 다시 내보내기** — Static/Parts를 임시 디자인으로 모두 덮어씀 (주의)
4. **연결 검사** — 빠진 이미지, 비어 있는 인스펙터 연결 확인
