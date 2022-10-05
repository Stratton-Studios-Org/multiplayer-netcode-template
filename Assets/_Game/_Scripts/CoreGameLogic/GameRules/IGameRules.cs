using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameRules
{

   
    // Start is called before the first frame update
    void ConditionToComplete();
    
    void CheckCondition();
    public  bool Success { get; set; }
    public  bool Fail { get; set; }
    
   public NetManage _netManager { get; set; }
    public GameManager _gameManager{get; set;}

    Dictionary<ulong, PlayerScoreBoard> FindWinner();
}
