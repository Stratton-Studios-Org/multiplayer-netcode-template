using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
public class NetManage : NetworkBehaviour
{
    NetworkManager networkManager;
    private ulong playerIDs;
    
    
   [SerializeField] public NetworkList<SessionPlayerData> playerList = new NetworkList<SessionPlayerData>();
    
    public  EventHandler OnAllPlayersConnected;

    public override void OnNetworkSpawn() {
        
        base.OnNetworkSpawn();
        Debug.Log("Spawned");
        networkManager = NetworkManager.Singleton;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        Debug.Log(networkManager.IsServer);
        
        if (networkManager.IsHost) {
            playerIDs = 0;
            AddPlayerData(networkManager.LocalClientId);
            var sessionPlayerData = playerList[0];
            sessionPlayerData.systemID = networkManager.LocalClient.PlayerObject.GetComponent<SystemGuid>().GetGuid();
            StartCoroutine(SetupPlayerData(networkManager.LocalClientId, SetupPlayerDataCallback));

        }

        if (IsServer)
        {
            GameManager g = FindObjectOfType<GameManager>();
            GamePlayEventManager.GameCleanUpInvoke();
        }
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        playerList.Clear();
        Debug.Log("Despawned");
    }
    void OnClientConnected(ulong clientId) {
         if(!IsServer){return;}
         Debug.Log("Client connected" + clientId);



         var oldplayerid = GetPlayerDataBasedOnSystemID(networkManager.LocalClient.PlayerObject.GetComponent<SystemGuid>().GetGuid());

        if (oldplayerid.systemID != "null")
        {
            oldplayerid.IsConnected = true;
            return;
        }else
        {
            playerIDs++;
            AddPlayerData(clientId);
            //  SpawnPlayerServerRpc(clientId);
        }


        StartCoroutine(SetupPlayerData(clientId, SetupPlayerDataCallback));

     }

     void OnClientDisconnected(ulong clientId) {
         if(!IsServer){return;}

         Debug.Log("Client disconnected");
            
         for (int i = 0; i < playerList.Count; i++)
         {
             if (playerList[i].ClientID == clientId)
             {
                 playerList[i]= new SessionPlayerData(playerList[i].playerScore, playerList[i].systemID,false, playerList[i].ClientID, playerList[i].playerObject);
             }
         }
         networkManager.ConnectedClients.TryGetValue(clientId, out var networkedClient);
         networkedClient.PlayerObject.GetComponent<NetworkObject>().Despawn(true);
        

     }
     
     public void SetupPlayerDataCallback(bool isDone){
         if(isDone)
            {
                CheckAllPlayers();
            }
     }

     IEnumerator SetupPlayerData(ulong clientId, Action<bool> callback)
     {
         yield return new WaitForSeconds(0.1f);
         GameManager g = FindObjectOfType<GameManager>();
         networkManager.ConnectedClients.TryGetValue(clientId, out var networkedClient);
        FixedString128Bytes guid = networkedClient.PlayerObject.GetComponent<SystemGuid>().GetGuid();
        var v = GetPlayerDataBasedOnClientID(clientId);
        if(v.systemID != "null")
        {
            v.systemID = guid;
        }
        callback(true);
     }

     void CheckAllPlayers()
     {
         if (!IsServer)
         {
             return;
         }
         var playersinlobby = networkManager.ConnectedClientsList.Count;
         if(playersinlobby <= 1)
         {
             Debug.Log("No players in lobby");
             return;
         }
         else
         {
             Debug.Log("Players in lobby: " + playersinlobby);
         }
        
         foreach (var VARIABLE in playerList)
         {
             if(VARIABLE.IsConnected == false)
             {
                 Debug.Log("Not all are created players created: ");

                 break;
             }
         }
         OnAllPlayersConnected?.Invoke(this, EventArgs.Empty);
     }


     #region Helper Methods
     
     private void AddPlayerData(ulong clientId)
     {
          networkManager.ConnectedClients.TryGetValue(clientId, out var networkedClient);
         var v =  networkedClient.PlayerObject.GetComponent<NetworkObject>();

         playerList.Add(new SessionPlayerData() { IsConnected = true,  ClientID = clientId, playerObject = v});
         
         Debug.Log("Added player data" + clientId + playerList.Count);
     }
     
     
     
     private void RemovePlayerData(ulong clientId)
     {
       //  SessionPlayerData playerData = playerList.Find(x => x.ClientID == clientId);
            
         var v = GetPlayerDataBasedOnClientID(clientId);

         SessionPlayerData playerData = v;

         playerList.Remove(playerData);


     }

     public void ClearPlayerData()
     {
       //  playerList.Clear();
     }
     
        
        ///
        /// new josh
        ///
        public SessionPlayerData GetPlayerDataBasedOnSystemID(FixedString128Bytes guid)
        {
            foreach (var VARIABLE in playerList)
            {
                var sessionPlayerData = VARIABLE;
                if(sessionPlayerData.systemID == guid)
                {
                    return sessionPlayerData;
                }
            }

            return new SessionPlayerData{systemID = "null"};
        }
        
        public SessionPlayerData GetPlayerDataBasedOnClientID(ulong Clientid)
        {
            foreach (var VARIABLE in playerList)
            {
                var sessionPlayerData = VARIABLE;
                if(sessionPlayerData.ClientID == Clientid)
                {
                    return sessionPlayerData;
                }
            }

            return new SessionPlayerData{systemID = "null"};
        }
        
        public bool CheckIfPlayerIDMatches(ulong clientid)
        {

            foreach (var VARIABLE in playerList)    
            {
                var sessionPlayerData = VARIABLE;
                if(sessionPlayerData.ClientID == clientid)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public bool CheckIfPlayerIsStillConnected(ulong clientid)
        {

            foreach (var VARIABLE in playerList)    
            {
                var sessionPlayerData = VARIABLE;
                if(sessionPlayerData.ClientID == clientid)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        
        
        public int GetTotalNumberOfPlayers()
        {
            if(playerList != null)
            {
                return playerList.Count;
            }
            else
            {
                return 0;
            }
         
        }
        public int GetTotalNumberOfConnectedPlayers()
        {
            if(playerList != null)
            {
                var v = playerList;
                int count = 0;  
                for (int i = 0; i < playerList.Count; i++)
                {
                    if(playerList[i].IsConnected)
                    {
                        count++;
                    }
                }

                return count;
            }
            else
            {
                return 0;
            }
         
        }
        

        public List<SessionPlayerData> GetAllPlayerData()
        {

            var v = new List<SessionPlayerData>();
            
            foreach (var VARIABLE in playerList)
            {
                var sessionPlayerData = VARIABLE;
                v.Add(sessionPlayerData);
            }
            Debug.Log("Get all player data: " + v.Count);

            return v;

        }
        
        public List<SessionPlayerData> GetAllPlayerDataConnected()
        {

            var v = new List<SessionPlayerData>();
            
            foreach (var VARIABLE in playerList)
            {
                
                var sessionPlayerData = VARIABLE;
                if (VARIABLE.IsConnected)
                {
                    v.Add(sessionPlayerData);
                }
            }

            return v;

        }



        #endregion
     
     
     
     
}
