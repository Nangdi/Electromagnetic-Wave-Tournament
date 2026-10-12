using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 후보 DB(StreamingAssets/candidates.json)를 불러오고 이미지를 Sprite로 읽어주는 매니저.
// 전시 현장에서는 JSON과 Images 폴더만 교체해 내용을 바꿀 수 있다.
public class CandidateDatabase : MonoBehaviour
{
    public static CandidateDatabase Instance { get; private set; }

    [Tooltip("StreamingAssets 기준 DB 파일 경로")]
    [SerializeField] private string dbFileName = "candidates.json";
    [Tooltip("StreamingAssets 기준 이미지 폴더")]
    [SerializeField] private string imageFolder = "Images";

    private CandidateDbJson db = new CandidateDbJson();
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public IReadOnlyList<SpaceData> Spaces => db.spaces;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, dbFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"후보 DB 파일이 없습니다: {path}");
            db = new CandidateDbJson();
            return;
        }

        db = JsonUtility.FromJson<CandidateDbJson>(File.ReadAllText(path)) ?? new CandidateDbJson();
        db.spaces ??= new List<SpaceData>();
        db.candidates ??= new List<CandidateData>();
        Debug.Log($"후보 DB 로드: 공간 {db.spaces.Count}개, 후보 {db.candidates.Count}개");
    }

    public SpaceData GetSpace(string spaceId)
    {
        return db.spaces.Find(s => s.id == spaceId);
    }

    public List<CandidateData> GetCandidates(string spaceId)
    {
        return db.candidates.FindAll(c => c.spaceId == spaceId);
    }

    // 이미지가 없거나 읽기 실패 시 null. 한 번 읽은 이미지는 캐시한다.
    public Sprite LoadSprite(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        if (spriteCache.TryGetValue(relativePath, out Sprite cached)) return cached;

        Sprite sprite = null;
        string path = Path.Combine(Application.streamingAssetsPath, imageFolder, relativePath);
        if (File.Exists(path))
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(File.ReadAllBytes(path)))
            {
                texture.wrapMode = TextureWrapMode.Clamp;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = relativePath;
            }
            else
            {
                Destroy(texture);
                Debug.LogWarning($"이미지 읽기 실패: {path}");
            }
        }

        spriteCache[relativePath] = sprite;
        return sprite;
    }
}
