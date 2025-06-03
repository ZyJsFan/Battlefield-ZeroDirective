using Mirror;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private readonly List<GetReady> players = new List<GetReady>();

    public static event Action OnGameStartEvent;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 由 RTSNetworkManager 调用
    [Server]
    public void RegisterPlayer(GetReady p)
    {
        players.Add(p);
        p.OnReadyChanged += (_, __) => TryStartGame();
    }

    [Server]
    void TryStartGame()
    {
        if (players.Count == 2 && players.All(pl => pl.isReady))
            StartGame();
    }

    [Server]
    void StartGame()
    {
        var rnd = new System.Random();
        var shuffled = players.OrderBy(_ => rnd.Next()).ToList();
        shuffled[0].faction = Faction.Allies;
        shuffled[1].faction = Faction.Axis;
        RpcGameStart();
    }

    [ClientRpc]
    void RpcGameStart()
    {
        Debug.Log("[GameFlowManager] Game Started!");
        OnGameStartEvent?.Invoke();
    }

    // 如果需要，GameFlowManager 也可以在 OnStartServer 注册自身到 RTSNetworkManager
    public void InitializeOnServer() { /* optional */ }
}
