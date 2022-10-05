using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayEventManager : MonoBehaviour
{
    public delegate void GameStart();
    public static event GameStart GameStartEvent;
    
    public delegate void GameEnd();
    public static event GameEnd GameEndEvent;
    
    public delegate void PlayerJoined();
    public static event PlayerJoined PlayerJoinedEvent;
    
    public delegate void PlayerLeft();
    public static event PlayerLeft PlayerLeftEvent;
    
    public delegate void GameFull();
    public static event GameFull GameFullEvent;
    
    
    public delegate void GameCleanup();
    public static event GameCleanup GameCleanUp;

    
    public static void StartGame()
    {
        if (GameStartEvent != null)
        {
            GameStartEvent();
        }
    }
    
    
    public static void EndGame()
    {
        if (GameEndEvent != null)
        {
            GameEndEvent();
        }
    }
    
    
    public static void PlayerJoin()
    {
        if (PlayerJoinedEvent != null)
        {
            PlayerJoinedEvent();
        }
    }
    
    public static void PlayerLeftInvoke()
    {
        if (PlayerLeftEvent != null)
        {
            PlayerLeftEvent();
        }
    }
    
    public static void GameFullInvoke()
    {
        if (GameFullEvent != null)
        {
            GameFullEvent();
        }
    }
    
    
    // Start is called before the first frame update
    public static void GameCleanUpInvoke()
    {
        if(GameCleanUp != null)
        {
            GameCleanUp();
        }
    }
}
