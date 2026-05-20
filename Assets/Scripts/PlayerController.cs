using Unity.Netcode;
using UnityEngine;

public enum PlayerEffectState { Normal, Advantage, Disadvantage }

public class PlayerController : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerEffectState> m_CurrentState = new();

    private PlayerInputActions m_Input;
    private Rigidbody m_Rb;
    private Renderer m_Renderer;

    private float m_MoveSpeed = 5f;
    private float m_AdvantageSpeed = 10f;
    private float m_DisadvantageSpeed = 2.5f;
    private float m_JumpForce = 6f;
    private float m_PanelLimit = 4.5f;


    public override void OnNetworkSpawn()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_Renderer = GetComponent<Renderer>();

        m_CurrentState.OnValueChanged += OnStateChanged;
        UpdateVisuals(m_CurrentState.Value); // Inicializar visuales

        if (IsServer)
        {
            transform.position = GetRandomPosition();
        }

        if (!IsOwner) { enabled = false; return; }

        m_Input = new PlayerInputActions();
        m_Input.Player.Enable();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) m_Input?.Dispose();
        m_CurrentState.OnValueChanged -= OnStateChanged;
    }

    private void Update()
    {
        Vector2 move = m_Input.Player.Move.ReadValue<Vector2>();
        bool jump = m_Input.Player.Jump.WasPressedThisFrame();
        MoveServerRpc(move, jump);
    }

    [Rpc(SendTo.Server)]
    private void MoveServerRpc(Vector2 move, bool jump)
    {
        // Determinar velocidad según el estado
        float speed;

        switch (m_CurrentState.Value)
        {
            case PlayerEffectState.Advantage:
                speed = m_AdvantageSpeed;
                break;
            case PlayerEffectState.Disadvantage:
                speed = m_DisadvantageSpeed;
                break;
            default:
                speed = m_MoveSpeed;
                break;
        }

        ApplyMovement(move, jump, speed);
        ClampPosition();
    }

    private void ApplyMovement(Vector2 move, bool jump, float speed)
    {
        Vector3 velocity = new Vector3(move.x, 0f, move.y) * speed;
        m_Rb.linearVelocity = new Vector3(velocity.x, m_Rb.linearVelocity.y, velocity.z);

        if (jump && Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            m_Rb.AddForce(Vector3.up * m_JumpForce, ForceMode.Impulse);
        }
    }

    private void ClampPosition()
    {
        transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, -m_PanelLimit, m_PanelLimit),
                    transform.position.y,
                    Mathf.Clamp(transform.position.z, -m_PanelLimit, m_PanelLimit)
                );
    }


    private static Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
    }

    // Cambiar estado desde el GameManager pasando el enum
    public void SetState(PlayerEffectState newState)
    {
        if (IsServer) m_CurrentState.Value = newState;
    }

    private void OnStateChanged(PlayerEffectState oldState, PlayerEffectState newState)
    {
        UpdateVisuals(newState);
    }


    private void UpdateVisuals(PlayerEffectState state)
    {
        if (m_Renderer == null) return;

        // Determinar color según el estado
        switch (state)
        {
            case PlayerEffectState.Advantage:
                m_Renderer.material.color = Color.green;
                break;

            case PlayerEffectState.Disadvantage:
                m_Renderer.material.color = Color.red;
                break;

            default:
                m_Renderer.material.color = Color.white; // Estado Normal por defecto
                break;
        }
    }
}