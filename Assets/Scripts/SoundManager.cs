using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [SerializeField] private AudioClip bgm;
    [SerializeField] private AudioClip buttonSE;
    [SerializeField] private AudioClip resultSE;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        bgmSource.clip = bgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayButtonSE()
    {
        seSource.PlayOneShot(buttonSE);
    }

    public void PlayResultSE()
    {
        seSource.PlayOneShot(resultSE);
    }
}