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

    [Header("������������")]
    public AreaRegion[] contestedRegions;
    public int alliesWinCount = 2;
    public int axisWinCount = 2;

    // �� ��������¼ÿ���ͻ��������ĸ���Ӫ
    private Dictionary<NetworkConnectionToClient, Faction> connFaction =
        new Dictionary<NetworkConnectionToClient, Faction>();

    int alliesCaptured = 0, axisCaptured = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // �� RTSNetworkManager ����
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

        foreach (var player in players) // players: List<GetReady>
        {
            var conn = player.connectionToClient;
            connFaction[conn] = player.faction;
        }

        // ע�������¼�
        foreach (var r in contestedRegions)
        {
            r.OnCaptured += HandleCaptured;
            r.OnLost += HandleLost;
        }
    }

    [ClientRpc]
    void RpcGameStart()
    {
        Debug.Log("[GameFlowManager] Game Started!");
        OnGameStartEvent?.Invoke();
    }

    // �����Ҫ��GameFlowManager Ҳ������ OnStartServer ע������ RTSNetworkManager
    public void InitializeOnServer() { /* optional */ }




    [Server]
    public Faction GetFactionForConnection(NetworkConnectionToClient conn)
    {
        return connFaction.TryGetValue(conn, out var f) ? f : Faction.None;
    }

    [Server]
    void HandleCaptured(AreaRegion region, Faction byFaction)
    {
        if (byFaction == Faction.Allies) alliesCaptured++;
        else axisCaptured++;
        CheckVictory();
    }
    [Server]
    void HandleLost(AreaRegion region, Faction byFaction)
    {
        if (byFaction == Faction.Allies) alliesCaptured--;
        else axisCaptured--;
        CheckVictory();
    }
    [Server]
    void CheckVictory()
    {
        if (alliesCaptured >= alliesWinCount) RpcGameOver(Faction.Allies);
        else if (axisCaptured >= axisWinCount) RpcGameOver(Faction.Axis);
    }

    [ClientRpc]
    void RpcGameOver(Faction winner)
    {
        Debug.Log($"Game Over! Winner = {winner}");
        // TODO: ��ʾʤ��/ʧ�� UI
    }

}
