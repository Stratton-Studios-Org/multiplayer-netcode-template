using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SessionPlayerUiData
{

    public ulong ClientID;
    public PlayerScoreBoard PlayerScoreBoardUnit;

    public SessionPlayerUiData(ulong clientID, PlayerScoreBoard playerScoreBoard)
    {
        ClientID = clientID;
        PlayerScoreBoardUnit = playerScoreBoard;
    }
}
