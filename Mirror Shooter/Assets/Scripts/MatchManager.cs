using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using System;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    [SerializeField] int roundsToWin = 12;
    [SerializeField] float timeBetweenRounds = 3f;
    [SerializeField] Transform[] redSpawnPoints;
    [SerializeField] Transform[] blueSpawnPoints;

    [SyncVar(hook = nameof(OnRedScoreChanged))]
    [SerializeField] int redScore = 0;

    [SyncVar(hook = nameof(OnBlueScoreChanged))]
    [SerializeField] int blueScore = 0;

    [SyncVar(hook = nameof(OnRoundChanged))]
    [SerializeField] int currentRound = 1;
    List<PlayerState> activePlayers = new List<PlayerState>();
    bool isRoundActive = false;
    public event Action<int, int> OnScoreUpdated;
    public event Action<int> OnRoundUpdated;
    public event Action<string> OnAnnounceMessage;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Server]
    public void RegisterPlayer(PlayerState player)
    {
        if (!activePlayers.Contains(player))
        {
            activePlayers.Add(player);
            player.OnPlayerDied += CheckRoundState; 
        }

        if (activePlayers.Count >= 2 && !isRoundActive && currentRound == 1)
        {
            StartRound();
        }
    }

    [Server]
    public void UnregisterPlayer(PlayerState player)
    {
        if (activePlayers.Contains(player))
        {
            player.OnPlayerDied -= CheckRoundState;
            activePlayers.Remove(player);
        }
    }

    [Server]
    void StartRound()
    {
        isRoundActive = true;
        RpcAnnounce("Раунд " + currentRound + " начался!");
    }

    [Server]
    void CheckRoundState()
    {
        if (!isRoundActive) return;
        int aliveRed = activePlayers.Count(p => p.PlayerTeam == Team.Red && !p.IsDead);
        int aliveBlue = activePlayers.Count(p => p.PlayerTeam == Team.Blue && !p.IsDead);

        if (aliveRed == 0 && aliveBlue > 0) RoundWon(Team.Blue);
        else if (aliveBlue == 0 && aliveRed > 0) RoundWon(Team.Red);
        else if (aliveRed == 0 && aliveBlue == 0)  RoundWon(Team.None);
    }

    [Server]
    void RoundWon(Team winningTeam)
    {
        isRoundActive = false;

        if (winningTeam == Team.Red) 
        {
            redScore++;
            RpcAnnounce("Красные выиграли раунд!");
        }
        else if (winningTeam == Team.Blue) 
        {
            blueScore++;
            RpcAnnounce("Синие выиграли раунд!");
        }
        else RpcAnnounce("Ничья!");

        if (redScore >= roundsToWin || blueScore >= roundsToWin)
        {
            RpcAnnounce(winningTeam.ToString() + " КОМАНДА ПОБЕДИЛА В МАТЧЕ!");
        }
        else StartCoroutine(RestartRoundRoutine());
    }

    [Server]
    IEnumerator RestartRoundRoutine()
    {
        yield return new WaitForSeconds(timeBetweenRounds);
        currentRound++;
        foreach (var player in activePlayers)
        {
            Transform spawnPoint = GetSpawnPoint(player.PlayerTeam);
            player.Respawn(spawnPoint.position, spawnPoint.rotation);
        }
        StartRound();
    }

    [Server]
    public Transform GetSpawnPoint(Team team)
    {
        if (team == Team.Red && redSpawnPoints.Length > 0) return redSpawnPoints[UnityEngine.Random.Range(0, redSpawnPoints.Length)];
        else if (team == Team.Blue && blueSpawnPoints.Length > 0) return blueSpawnPoints[UnityEngine.Random.Range(0, blueSpawnPoints.Length)];
        return transform;
    }


    [ClientRpc]
    void RpcAnnounce(string message)
    {
        OnAnnounceMessage?.Invoke(message);
        Debug.Log($"[MatchManager]: {message}");
    }

    void OnRedScoreChanged(int oldScore, int newScore) => OnScoreUpdated?.Invoke(newScore, blueScore);
    void OnBlueScoreChanged(int oldScore, int newScore) => OnScoreUpdated?.Invoke(redScore, newScore);
    void OnRoundChanged(int oldRound, int newRound) => OnRoundUpdated?.Invoke(newRound);
}