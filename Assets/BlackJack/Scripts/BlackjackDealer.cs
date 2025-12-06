using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackDealer : MonoBehaviour
{
    public static BlackjackDealer Instance;

    public int dealerTotal = 0;

    void Awake() => Instance = this;

    public void AddDealerCard(int cardValue)
    {
        dealerTotal += cardValue;
        Debug.Log($"Dealer total = {dealerTotal}");
    }
}
