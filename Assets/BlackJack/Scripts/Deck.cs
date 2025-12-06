using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Deck : NetworkBehaviour
{
    public static Deck Instance;

    public RectTransform deckOrigin;
    public Transform animationLayer, localHand, remoteHand, dealerHand;

    public GameObject cardPrefab;
    public float flySpeed = 7f;

    List<CardData> deck = new();
    readonly string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
        { Generate(); Shuffle(); }

        deckOrigin = GameObject.Find("Deck").GetComponent<RectTransform>();
        animationLayer = GameObject.Find("AnimatingLayer").transform;
        localHand = GameObject.Find("P1PlayArea").transform;
        remoteHand = GameObject.Find("P2PlayArea").transform;
        dealerHand = GameObject.Find("DealerDeckArea").transform;
    }

    void Generate()
    {
        deck.Clear();
        foreach (var r in ranks)
            deck.Add(new CardData(r));
    }
    void Shuffle()
    {
        var r = new System.Random();
        for (int i = 0; i < deck.Count; i++)
        {
            int j = r.Next(deck.Count);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    // ==========================================================
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestHit(PlayerRef p) => Draw(p, true);
    public void RequestDealerCard(bool faceUp) => Draw(default, faceUp);

    void Draw(PlayerRef p, bool faceUp)
    {
        var c = deck[0]; deck.RemoveAt(0);
        RPC_GiveCard(p, c.rank, c.value, faceUp);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_GiveCard(PlayerRef p, string rank, int value, bool faceUp)
    {
        if (p != default) // PLAYER
        {
            bool mine = p == Runner.LocalPlayer;
            Spawn(rank, mine, mine ? localHand : remoteHand);
            BlackjackGameManager.Instance.PlayerHit(p, value);
        }
        else // DEALER
        {
            Spawn(rank, faceUp, dealerHand);
            BlackjackGameManager.Instance.DealerHit(value);
        }
    }

    void Spawn(string rank, bool front, Transform area)
    {
        var go = UnityEngine.Object.Instantiate(cardPrefab, animationLayer); // << FIXED
        var ui = go.GetComponent<CardUI>();

        ui.SetCard(rank, front);
        StartCoroutine(Fly(go.GetComponent<RectTransform>(), area));
    }


    System.Collections.IEnumerator Fly(RectTransform card, Transform t)
    {
        Vector2 dest = ((RectTransform)t).anchoredPosition;

        while (Vector2.Distance(card.anchoredPosition, dest) > 25f)
        {
            card.anchoredPosition = Vector2.Lerp(card.anchoredPosition, dest, Time.deltaTime * flySpeed);
            yield return null;
        }

        card.SetParent(t);
        card.anchoredPosition = Vector2.zero;
    }

    public void RevealDealerCards()
    {
        foreach (Transform c in dealerHand)
            c.GetComponent<CardUI>().Reveal();
    }

    public void ResetAndShuffleDeck()
    {
        Generate(); Shuffle();

        Clear(localHand); Clear(remoteHand); Clear(dealerHand);
        Clear(animationLayer);

        Debug.Log("<color=yellow>Deck Reset</color>");
    }

    void Clear(Transform t)
    {
        foreach (Transform c in t)
            Destroy(c.gameObject);
    }
}

public struct CardData
{
    public string rank; public int value;

    public CardData(string r)
    {
        rank = r;
        value = (r == "A" ? 11 :
                (r == "J" || r == "Q" || r == "K") ? 10 :
                 int.Parse(r));
    }
}
