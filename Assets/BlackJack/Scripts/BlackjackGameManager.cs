using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class BlackjackGameManager : NetworkBehaviour
{
    public static BlackjackGameManager Instance;

    [Header("Rules")]
    public int blackjackTarget = 21;
    public int dealerLimit = 17;

    private Dictionary<PlayerRef, int> playerTotals = new();
    private Dictionary<PlayerRef, bool> playerReady = new();
    private Dictionary<PlayerRef, bool> playerStayed = new();
    private Dictionary<PlayerRef, bool> playerBusted = new();
    private Dictionary<PlayerRef, int> playerBets = new();

    private int dealerTotal = 0;

    public override void Spawned()
    {
        Instance = this;
        Debug.Log("<color=cyan>Blackjack Game Manager Active</color>");
    }

    // ============================================================
    // READY SYSTEM
    // ============================================================
    public void PlayerReady(PlayerRef player)
    {
        if (!Object.HasStateAuthority) return;

        playerReady[player] = true;

        Debug.Log($"Player {player} Ready ({playerReady.Count}/{Runner.ActivePlayers.Count()})");

        if (playerReady.Count == Runner.ActivePlayers.Count())
            RPC_StartGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_StartGame()
    {
        Debug.Log("<color=lime>All players ready — starting game</color>");

        foreach (var p in FindObjectsOfType<PlayerInstanceController>())
            p.readyPanel.SetActive(false);

        if (Object.HasStateAuthority)
            _ = DealOpeningCards();
    }

    // ============================================================
    // OPENING DEAL (HOST ONLY)
    // ============================================================
    public async Task DealOpeningCards()
    {
        Debug.Log("<color=cyan>Dealing Cards...</color>");

        var players = FindObjectsOfType<PlayerInstanceController>();

        // 2 cards each
        for (int i = 0; i < 2; i++)
        {
            foreach (var p in players)
            {
                Deck.Instance.RPC_RequestHit(p.Object.InputAuthority);
                RPC_DealingCardsStep(p.Object.InputAuthority.ToString());
                await Task.Delay(700);
            }
        }

        // Dealer cards
        Deck.Instance.RequestDealerCard(faceUp: true);
        await Task.Delay(600);

        Deck.Instance.RequestDealerCard(faceUp: false);
        await Task.Delay(600);

        RPC_EnablePlayerControls();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_DealingCardsStep(string player)
    {
        Debug.Log($"<color=white>Card dealt → {player}</color>");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_EnablePlayerControls()
    {
        foreach (var p in FindObjectsOfType<PlayerInstanceController>())
            if (p.betLocked)
            {
                p.hitButton.interactable = true;
                p.stayButton.interactable = true;
            }

        Debug.Log("<color=yellow>Players may Hit / Stay now</color>");
    }

    // ============================================================
    // CARD EVENTS
    // ============================================================
    public void PlayerHit(PlayerRef p, int value)
    {
        if (!playerTotals.ContainsKey(p))
            playerTotals[p] = 0;

        playerTotals[p] += value;
        Debug.Log($"Player {p} total = {playerTotals[p]}");

        if (playerTotals[p] > blackjackTarget)
            PlayerBust(p);
    }

    public void DealerHit(int v)
    {
        dealerTotal += v;
        Debug.Log($"Dealer total = {dealerTotal}");
    }

    // ============================================================
    public void PlayerStay(PlayerRef p)
    {
        if (!Object.HasStateAuthority) return;

        playerStayed[p] = true;
        Debug.Log($"Player {p} STAYED ({playerStayed.Count}/{Runner.ActivePlayers.Count()})");

        if (AllPlayersDone())
            StartDealerTurn();
    }


    void PlayerBust(PlayerRef p)
    {
        playerBusted[p] = true;

        Debug.Log($"<color=red>Player {p} BUST</color>");

        if (AllPlayersDone())
            StartDealerTurn();
    }

    bool AllPlayersDone()
    {
        return Runner.ActivePlayers.All(p =>
            (playerStayed.ContainsKey(p) && playerStayed[p]) ||
            (playerBusted.ContainsKey(p) && playerBusted[p]));
    }

    // ============================================================
    // DEALER TURN
    // ============================================================
    async void StartDealerTurn()
    {
        RPC_RevealDealerCards();
        await Task.Delay(800);

        while (dealerTotal < dealerLimit)
        {
            Deck.Instance.RequestDealerCard(true);
            await Task.Delay(900);
        }

        EvaluateResults();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RevealDealerCards()
    {
        Deck.Instance.RevealDealerCards();
    }

    // ============================================================
    // RESULTS BROADCAST TO ALL PLAYERS
    // ============================================================
    void EvaluateResults()
    {
        foreach (var kv in playerTotals)
        {
            PlayerRef pref = kv.Key;
            int score = kv.Value;

            var player = FindPlayer(pref);

            bool win = false;
            bool draw = false;

            if (score > 21) win = false;
            else if (dealerTotal > 21) win = true;
            else if (score > dealerTotal) win = true;
            else if (score == dealerTotal) draw = true;
            else win = false;

            string resultLabel = draw ? "Draw" : (win ? "Win" : "Lose");

            RPC_SetResultText(pref, resultLabel);

            if (draw == false) // draw = no HP change
            {
                if (win) player.Health = Mathf.Min(player.Health + player.betAmount, 50);
                else player.Health -= player.betAmount;
            }

            Debug.Log($"Player {pref}: {resultLabel} | HP:{player.Health}");
        }

        Invoke(nameof(ResetRoundHost), 2f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetResultText(PlayerRef player, string text)
    {
        var p = FindPlayer(player);
        p.resultText.text = text;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ApplyResultToPlayer(PlayerRef player, bool win, int bet)
    {
        var p = FindPlayer(player);

        if (win) p.Health = Mathf.Min(p.Health + bet, 50);
        else p.Health -= bet;

        Debug.Log($"Player {player} → {(win ? "WIN" : "LOSE")} | HP:{p.Health}");
    }

    // ============================================================
    void ResetRoundHost()
    {
        RPC_ResetRound();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ResetRound()
    {
        playerTotals.Clear();
        playerStayed.Clear();
        playerBusted.Clear();
        playerReady.Clear();
        dealerTotal = 0;

        Deck.Instance.ResetAndShuffleDeck();

        foreach (var p in FindObjectsOfType<PlayerInstanceController>())
            p.ResetBetting();

        Debug.Log("<color=cyan>=== NEW ROUND ===</color>");

        if (Object.HasStateAuthority)
            _ = DealOpeningCards();
    }

    PlayerInstanceController FindPlayer(PlayerRef id)
    {
        return FindObjectsOfType<PlayerInstanceController>()
            .First(p => p.Object.InputAuthority == id);
    }
}
