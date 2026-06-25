using Mirror;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] int damage = 25;
    [SerializeField] float range = 100f;
    [SerializeField] LayerMask shootableLayers;
    [SerializeField] Transform gunBarrel;
    [SerializeField] PlayerState playerState;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] Transform playerCameraTransform;

    void Update()
    {
        if (!isLocalPlayer || playerState.IsDead) return;

        if (Input.GetButtonDown("Fire1")) CmdShoot(gunBarrel.position, playerCameraTransform.forward);
    }

    [Command]
    void CmdShoot(Vector3 origin, Vector3 direction)
    {
        Vector3 targetPoint = origin + (direction * range);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, shootableLayers))
        {
            targetPoint = hit.point;
            Debug.Log($"<color=orange>[Сервер]: Луч попал в объект: {hit.collider.gameObject.name}</color>"); 

            PlayerState target = hit.collider.GetComponentInParent<PlayerState>();
            
            if (target == null)
            {
                Debug.Log("<color=red>[Сервер]: На объекте нет PlayerState! Это стена или пол?</color>");
            }
            else if (target == playerState)
            {
                Debug.Log("<color=yellow>[Сервер]: Попадание в самого себя, урон отменен.</color>");
            }
            else if (target.PlayerTeam == playerState.PlayerTeam)
            {
                Debug.Log("<color=yellow>[Сервер]: Огонь по своим! Урон отменен.</color>");
            }
            else
            {
                Debug.Log($"<color=green>[Сервер]: Успешное попадание по врагу! Наносим {damage} урона.</color>");
                target.TakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("[Сервер]: Луч улетел в пустоту (ни во что не попал).");
        }
        RpcPlayShootEffects(gunBarrel.position, targetPoint);
    }

    [ClientRpc]
    void RpcPlayShootEffects(Vector3 startPoint, Vector3 endPoint)
    {
        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab, startPoint, Quaternion.identity);
            LineRenderer lr = tracer.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, startPoint);
                lr.SetPosition(1, endPoint);
            }
        }
    }
}