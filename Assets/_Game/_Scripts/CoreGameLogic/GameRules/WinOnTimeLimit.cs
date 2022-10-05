using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WinOnTimeLimit : IGameRules
{

    public  WinOnTimeLimit(GameManager gameManager, NetManage netManager)
    {
        this._gameManager = gameManager;
        this._netManager = netManager;
    }
   
   

    public void ConditionToComplete()
    {
        throw new System.NotImplementedException();
    }

    public void CheckCondition()
    {
        throw new System.NotImplementedException();
    }

    public bool Success { get; set; }
    public bool Fail { get; set; }
    public NetManage _netManager { get; set; }
    public GameManager _gameManager { get; set; }
    
    
    
    bool checkNoDraws()
    {
        Dictionary<ulong, PlayerScoreBoard> potentialwinners = new Dictionary<ulong, PlayerScoreBoard>();
        var winner = _netManager.playerList[0];

        bool draw = false;
        var player_holder = _netManager.GetAllPlayerDataConnected();

        foreach (var player in player_holder)
        {

            var v = player.ClientID;

            var playerdata = _netManager.GetPlayerDataBasedOnClientID(v);

            //check list to see if player is drawing
            if (playerdata.playerScore == winner.playerScore)
            {
                var scoreboard = _gameManager.UIManager.FecthPlayerScoreBoardOnID(player.ClientID);
                //if player is drawing add to list
                potentialwinners.Add(player.ClientID, scoreboard);
            }
            else if (playerdata.playerScore > winner.playerScore)
            {
                var scoreboard = _gameManager.UIManager.FecthPlayerScoreBoardOnID(player.ClientID);

                //if player is not drawing and has a higher score than the current winner and clear the list
                winner = playerdata;

                potentialwinners.Clear();
                potentialwinners.Add(v, scoreboard);
            }
        }

        if (potentialwinners.Count > 1)
        {
            _gameManager.RequestTimeIncreaseDueToOvertime();
            return false;
        }
        
        return true;
    }

   
    
    
    
    public Dictionary<ulong, PlayerScoreBoard> FindWinner()
    {
        var dictionaryofwin = new Dictionary<ulong, PlayerScoreBoard>();

        var winner = new PlayerScoreBoard();

        //  Debug.Log(player_UIholder.Count);
        var player_holder = _netManager.GetAllPlayerDataConnected();
        //win condition here
        foreach (var player in player_holder)
        {
            // if (player.playerData.Playerscore > winner.playerData.Playerscore)
            // {
            //     winner = player;
            // }
            Debug.Log("player score is " + player.ClientID);
            var data = _netManager.GetPlayerDataBasedOnClientID(player.ClientID);

            if (data.ClientID == 0)
            {
                var scoreboard = _gameManager.UIManager.FecthPlayerScoreBoardOnID(player.ClientID);
                dictionaryofwin.Add(player.ClientID, scoreboard);
                winner = scoreboard;
            }
        }

        return dictionaryofwin;
    }
    
    
}
