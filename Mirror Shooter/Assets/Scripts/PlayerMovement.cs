using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 3f; 
    [SerializeField] float maxLookAngle = 85f;
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform cameraTransform;
    [SerializeField] PlayerState playerState; 

    float cameraPitch = 0f;

    void Start()
    {
        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!isLocalPlayer || playerState.IsDead) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = (transform.forward * vertical) + (transform.right * horizontal);
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * mouseX * rotationSpeed);
        float mouseY = Input.GetAxis("Mouse Y");
        cameraPitch -= mouseY * rotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }
}