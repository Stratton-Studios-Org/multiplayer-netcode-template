using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ISessionPlayerUiData
{

    public ulong ClientID;
    public PlayerScoreBoard PlayerScoreBoardUnit;

    public ISessionPlayerUiData(ulong clientID, PlayerScoreBoard playerScoreBoard)
    {
        ClientID = clientID;
        PlayerScoreBoardUnit = playerScoreBoard;
    }
}
