// This script is a placeholder for a custom network manager that will handle multiplayer functionality using the Mirror networking library.
// CustomNetworkManager.cs

using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
public class CustomNetworkManager : NetworkManager
{
    [Header("Custom Spawns")]
    public GameObject hostPrefab;   // this wiil be the place to drag Player_Ball to in the Inspector
    public GameObject clientPrefab; // this wiil be the place to drag Enemy_Cube to in the Inspector

    // This is the built-in Mirror method that triggers the exact millisecond someone joins
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject prefabToSpawn;

        // "numPlayers" is a built-in Mirror variable that counts connected users.
        // If it is 0, the person currently connecting is the Host. 
        // Else if 1+ it is a Client. This is how we can spawn different prefabs for Host vs Client.

        if (numPlayers == 0)
        {
            prefabToSpawn = hostPrefab;
        }
        else
        {
            prefabToSpawn = clientPrefab;
        }

        // 1. Find a spawn point 
        Transform startPos = GetStartPosition();
        
        // 2. Instantiate the physical object in the Unity Scene
        GameObject playerInstance = startPos != null
            ? Instantiate(prefabToSpawn, startPos.position, startPos.rotation)
            : Instantiate(prefabToSpawn);

        // 3. Tell the server to bind this specific object to this specific user's keyboard
        NetworkServer.AddPlayerForConnection(conn, playerInstance);
    }
}

