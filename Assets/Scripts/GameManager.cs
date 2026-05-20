using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    private float timeToEffect = 20f;
    private float effectDuration = 10f;

    public override void OnNetworkSpawn()
    {
        // Solo el servidor ejecuta la lógica del bucle de efectos
        if (IsServer) 
        {
            StartCoroutine(GlobalEffectLoop());
        }
    }

    private IEnumerator GlobalEffectLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToEffect);

            // Buscar todos los jugadores en la escena
            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (players.Length == 0) continue;

            // Elegir un jugador aleatorio
            PlayerController targetPlayer = players[Random.Range(0, players.Length)];

            // Decidir el estado (50% probabilidad)
            PlayerEffectState randomState = Random.value > 0.5f 
                ? PlayerEffectState.Advantage 
                : PlayerEffectState.Disadvantage;

            // Aplicar el estado en el servidor
            targetPlayer.SetState(randomState);
            
            // Iniciar la cuenta atrás para quitar el efecto a ese jugador específico
            StartCoroutine(ClearEffectAfterDelay(targetPlayer, effectDuration));
        }
    }

    private IEnumerator ClearEffectAfterDelay(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Validar que el jugador no se haya desconectado durante la espera
        if (player != null && player.IsSpawned) 
        {
            player.SetState(PlayerEffectState.Normal);
        }
    }
}