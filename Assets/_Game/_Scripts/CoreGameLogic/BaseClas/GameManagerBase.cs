using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerBase : MonoBehaviour
{
    
    public virtual void Awake()
    {
        Debug.Log("GameManagerBase Start");
    }
    
    public virtual void EventDelegates()
    {
        Debug.Log("GameManagerBase EventDelegate");
    }
    
    public virtual void Start()
    {
        Debug.Log("GameManagerBase Start");
    }
    
    public virtual void Update()
    {
        Debug.Log("GameManagerBase Update");
    }
    
    public virtual IEnumerator VeryFirstGameLoop()
    {

        yield return null;
    }
    
    public virtual IEnumerator StartGame()
    {

        yield return null;
    }
    
    public virtual IEnumerator EndGame()
    {

        yield return null;
    }
    
    public virtual void CheckWinner()
    {
        Debug.Log("GameManagerBase CheckWinner");
    }
    
    public virtual bool Checknodraws()
    {
        Debug.Log("GameManagerBase CheckWinner");

        return false;
    }

    public virtual void IncreaseTimeDueToDraw()
    {
        
    }

    public virtual void GameHeartbeat()
    {
        
    }
    
    public virtual void ResetGameState()
    {
        
    }

}
