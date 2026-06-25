using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    int nextTeamIndex = 0;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Team assignedTeam = (nextTeamIndex % 2 == 0) ? Team.Red : Team.Blue;
        nextTeamIndex++;
        Transform spawnPoint = null;
        if (MatchManager.Instance != null) spawnPoint = MatchManager.Instance.GetSpawnPoint(assignedTeam);
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
        PlayerState playerState = playerInstance.GetComponent<PlayerState>();
        if (playerState != null) playerState.SetTeam(assignedTeam);
        NetworkServer.AddPlayerForConnection(conn, playerInstance);
        Debug.Log($"[NetworkManager]: Игрок подключился и добавлен в команду {assignedTeam}");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("[NetworkManager]: Игрок отключился.");
        base.OnServerDisconnect(conn); 
    }
}