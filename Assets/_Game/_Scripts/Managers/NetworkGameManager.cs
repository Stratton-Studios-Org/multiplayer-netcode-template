using Unity.Netcode;
using UnityEngine;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour 
{
  
    
    public override void OnNetworkSpawn() {
        //   if(!IsServer)
      //  SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        
      
    }   

    // [ServerRpc(RequireOwnership = false)]
    // private void SpawnPlayerServerRpc(ulong playerId) {
    //     var spawn = Instantiate( _networkManager.._networkManager._playerPrefab);
    //     spawn.NetworkObject.SpawnWithOwnership(playerId);
    // }
}
