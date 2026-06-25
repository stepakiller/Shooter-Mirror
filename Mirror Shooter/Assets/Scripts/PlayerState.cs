using Mirror;
using System;
using UnityEngine;

public enum Team { None, Red, Blue }

public class PlayerState : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] Renderer playerRenderer;
    [SerializeField] Material redMaterial;
    [SerializeField] Material blueMaterial;
    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] int currentHealth;
    [SyncVar(hook = nameof(OnTeamChanged))]
    [SerializeField] Team team;
    [SerializeField] CharacterController characterController;
    public bool IsDead => currentHealth <= 0;
    public Team PlayerTeam => team;
    public int CurrentHealth => currentHealth;
    public event Action<int> OnHealthUpdated; 
    public event Action OnPlayerDied;

    [Server]
    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnPlayerDied?.Invoke();
        }
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        MatchManager.Instance.RegisterPlayer(this);
    }

    public override void OnStopServer()
    {
        if (MatchManager.Instance != null) MatchManager.Instance.UnregisterPlayer(this);
    }

    [Server]
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        currentHealth = maxHealth;
        RpcTeleport(position, rotation);
    }

    [ClientRpc]
    void RpcTeleport(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        characterController.enabled = true;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (UIManager.Instance != null) UIManager.Instance.ConnectLocalPlayer(this);
    }

    [Server]
    public void SetTeam(Team newTeam) => team = newTeam;
    void OnHealthChanged(int oldHealth, int newHealth) => OnHealthUpdated?.Invoke(newHealth);
    void OnTeamChanged(Team oldTeam, Team newTeam) => playerRenderer.material = (newTeam == Team.Red) ? redMaterial : blueMaterial;
}