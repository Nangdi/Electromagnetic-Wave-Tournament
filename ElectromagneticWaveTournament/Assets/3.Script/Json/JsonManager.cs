using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 설정창(SettingsPanelUI)에 노출되는 런타임 설정.
// public bool 필드를 추가하면 설정창에서 토글로 다룰 수 있다.
[Serializable]
public class GameSettingData
{
    // Unity 빌드 창을 항상 맨 위로 띄울지 여부.
    public bool useUnityOnTop;
    // 마우스 커서 표시 여부. false면 커서를 숨긴다(키오스크/전시용).
    public bool showMouseCursor;
}

// 프로젝트별로 실행 중에 바뀌는 데이터를 담는 곳 (Setting.json).
[Serializable]
public class GameDynamicData
{
    // 선택 스위치 컨트롤러가 RS232로 보내는 문자열 (앞뒤 공백/줄바꿈 제거 후 대소문자 무시 비교)
    public string leftSwitchCommand = "L";
    public string rightSwitchCommand = "R";
    // 같은 스위치의 연속 입력을 무시하는 시간(초). 채터링/연타 방지.
    public float inputDebounceSeconds = 0.2f;

    // 공간 선택 화면에서 입력이 없을 때 가운데 공간을 자동 선택하기까지의 시간(초)
    public float mapAutoSelectSeconds = 10f;
    // 토너먼트 경기에서 선택 후 다음 경기로 넘어가기 전 강조 효과 시간(초)
    public float selectEffectSeconds = 0.4f;
    // 최종 결과 화면의 순차 공개 간격(초)
    public float resultRevealInterval = 0.8f;
    // 대기 화면이 아닐 때 이 시간(초) 동안 입력이 없으면 대기 화면으로 복귀. 0 이하이면 사용 안 함.
    public float idleReturnSeconds = 90f;
    // 토너먼트 참가 후보 수 (2의 거듭제곱). 기본 16강.
    public int bracketSize = 16;
}

[Serializable]
public class PortConfig
{
    public int controllerId;
    public string com;
    public int baudLate;
    // RS232 사용 여부. false면 해당 컨트롤러는 포트를 열지 않는다.
    public bool enabled = true;
}

[Serializable]
public class PortJson
{
    // 컨트롤러별 전용 포트 목록. 필요한 만큼 port.json 에서 늘려 쓴다.
    public List<PortConfig> ports = new List<PortConfig>
    {
        new PortConfig { controllerId = 1, com = "COM1", baudLate = 19200, enabled = false },
    };
}

[Serializable]
public class TcpJson
{
    // 메인 서버 접속 주소와 포트
    public string host = "127.0.0.1";
    public int port = 5000;
    // 끊겼을 때 재시도 간격(초). 0 이하이면 재연결 시도하지 않음.
    public float reconnectIntervalSeconds = 3f;
    // 송신 메시지 끝에 줄바꿈을 자동으로 붙일지 여부
    public bool appendOutgoingLineEnding = true;
}

public class JsonManager : MonoBehaviour
{
    public static JsonManager instance;
    public GameSettingData gameSettingData = new GameSettingData();
    public PortJson portJson = new PortJson();
    public TcpJson tcpJson = new TcpJson();
    public GameDynamicData gameDynamicData = new GameDynamicData();

    private string gameDataPath;
    private string gameDynamicDataPath;
    private string portPath;
    private string tcpPath;

    public string GameDataPath => gameDataPath;

    // 싱글톤을 초기화하고 JSON 파일에서 런타임 설정 데이터를 불러옵니다.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        portPath = Path.Combine(Application.streamingAssetsPath, "port.json");
        tcpPath = Path.Combine(Application.streamingAssetsPath, "tcp.json");
        gameDynamicDataPath = Path.Combine(Application.streamingAssetsPath, "Setting.json");
        gameDataPath = Path.Combine(Application.persistentDataPath, "gameSettingData.json");

        gameSettingData ??= new GameSettingData();
        gameDynamicData ??= new GameDynamicData();
        portJson ??= new PortJson();
        tcpJson ??= new TcpJson();

        gameSettingData = LoadData(gameDataPath, gameSettingData);
        gameDynamicData = LoadData(gameDynamicDataPath, gameDynamicData);
        portJson = LoadData(portPath, portJson);
        tcpJson = LoadData(tcpPath, tcpJson);
    }

    // 현재 게임 설정 데이터를 gameSettingData.json 파일에 저장합니다.
    public void SaveGameSettingData()
    {
        SaveData(gameSettingData, gameDataPath);
    }

    // 지정한 경로에 JSON 파일을 생성하거나 덮어씁니다.
    public static void SaveData<T>(T jsonObject, string path) where T : new()
    {
        if (jsonObject == null)
            jsonObject = new T();

        string directoryPath = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string json = JsonUtility.ToJson(jsonObject, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved JSON: {path}");
    }

    // JSON 파일을 읽고, 파일이 없으면 기본값으로 새 파일을 만듭니다.
    // FromJsonOverwrite를 사용해 json에 없는 필드는 인스턴스 기본값을 유지한다.
    // (예: 기존 port.json에 enabled 필드가 없어도 PortConfig.enabled = true 가 보존됨)
    public static T LoadData<T>(string path, T data) where T : new()
    {
        if (data == null) data = new T();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"JSON file does not exist. Creating a new file: {path}");
            SaveData(data, path);
            return data;
        }

        Debug.Log($"Loaded JSON: {path}");
        string json = File.ReadAllText(path);
        JsonUtility.FromJsonOverwrite(json, data);
        return data;
    }
}
