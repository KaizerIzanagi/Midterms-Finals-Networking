using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LocalAPI : MonoBehaviour
{
    public string baseURL = "http://localhost:3000";

    [System.Serializable]
    public class PlayerData
    {
        public string PlayerID;
        public string PlayerName;
        public string Email;
        public int Level;
    }

    [Header("UI ELEMENTS")]
    public TMP_InputField playerID_IF;
    public TMP_InputField playerName_IF;
    public TMP_InputField playerEmail_IF;
    public TMP_InputField playerLevel_IF;

    // -------------------------
    // Register Player (POST)
    // -------------------------
    public IEnumerator RegisterPlayer(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        string url = baseURL + "/register";

        yield return SendPostRequest(url, json, (response) =>
        {
            Debug.Log("Register Response: " + response);
        });
    }

    // -------------------------
    // Login Player (POST)
    // -------------------------
    public IEnumerator LoginPlayer(string playerID)
    {
        string url = baseURL + "/login";
        string json = "{\"PlayerID\":\"" + playerID + "\"}";

        yield return SendPostRequest(url, json, (response) =>
        {
            Debug.Log("Login Response: " + response);
        });
    }

    // -------------------------
    // GET Player
    // -------------------------
    public IEnumerator GetPlayer(string playerID)
    {
        string url = baseURL + "/player/" + playerID;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get Error: " + www.error);
            }
            else
            {
                Debug.Log("Player Data: " + www.downloadHandler.text);

                PlayerData data = JsonUtility.FromJson<PlayerData>(www.downloadHandler.text);

                // Save locally
                PlayerPrefs.SetString("PlayerID", data.PlayerID);
                PlayerPrefs.SetString("PlayerName", data.PlayerName);
                PlayerPrefs.SetString("Email", data.Email);
                PlayerPrefs.SetInt("Level", data.Level);
                PlayerPrefs.Save();

                // Update UI
                playerID_IF.text = data.PlayerID;
                playerName_IF.text = data.PlayerName;
                playerEmail_IF.text = data.Email;
                playerLevel_IF.text = data.Level.ToString();
            }
        }
    }

    // -------------------------
    // UPDATE Player (PUT)
    // -------------------------
    public IEnumerator UpdatePlayer(PlayerData data)
    {
        string url = baseURL + "/player/" + data.PlayerID;
        string json = JsonUtility.ToJson(data);

        UnityWebRequest www = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Put Error: " + www.error);
        }
        else
        {
            Debug.Log("Update Response: " + www.downloadHandler.text);
        }
    }

    // -------------------------
    // DELETE Player
    // -------------------------
    public IEnumerator DeletePlayer(string playerID)
    {
        string url = baseURL + "/player/" + playerID;

        UnityWebRequest www = UnityWebRequest.Delete(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Delete Error: " + www.error);
        }
        else
        {
            Debug.Log("Delete Response: " + www.downloadHandler.text);

            // Clear UI
            playerID_IF.text = "";
            playerName_IF.text = "";
            playerEmail_IF.text = "";
            playerLevel_IF.text = "";
        }
    }

    // -------------------------
    // Helper POST request
    // -------------------------
    private IEnumerator SendPostRequest(string url, string json, Action<string> callback)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Post Error: " + www.error);
        }
        else
        {
            callback?.Invoke(www.downloadHandler.text);
        }
    }

    // -------------------------
    // BUTTONS
    // -------------------------
    public void OnClick_LoginUser()
    {
        StartCoroutine(GetPlayer(playerID_IF.text));
    }

    public void OnClick_RegisterUser()
    {
        StartCoroutine(RegisterPlayer(new PlayerData
        {
            PlayerID = playerID_IF.text,
            PlayerName = playerName_IF.text,
            Email = playerEmail_IF.text,
            Level = int.Parse(playerLevel_IF.text)
        }));
    }

    public void OnClick_UpdatePlayer()
    {
        StartCoroutine(UpdatePlayer(new PlayerData
        {
            PlayerID = playerID_IF.text,
            PlayerName = playerName_IF.text,
            Email = playerEmail_IF.text,
            Level = int.Parse(playerLevel_IF.text)
        }));
    }

    public void OnClick_DeletePlayer()
    {
        StartCoroutine(DeletePlayer(playerID_IF.text));
    }
}