using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class GachaAPI : MonoBehaviour
{
    [SerializeField] private Renderer emissionTarget;
    [SerializeField] private Color emissionColor = Color.white;
    private Button drawButton;
    private VisualElement cardImage;
    private string sessionId;
    private const string BaseUrl = "https://lootbox2.gjmj.net";

    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        drawButton = uiDocument.rootVisualElement.Q<Button>("DrawButton");
        cardImage = uiDocument.rootVisualElement.Q<VisualElement>("CardImage");

        drawButton.clicked += OnDrawButtonClicked;

        StartCoroutine(GetUuid());
    }

    private IEnumerator GetUuid()
    {
        using UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/uuid");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        Debug.Log(request.downloadHandler.text);
        UuidResponse data =
    JsonUtility.FromJson<UuidResponse>(request.downloadHandler.text);

        string uuid = data.response.uuid;

        Debug.Log("UUID = " + uuid);

        StartCoroutine(Register(uuid));
    }

    [System.Serializable]
    public class UuidResponse
    {
        public UuidData response;
    }

    [System.Serializable]
    public class UuidData
    {
        public string uuid;
    }

    [System.Serializable]
    public class RegisterRequest
    {
        public string uuid;
        public string name;
    }

    [System.Serializable]
    public class SessionResponse
    {
        public SessionData response;
    }

    [System.Serializable]
    public class SessionData
    {
        public string session_id;
    }

    [System.Serializable]
    public class DrawResponse
    {
        public DrawData response;
    }

    [System.Serializable]
    public class DrawData
    {
        public int[] card_ids;
    }

    private IEnumerator Register(string uuid)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("name", "ebi");

        using UnityWebRequest request =
            UnityWebRequest.Post(BaseUrl + "/register", form);

        yield return request.SendWebRequest();

        Debug.Log("REGISTER: " + request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        StartCoroutine(GetSession(uuid));
    }

    private IEnumerator GetSession(string uuid)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);

        using UnityWebRequest request =
            UnityWebRequest.Post(BaseUrl + "/session/get", form);

        yield return request.SendWebRequest();

        Debug.Log("SESSION: " + request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        SessionResponse data =
            JsonUtility.FromJson<SessionResponse>(request.downloadHandler.text);

        sessionId = data.response.session_id;

        Debug.Log("SESSION ID = " + sessionId);
    }

    private IEnumerator DrawGacha(string sessionId)
    {
        using UnityWebRequest request =
            UnityWebRequest.PostWwwForm(BaseUrl + "/loot_box/draw/1", "");
        request.SetRequestHeader(
            "Authorization",
            "Bearer " + sessionId
        );

        yield return request.SendWebRequest();

        Debug.Log("DRAW: " + request.downloadHandler.text);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        DrawResponse data =
            JsonUtility.FromJson<DrawResponse>(request.downloadHandler.text);

        int cardId = data.response.card_ids[0];

        Debug.Log("CARD ID = " + cardId);

        StartCoroutine(GetCardImage(sessionId, cardId));
    }

    private IEnumerator GetCardImage(string sessionId, int cardId)
    {
        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(
                BaseUrl + "/card/image/" + cardId
            );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + sessionId
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        // texture作成
        Texture2D texture =
            DownloadHandlerTexture.GetContent(request);

        // なのでtextureを使う処理もここ
        cardImage.style.backgroundImage =
            new StyleBackground(texture);

        // 結果SE
        SoundManager.Instance.PlayResultSE();

        Debug.Log(
            $"画像取得成功！ {texture.width} x {texture.height}"
        );

        StartCoroutine(FlashEmission());
    }
    private void OnDrawButtonClicked()
    {
        SoundManager.Instance.PlayButtonSE();
        StartCoroutine(DrawGacha(sessionId));
    }

    private IEnumerator FlashEmission()
    {
        if (emissionTarget == null)
        {
            Debug.LogError("Emission Targetが未設定");
            yield break;
        }

        emissionTarget.gameObject.SetActive(true);

        Material mat = emissionTarget.material;

        Debug.Log("発光対象：" + emissionTarget.gameObject.name);

        mat.EnableKeyword("_EMISSION");

        // 見た目も確実に白くする
        mat.color = Color.white;

        // Emissionも同時にON
        mat.SetColor("_EmissionColor", Color.white * 5f);

        Debug.Log("発光ON");

        yield return new WaitForSeconds(1f);

        mat.SetColor("_EmissionColor", Color.black);
        mat.color = Color.black;

        Debug.Log("発光OFF");
    }
}