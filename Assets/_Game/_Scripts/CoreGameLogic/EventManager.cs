using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void GameStart();
    public static event GameStart OnGameStart;
    
    public delegate void GameEnd();
    public static event GameEnd OnGameEnd;
    
    public delegate void PlayerJoined();
    public static event PlayerJoined OnPlayerJoined;
    
    public delegate void PlayerLeft();
    public static event PlayerLeft OnPlayerLeft;
    
    public delegate void GameFull();
    public static event GameFull OnGameFull;
    
    
    public static void StartGame()
    {
        if (OnGameStart != null)
        {
            OnGameStart();
        }
    }
    
    
    public static void EndGame()
    {
        if (OnGameEnd != null)
        {
            OnGameEnd();
        }
    }
    
    
    public static void PlayerJoin()
    {
        if (OnPlayerJoined != null)
        {
            OnPlayerJoined();
        }
    }
    
    public static void PlayerLeftInvoke()
    {
        if (OnPlayerLeft != null)
        {
            OnPlayerLeft();
        }
    }
    
    public static void GameFullInvoke()
    {
        if (OnGameFull != null)
        {
            OnGameFull();
        }
    }
    
    
    // Start is called before the first frame update
}
