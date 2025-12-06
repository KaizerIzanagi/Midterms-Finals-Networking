using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStarter : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Prefabs")]
    public NetworkPrefabRef playerPrefab;
    public NetworkPrefabRef deckPrefab;   // <- assign your Deck prefab here
    public NetworkPrefabRef blackjackGMPrefab;   // <- assign your Deck prefab here

    private NetworkRunner runner;

    async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "BlackjackRoom",
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // Spawn deck only on host / state authority
        if (runner.IsServer || runner.IsSharedModeMasterClient)
        {
            var deckObj = runner.Spawn(deckPrefab, Vector3.zero, Quaternion.identity);
            var blackjackObj = runner.Spawn(blackjackGMPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("<color=cyan>Deck network prefab spawned</color>");
            Debug.Log("<color=cyan>Black Jack Game Manager network prefab spawned</color>");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // This spawns the PlayerInstance prefab for each player
        var playerObj = runner.Spawn(playerPrefab, inputAuthority: player);

        Debug.Log($"<color=cyan>Player Joined → {player} | Spawned PlayerPrefab</color>");
    }

    //---------------------- Required empty callbacks ----------------------//
    #region RUNNER_CALLBACKS
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }
    #endregion
}