using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class NetManageDebug : MonoBehaviour
{
    public List<localreference> localref;
    NetManage netManager;
    // Start is called before the first frame update
    void Start()
    {
        netManager = GetComponent<NetManage>();
    }

    // Update is called once per frame
    void Update()
    {
      //  localref = netManager.playerList.toli
            if(netManager != null)
            {
                if(netManager.playerList == null){return;}

//                Debug.Log(netManager.GetTotalNumberOfConnectedPlayers());
               //netManager.playerList[0] = new SessionPlayerData(10,"test", false, 2, NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>());
                
                 foreach(SessionPlayerData player in netManager.playerList)
                 {
                
                     check(player);
                
                 }
            }else
            {
                Debug.Log("netmanager is null");
            }
            

    }

    void check(SessionPlayerData player)
    {
        foreach (var VARIABLE in localref)
        {
            if(VARIABLE.ClientID == player.ClientID)
            {
                updateplayer(VARIABLE, player);
               return;
            }
            
        }
        
        Addplayer(player);
        
    }


    void updateplayer(localreference newplayer, SessionPlayerData player)
    {
        newplayer.IsConnected = player.IsConnected;
        newplayer.playerScore = player.playerScore;
        newplayer.ClientID = player.ClientID;
        newplayer.systemID = player.systemID;
    }
    
    void Addplayer(SessionPlayerData player)
    {
        var newplayer = new localreference();
        newplayer.IsConnected = player.IsConnected;
        newplayer.playerScore = player.playerScore;
        newplayer.ClientID = player.ClientID;
        newplayer.systemID = player.systemID;
        localref.Add(newplayer);
    }
    
    
}


[Serializable]
public class localreference
{
    [SerializeField]public bool IsConnected;
    [SerializeField] public int playerScore;

    [SerializeField]public ulong ClientID;

    [SerializeField]public FixedString128Bytes systemID;
        
    [SerializeField] public NetworkObjectReference playerObject;
}
