using UnityEngine;
using TMPro;

public class SimpleBlackJackScript : MonoBehaviour
{
    [SerializeField]
    [Header("Player 1 Values")]
    public float player1HandValue;
    public TextMeshProUGUI player1HV;
    public float player1HPValue;
    public TextMeshProUGUI player1HP;
    [SerializeField]
    [Header("Player 2 Values")]
    public float player2HandValue;
    public TextMeshProUGUI player2HV;
    public float player2HPValue;
    public TextMeshProUGUI player2HP;

    [SerializeField]
    [Header("Misc Values")]
    public float playerWinState;
    public float player1PointValue;
    public float player2PointValue;


    void Start()
    {
        player1HandValue = Random.Range(1, 21);
        player1HandValue = Random.Range(1, 21);

        player1HPValue = 100;
        player2HPValue = 100;
    }

    
    void Update()
    {
        switch (playerWinState)
        {
            case 1:
                Debug.Log("Player 1 Wins. Player 2 Loses (Blackjack)");
                break;
            case 2:
                Debug.Log("Player 1 Loses, Player 2 Wins (Blackjack)");
                break;
            case 3:
                Debug.Log("Player 1 Loses. Player 2 Wins. (Bust)");
                break;
            case 4:
                Debug.Log("Player 1 Wins. Player 2 Loses. (Bust)");
                break;
            case 5:
                Debug.Log("Player 1 Wins. Player 2 Loses. (Higher Hand Value)");
                break;
            case 6:
                Debug.Log("Player 1 Loses. Player 2 Wins. (Higher Hand Value)");
                break;
            case 7:
                Debug.Log("Player 1 and 2 Tie (Both Blackjack)");
                break;
            case 8:
                Debug.Log("Player 1 and 2 Tie (Both Bust)");
                break;
            case 9:
                Debug.Log("Player 1 and 2 Tie (Equal Hand Values");
                break;
            default:
                break;
        }

    }

    public void Hit()
    {
        player1HandValue += Random.Range(1, 21);
        player2HandValue += Random.Range(1, 21);

        if (player1HandValue == 21)
        {
            if (player2HandValue == 21)
            {
                playerWinState = 7;
            }
            else
            {
                playerWinState = 1;
            }
        }
        else if (player2HandValue == 21)
        {
            if (player1HandValue == 21)
            {
                playerWinState = 7;
            }
            else
            {
                playerWinState = 2;
            }
        }
        else if (player1HandValue > 21)
        {
            if (player2HandValue > 21)
            {
                playerWinState = 8;
            }
            else
            {
                playerWinState = 3;
            }
        }
        else if (player1HandValue > player2HandValue)
        {
            playerWinState = 5;
        }
        else if (player2HandValue > player1HandValue)
        {
            playerWinState = 6;
        }

    }

    public void Stand()
    {

    }

    public void Double()
    {

    }
}
