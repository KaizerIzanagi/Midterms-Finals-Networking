using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstanceController : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int Health { get; set; } = 50;

    [Networked] public bool isReady { get; set; }

    public Button readyButton;
    public GameObject readyPanel;

    public GameObject playerUIButtons, playerUIStats;

    public Button hitButton, stayButton;
    public Button increaseBetButton, decreaseBetButton, confirmBetButton;

    public Text betText, healthText;

    [Header("Result UI")]
    public Text resultText;

    public int betAmount = 0;
    public bool betLocked = false;

    public override void Spawned()
    {
        bool local = Object.HasInputAuthority;

        transform.SetParent(FindObjectOfType<Canvas>().transform, false);

        if (local)
        {
            playerUIButtons.SetActive(true);
            playerUIStats.SetActive(true);
            readyPanel.SetActive(true);
            betText.rectTransform.anchoredPosition = new Vector3(0f, -60f, 0f);
        }
        else
        {
            playerUIButtons.SetActive(false);
            playerUIStats.SetActive(false);
            readyPanel.SetActive(false);
            betText.rectTransform.anchoredPosition = new Vector3(0f, 60f, 0f);
            GetComponent<Image>().raycastTarget = false;
        }

        increaseBetButton.gameObject.SetActive(local);
        decreaseBetButton.gameObject.SetActive(local);
        confirmBetButton.gameObject.SetActive(local);

        hitButton.interactable = false;
        stayButton.interactable = false;

        UpdateHealthText();
        UpdateBetText();
    }

    public void OnReadyPressed()
    {
        if (!Object.HasInputAuthority) return;
        RPC_SetReady(Runner.LocalPlayer);
        readyButton.interactable = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(PlayerRef p)
    {
        isReady = true;
        BlackjackGameManager.Instance.PlayerReady(p);
    }

    public void OnHealthChanged(NetworkBehaviourBuffer prev)
    {
        Health = GetPropertyReader<int>(nameof(Health)).Read(prev);
        UpdateHealthText();
    }

    void UpdateHealthText() => healthText.text = $"HP: {Health}";
    void UpdateBetText() => betText.text = $"Bet: {betAmount}";

    private void Start()
    {
        hitButton.onClick.AddListener(() => Deck.Instance.RPC_RequestHit(Object.InputAuthority));
        stayButton.onClick.AddListener(() => OnStayPressed());

        increaseBetButton.onClick.AddListener(() => ChangeBet(5));
        decreaseBetButton.onClick.AddListener(() => ChangeBet(-5));
        confirmBetButton.onClick.AddListener(ConfirmBet);
    }

    void OnStayPressed()
    {
        if (!Object.HasInputAuthority || !betLocked) return;

        hitButton.interactable = false;
        stayButton.interactable = false;

        RPC_RequestStay(Object.InputAuthority);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStay(PlayerRef player)
    {
        BlackjackGameManager.Instance.PlayerStay(player);
    }

    void ChangeBet(int amount)
    {
        if (betLocked) return;
        betAmount = Mathf.Clamp(betAmount + amount, 0, Health);
        UpdateBetText();
    }

    void ConfirmBet()
    {
        betLocked = true;
        increaseBetButton.interactable = false;
        decreaseBetButton.interactable = false;
        confirmBetButton.interactable = false;

        hitButton.interactable = true;
        stayButton.interactable = true;
    }

    public void ResetBetting()
    {
        betLocked = false;
        betAmount = 0;

        increaseBetButton.interactable = true;
        decreaseBetButton.interactable = true;
        confirmBetButton.interactable = true;

        hitButton.interactable = false;
        stayButton.interactable = false;

        resultText.text = "";

        UpdateBetText();
    }
}
