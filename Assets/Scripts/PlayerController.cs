using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private float moveSpeed = 5f;
    private float advantageSpeed = 10f;
    private float disavantageSpeed = 2f;
    private float jumpForce = 6f;
    private float timeToAdvantageOrDisavantage = 20f;
    private float effectDuration = 10f;
    private float panelLimit = 4.5f;

    private PlayerInputActions m_Input;
    private Rigidbody m_Rb;



    private bool IsGrounded() =>
        Physics.Raycast(transform.position, Vector3.down, 1.1f);

    public override void OnNetworkSpawn()
    {
        m_Rb = GetComponent<Rigidbody>();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        if (IsServer)
        {
            transform.position = GetRandomPosition();
        }

        m_Input = new PlayerInputActions();
        m_Input.Player.Enable();
    }

    void Update()
    {
        Vector2 moveInput = m_Input.Player.Move.ReadValue<Vector2>();
        bool jump = m_Input.Player.Jump.WasPressedThisFrame();

        SendInputServerRpc(moveInput, jump);
    }

    private static Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
    }

    private void ApplyMovement(Vector2 moveInput, bool jump)
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        m_Rb.linearVelocity = new Vector3(move.x, m_Rb.linearVelocity.y, move.z);

        if (jump && IsGrounded())
        {
            m_Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ClampPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -panelLimit, panelLimit);
        position.z = Mathf.Clamp(position.z, -panelLimit, panelLimit);
        transform.position = position;
    }

    [Rpc(SendTo.Server)]
    private void SendInputServerRpc(Vector2 moveInput, bool jump)
    {
        ApplyMovement(moveInput, jump);
        ClampPosition();
    }
}
