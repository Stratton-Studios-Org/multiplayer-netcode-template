using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Netcode;


[Serializable]
public class PlayerScoreBoard
{
 //   public  ISessionPlayerData playerData;
    public string playerName = "";
    public GameObject playerObject;
    public PlayerScoreUI playerScoreUI;
    private string[] playernames = new string[] { "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7", "Player8", "Player9", "Player10" };

    public void Init()
    {
      // playerData.playerScore = 0;
        playerName = "** " + playernames[Random.Range(0, playernames.Length)];
        
    }
    
    public void Clear()
    {
        
        playerScoreUI?.gameObject.SetActive(false);
    }
    
    public void disconnect()
    {
        playerScoreUI.Inactive();
        
    }
 

    
}
