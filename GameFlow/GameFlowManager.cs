using Mirror;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public static event Action OnGameStartEvent;
    public static event Action<string> OnLargeAreaCapturedUI;

    [Header("按顺序排列的所有大区域")]
    public LargeArea[] largeAreas;

    private readonly List<GetReady> players = new List<GetReady>();
    private Dictionary<NetworkConnectionToClient, Faction> connFaction = new();

    // 当前进行到第几个大区
    public int currentLargeIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        // 随机分派阵营…
        var rnd = new System.Random();
        var shuffled = players.OrderBy(_ => rnd.Next()).ToList();
        shuffled[0].faction = Faction.Allies;
        shuffled[1].faction = Faction.Axis;
        foreach (var p in players)
            connFaction[p.connectionToClient] = p.faction;

        RpcGameStart();

        // —— 新：订阅第一个大区 —— 
        if (largeAreas != null && largeAreas.Length > 0)
            SubscribeLargeArea(0);
    }

    [ClientRpc]
    void RpcGameStart()
    {
        OnGameStartEvent?.Invoke();
    }

    [Server]
    void SubscribeLargeArea(int idx)
    {
        largeAreas[idx].OnFullyCaptured += HandleLargeAreaCaptured;
    }

    [Server]
    void HandleLargeAreaCaptured(LargeArea la)
    {
        la.OnFullyCaptured -= HandleLargeAreaCaptured;

        bool isLast = (currentLargeIndex == largeAreas.Length - 1);
        RpcFireOnLargeAreaCaptured(la.name);

        if (!isLast)
        {
            currentLargeIndex++;
            SubscribeLargeArea(currentLargeIndex);
        }
        else
        {
            RpcGameOver(Faction.Allies);
        }
    }

    [ClientRpc]
    void RpcFireOnLargeAreaCaptured(string areaName)
    {
        OnLargeAreaCapturedUI?.Invoke(areaName);
    }

    [ClientRpc]
    void RpcGameOver(Faction winner)
    {
        Debug.Log($"Game Over! Winner = {winner}");
        // TODO: 结束 UI
    }

    [Server]
    public Faction GetFactionForConnection(NetworkConnectionToClient conn)
    {
        return connFaction.TryGetValue(conn, out var f) ? f : Faction.None;
    }
}
