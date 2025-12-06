using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlackjackUI : NetworkBehaviour
{
    public static BlackjackUI Instance;

    public GameObject resultPanelLocal;
    public TMP_Text resultTextLocal;

    public GameObject resultPanelRemote;
    public TMP_Text resultTextRemote;

    void Awake() => Instance = this;

    public void ShowResult(PlayerRef player, string result)
    {
        bool isMe = player == Runner.LocalPlayer;

        if (isMe)
        {
            resultPanelLocal.SetActive(true);
            resultTextLocal.text = result;
        }
        else
        {
            resultPanelRemote.SetActive(true);
            resultTextRemote.text = result;
        }
    }
}
