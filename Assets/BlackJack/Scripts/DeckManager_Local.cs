using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager_Local : MonoBehaviour
{
    public static DeckManager_Local Instance;

    [Header("UI References")]
    public RectTransform deckOrigin;
    public Transform handParent;       // the Horizontal Layout Group parent
    public Transform animationLayer;   // UI canvas top layer for flying cards

    public GameObject cardPrefab;          // CardUI Prefab
    public float cardFlySpeed = 8f;

    private List<CardData> deck = new List<CardData>();
    private readonly string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private void Awake() => Instance = this;

    private void Start()
    {
        GenerateDeck();
        ShuffleDeck();
    }

    //=========================================================

    void GenerateDeck()
    {
        deck.Clear();
        foreach (string rank in ranks)
        {
            int val = rank switch
            {
                "A" => 11,
                "J" or "Q" or "K" => 10,
                _ => int.Parse(rank)
            };
            deck.Add(new CardData { rank = rank, value = val });
        }
    }

    void ShuffleDeck()
    {
        System.Random rnd = new();
        for (int i = 0; i < deck.Count; i++)
        {
            int j = rnd.Next(deck.Count);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    //=========================================================
    // HIT (call from your UI button)
    // isMyCard -> true = show front, false = show back
    //=========================================================

    public void Hit(Transform playerHand, bool isMyCard)
    {
        if (deck.Count == 0) return;

        CardData card = deck[0];
        deck.RemoveAt(0);

        SpawnCardUI(card.rank, isMyCard, playerHand);
    }

    void SpawnCardUI(string rank, bool isMyCard, Transform targetHand)
    {
        // 1. Spawn in animation layer (NOT in the hand yet)
        GameObject go = Instantiate(cardPrefab, animationLayer);
        RectTransform rt = go.GetComponent<RectTransform>();

        rt.anchoredPosition = deckOrigin.anchoredPosition;
        rt.localScale = Vector3.one;

        CardUI ui = go.GetComponent<CardUI>();
        if (isMyCard) ui.ShowFront(rank);
        else ui.ShowBack();

        // 2. Animate to where the future slot *will be*
        StartCoroutine(FlyThenSnap(rt, handParent));
    }

    //=========================================================
    // UI Animation
    //=========================================================

    IEnumerator FlyThenSnap(RectTransform card, Transform finalParent)
    {
        Vector2 targetPos = finalParent.GetComponent<RectTransform>().anchoredPosition;

        while (Vector2.Distance(card.anchoredPosition, targetPos) > 20f)
        {
            card.anchoredPosition = Vector2.Lerp(card.anchoredPosition, targetPos, Time.deltaTime * 7f);
            yield return null;
        }

        // 3. After animation → snap into the layout group
        card.SetParent(finalParent);
        card.localScale = Vector3.one;
    }
}
