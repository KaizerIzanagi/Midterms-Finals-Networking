using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class FusionManualBootstrapper : MonoBehaviour
{
    #region SINGLETON
    public static FusionManualBootstrapper Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("Fusion Components")]
    public NetworkRunner runner;
    public NetworkSceneManagerDefault sceneManager;

    [Header("Session Settings")]
    public string sessionName = "LocalServerSession";
    public string gameSceneName = "MainGame";

    [Header("Local Auth Manager")]
    public LocalAuthManager auth;

    async void StartServer()
    {
        if (runner == null)
        {
            return;
        }

        if (!auth.isUserAuthenticated())
        {
            Debug.LogError("User authentication invalid.");
            return;
        }

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "MainGame",
            SceneManager = sceneManager
        };

        var result = await runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log($"Fusion Server started on {runner.name} ({gameSceneName})");
        }
        else
        {
            Debug.LogError($"Failed to start Fusion Server: {result.ShutdownReason}");
        }
    }

    private async Task CreateSession()
    {
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            SceneManager = sceneManager,
        };

        var result = await runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log($"Created new session '{sessionName}' and loaded scene.");
        }
        else
        {
            Debug.LogError($"Failed to create session: {result.ShutdownReason}");
        }
    }

    private async Task JoinSession()
    {
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = sceneManager
        };

        var result = await runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log($" Joined existing session '{sessionName}'.");
        }
        else
        {
            Debug.LogError($" Failed to join session: {result.ShutdownReason}");
        }
    }

    public void OnLoadStartServer()
    {
        Debug.Log("Manual server start requested.");
        StartServer();
    }
}