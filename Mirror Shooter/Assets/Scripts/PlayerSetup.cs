using Mirror;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] AudioListener audioListener;
    [SerializeField] Behaviour[] componentsToDisable; 

    void Start()
    {
        if (!isLocalPlayer)
        {
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
            foreach (var comp in componentsToDisable)
            {
                if (comp != null) comp.enabled = false;
            }
        }
        else
        {
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
            if (audioListener != null) audioListener.enabled = true;
        }
    }
}
