using Mirror;
using UnityEngine;

namespace Scenes.Multiplayer.MultiplayerScripts
{
    public class PlayerHealth : NetworkBehaviour
    {
        // [ServerCallback] is a Mirror security feature.
        // It guarantees this collision logic ONLY runs on the Server. 
        // Clients will physically collide, but their computers will ignore this code.
        [ServerCallback]
        void OnCollisionEnter(Collision collision)
        {
            // Check if the thing the player hit was "Enemy" tagged
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // The Server confirms the hit
                // and informs the player that he lost
                TargetLoseGame(connectionToClient);
            }
        }
        
        // [TargetRpc] tells Mirror: "Server, send this function over the internet, 
        // but ONLY execute it on the screen of the specific client we pass in."
        [TargetRpc]
        public void TargetLoseGame(NetworkConnectionToClient target)
        {
            Debug.LogWarning("Fetal HIT: " + target + "killed you");
            
            // Disable the movement scripts so the player can not move
            GetComponent<NetworkPlayerController>().enabled = false;
            
            // this can be used to show "Game over" later if asked by Aaron
        }
    }
}