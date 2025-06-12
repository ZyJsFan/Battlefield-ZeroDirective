using Mirror;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    /// <summary>游戏开始事件，客户端 UI 订阅显示子区域面板</summary>
    public static event Action OnGameStartEvent;
    /// <summary>大区完成事件，参数：大区名字</summary>
    public static event Action<string> OnLargeAreaCapturedUI;

    [Header("按顺序排列的所有大区域")]
    public LargeArea[] largeAreas;

    private readonly List<GetReady> players = new();
    private Dictionary<NetworkConnectionToClient, Faction> connFaction = new();
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

        Debug.Log("[Server] StartGame: 重置所有大区和子区状态");
           foreach (var la in largeAreas)
               {
            la.ResetState();
                   foreach (var reg in la.subRegions)
                reg.ResetState();
               }



        Debug.Log("[Server] StartGame: 分派阵营并启动");
        var rnd = new System.Random();
        var shuffled = players.OrderBy(_ => rnd.Next()).ToList();
        shuffled[0].faction = Faction.Allies;
        shuffled[1].faction = Faction.Axis;
        foreach (var p in players)
            connFaction[p.connectionToClient] = p.faction;

        RpcGameStart();
        if (largeAreas != null && largeAreas.Length > 0)
            SubscribeLargeArea(0);
    }

    [ClientRpc]
    void RpcGameStart()
    {
        Debug.Log("[Client] RpcGameStart received");
        OnGameStartEvent?.Invoke();

               if (LargeAreaUI.Instance != null)
                   {
            LargeAreaUI.Instance.subRegionPanel.SetActive(true);
            LargeAreaUI.Instance.BuildSubRegionRows(GameFlowManager.Instance.currentLargeIndex);
        }
    }

    [Server]
    void SubscribeLargeArea(int idx)
    {
        Debug.Log($"[Server] SubscribeLargeArea idx={idx}");
        largeAreas[idx].OnFullyCaptured += HandleLargeAreaCaptured;
    }

    [Server]
    void HandleLargeAreaCaptured(LargeArea la)
    {
        Debug.Log($"[Server] LargeArea {la.name} fully captured at idx={currentLargeIndex}");
        la.OnFullyCaptured -= HandleLargeAreaCaptured;
        RpcFireOnLargeAreaCaptured(la.name);

        bool isLast = (currentLargeIndex == largeAreas.Length - 1);
        if (!isLast)
        {
            currentLargeIndex++;
            SubscribeLargeArea(currentLargeIndex);
        }
        else RpcGameOver(Faction.Allies);
    }

    [ClientRpc]
    void RpcFireOnLargeAreaCaptured(string areaName)
    {
        Debug.Log($"[Client] RpcFireOnLargeAreaCaptured received: {areaName}");
        OnLargeAreaCapturedUI?.Invoke(areaName);

        if (LargeAreaUI.Instance != null)
                   {
                       // 重新把下一区的 rows 给建起来
            LargeAreaUI.Instance.subRegionPanel.SetActive(true);
            LargeAreaUI.Instance.BuildSubRegionRows(GameFlowManager.Instance.currentLargeIndex);
        }
    }

    [ClientRpc]
    void RpcGameOver(Faction winner)
    {
        Debug.Log($"[Client] RpcGameOver Winner = {winner}");
    }

    [Server]
    public Faction GetFactionForConnection(NetworkConnectionToClient conn)
        => connFaction.TryGetValue(conn, out var f) ? f : Faction.None;
}
