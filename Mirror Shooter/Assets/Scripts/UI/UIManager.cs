using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI roundText;
    [SerializeField] TextMeshProUGUI announceText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        announceText.text = "";
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnScoreUpdated += UpdateScore;
            MatchManager.Instance.OnRoundUpdated += UpdateRound;
            MatchManager.Instance.OnAnnounceMessage += ShowAnnouncement;
            UpdateScore(0, 0);
            UpdateRound(1);
        }
    }

    void OnDestroy()
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnScoreUpdated -= UpdateScore;
            MatchManager.Instance.OnRoundUpdated -= UpdateRound;
            MatchManager.Instance.OnAnnounceMessage -= ShowAnnouncement;
        }
    }

    public void ConnectLocalPlayer(PlayerState localPlayer)
    {
        localPlayer.OnHealthUpdated += UpdateHealth;
        UpdateHealth(localPlayer.CurrentHealth);
    }

    void UpdateHealth(int currentHealth) => healthText.text = $"ЗДОРОВЬЕ: {currentHealth}";

    void UpdateScore(int redScore, int blueScore) => scoreText.text = $"КРАСНЫЕ: {redScore} | СИНИЕ: {blueScore}";

    void UpdateRound(int round) => roundText.text = $"РАУНД: {round}";

    void ShowAnnouncement(string message)
    {
        announceText.text = message;
        CancelInvoke(nameof(HideAnnouncement));
        Invoke(nameof(HideAnnouncement), 3f);
    }

    void HideAnnouncement() => announceText.text = "";
}