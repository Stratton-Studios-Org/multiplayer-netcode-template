using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScoreUI : MonoBehaviour
{   
    GameManager gameStateManager;
    PlayerScoreBoard playerScoreBoard;
    
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI playerNameText;
    // Start is called before the first frame update
    void Start()
    {
        gameStateManager = FindObjectOfType<GameManager>();
        //gameStateManager.onScoreChanged += UpdateScore;
        
    }

    public void Init(PlayerScoreBoard playerScoreBoard)
    {
        this.playerScoreBoard = playerScoreBoard;
        playerNameText.text = playerScoreBoard.playerName;
       // playerScoreText.text = playerScoreBoard.playerData.Playerscore.ToString();
    }
    
    public void Inactive()
    {
        playerNameText.text = "disconnected";
    }
    
    public void Winner()
    {
        playerNameText.text = "Winner";
    }


    public void UpdateScore(string playerName, int score)
    {
        if(playerName != playerScoreBoard.playerName)
        {
            return;
        }
        playerScoreText.text = score.ToString();
        playerNameText.text = playerName;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
