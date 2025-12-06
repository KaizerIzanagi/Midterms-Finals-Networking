using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("Main Image")]
    public Image cardImage;          // the Image on your card prefab

    [Header("Sprites")]
    public Sprite cardBackSprite;    // back of the card
    public Sprite[] cardRankSprites; // 13 sprites: A,2,3,...,K in order

    public string storedRank;

    private Sprite storedFrontSprite;
    private bool isFaceUp;

    // Map ranks to indices (0..12) to avoid depending on sprite names
    private static readonly System.Collections.Generic.Dictionary<string, int> rankIndex =
        new System.Collections.Generic.Dictionary<string, int>
        {
            { "A",  0 },
            { "2",  1 },
            { "3",  2 },
            { "4",  3 },
            { "5",  4 },
            { "6",  5 },
            { "7",  6 },
            { "8",  7 },
            { "9",  8 },
            { "10", 9 },
            { "J", 10 },
            { "Q", 11 },
            { "K", 12 },
        };

    void Awake()
    {
        if (!cardImage)
            cardImage = GetComponent<Image>();
    }

    // ----------------------------------------------------------------
    // Called by Deck on spawn
    // ----------------------------------------------------------------
    public void SetCard(string rank, bool faceUp)
    {
        storedRank = rank;
        storedFrontSprite = GetSpriteForRank(rank);
        isFaceUp = faceUp;

        cardImage.sprite = isFaceUp ? storedFrontSprite : cardBackSprite;
    }

    // Old one-parameter version still supported
    public void SetCard(string rank) => SetCard(rank, true);

    public void ShowFront(string rank)
    {
        storedRank = rank;
        storedFrontSprite = GetSpriteForRank(rank);
        isFaceUp = true;
        cardImage.sprite = storedFrontSprite;
    }

    public void ShowBack()
    {
        isFaceUp = false;
        cardImage.sprite = cardBackSprite;
    }

    // For dealer hidden card at reveal time
    public void Reveal()
    {
        if (isFaceUp)
            return;

        if (storedFrontSprite == null)
            storedFrontSprite = GetSpriteForRank(storedRank);

        isFaceUp = true;
        cardImage.sprite = storedFrontSprite;
    }

    // ----------------------------------------------------------------
    private Sprite GetSpriteForRank(string rank)
    {
        if (rankIndex.TryGetValue(rank, out int idx))
        {
            if (cardRankSprites != null && idx >= 0 && idx < cardRankSprites.Length)
                return cardRankSprites[idx];
        }

        Debug.LogWarning($"[CardUI] No sprite mapped for rank '{rank}'");
        return null;
    }
}
