using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

[Serializable]
public class UiManager : MonoBehaviour
{
    
    [SerializeField] public List<SessionPlayerUiData> player_UIholder = new List<SessionPlayerUiData>();
    
    [SerializeField]
    public Transform _playerScoreUIParent;

    [SerializeField]
    public TextMeshProUGUI _timeLeftText;

    [SerializeField]
    public TextMeshProUGUI _scoreText;

    [SerializeField]
    public TextMeshProUGUI _annoucerText;

    [SerializeField]
    public TextMeshProUGUI _overtimeText;

    [SerializeField]
    public TextMeshProUGUI _countdownText;

    [SerializeField]
    public GameObject _CountdownPanel;

    [SerializeField]
    public GameObject _AnnoucePanel;

    [SerializeField]
    public GameObject _HUDPanel;

    [SerializeField]
    public GameObject _resetButton;


    
    [HideInInspector]public GameManager _gameManager;
   
    [HideInInspector]public GameSettings _gameSettings;
    
    [HideInInspector]public NetManage _netManager;
    public UiManager(GameManager gameManager, GameSettings _gameSettings, NetManage netManager)
    {
        _gameManager = gameManager;
        _gameSettings = _gameSettings;
        _netManager = netManager;
    }


    public void OnEnable()
    {
        _gameManager = GameObject.FindObjectOfType<GameManager>();
        _gameManager.Spawned += OnSpawn;
        _gameManager.Despawned += OnDespawn;
        _gameManager.CountdownStarted += CountdownBegun;
        _gameManager.CountdownTick += CountdownUpdate;
        _gameManager.CountdownFinished += CountdownEnded;
        _gameManager.GameTick += HandleUi;
        _gameManager.GameReset += OnGameReset;
        _gameManager.GameOvertimeTriggered += OnOvertime;
            
    }

   

    private void OnDisable()
    {
     
      
            _gameManager.Spawned -= OnSpawn;
            _gameManager.Despawned -= OnDespawn;
            _gameManager.CountdownStarted -= CountdownBegun;
            _gameManager.CountdownTick -= CountdownUpdate;
            _gameManager.CountdownFinished -= CountdownEnded;
            _gameManager.GameTick -= HandleUi;
            _gameManager.GameReset -= OnGameReset;

        

    }

    #region Countdown Events

    

    void CountdownBegun(object obj, EventArgs args)
    {
        _CountdownPanel.SetActive(true);
        _countdownText.text = _gameSettings.gameStartDelay.ToString("0");
    }

    private void CountdownUpdate(float obj)
    {
        _CountdownPanel.SetActive(true);
        _countdownText.text = obj.ToString("0");
    }
    
    
    private void CountdownEnded(object obj, EventArgs args)
    {
        _CountdownPanel.SetActive(false);
       
    }
    
    
    #endregion

    //called when the game is reseting
    void OnGameReset(object obj, EventArgs args)
    {
        _AnnoucePanel.SetActive(true);
        _overtimeText.gameObject.SetActive(false);
        _annoucerText.text = "Get Ready!";
        _HUDPanel.SetActive(false);
        _resetButton?.SetActive(false);
    }

    public  void OnDespawn(object obj, EventArgs args)
    {
  
         NetworkManager.Singleton.OnClientDisconnectCallback -= FuzzyCheckForPlayerStillConnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= checkUIOnReconnect;
         GamePlayEventManager.GameEndEvent -= OnGameEndUI;
    }


    public void OnSpawn(object obj, EventArgs args)
    {
     
         NetworkManager.Singleton.OnClientDisconnectCallback += FuzzyCheckForPlayerStillConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += checkUIOnReconnect;
         GamePlayEventManager.GameEndEvent += OnGameEndUI;
    }
    
    public void HandleUi( GameManager.GameState _gameState, float _timeleftinGame)
    {
        if (_gameState == GameManager.GameState.Loading)
        {
            _AnnoucePanel.SetActive(false);
            _HUDPanel.SetActive(false);
            _CountdownPanel.SetActive(false);

            _annoucerText.text = "Game Over";
        }
        else if (_gameState == GameManager.GameState.Play)
        {
           _AnnoucePanel.SetActive(false);
            _HUDPanel.SetActive(true);
            _timeLeftText.text = _timeleftinGame.ToString("F2");


        }
        else if (_gameState == GameManager.GameState.End)
        {
           _AnnoucePanel.SetActive(true);
            _HUDPanel.SetActive(false);
            _annoucerText.text = "Game Over";
        }

        CheckUIForPlayers();
    }
    
    public void OnOvertime(float obj)
    {
        StartCoroutine(OvertimeText(obj));
    }
    
   public IEnumerator OvertimeText(float timePassed)
    {
        _overtimeText.gameObject.SetActive(true);
        _overtimeText.text = "Overtime!" + timePassed.ToString();
        yield return new WaitForSeconds(3f);
       _overtimeText.gameObject.SetActive(false);

    }
    
   void OnGameEndUI()
   {
       _resetButton.SetActive(true);
       EndGameUi(_gameManager.getWinner());
   }
   
   public void EndGameUi(ulong clientid)
   {
       
       var g = _netManager.GetPlayerDataBasedOnClientID(clientid);
       _annoucerText.text = g.systemID + " Wins!";
       _timeLeftText.text = "0:00";

       var v = _netManager.GetAllPlayerDataConnected();

       var winner = v.Select(data => data).Where(data => data.ClientID == clientid);
       var winnerscoreboard = player_UIholder.Select(data => data).Where(data => data.ClientID == clientid);
        
       winnerscoreboard.First().PlayerScoreBoardUnit.playerScoreUI.Winner();
   }
  
  
   bool recheck = false;
   
   private void checkUIOnReconnect(ulong obj)
   {
       recheck = true;
   }

   void CheckUIForPlayers()
   {
       if(_netManager.GetTotalNumberOfPlayers() == 0){return;}
       // Debug.Log("Checking UI for players" + _netManager.GetTotalNumberOfConnectedPlayers() + " || " + player_UIholder.Count + " || "+ _netManager.playerList[0].ClientID + " || " + _netManager.playerList[1].ClientID);
       if(player_UIholder.Count != _netManager.GetTotalNumberOfConnectedPlayers() && !_gameManager.HasGameStarted())
       {
           PlayerScoreboardCleanup();
       }else if(recheck)
       {
           recheck = false;
            
           PlayerScoreboardCleanup();
           
       }

   }
    
   void FuzzyCheckForPlayerStillConnected(ulong clientid)
   {
       // Debug.Log("Fuzzy check for player still connected");
    
       var scoreboard = player_UIholder.Select(data => data).Where(data => data.ClientID == clientid);
       scoreboard.First().PlayerScoreBoardUnit.disconnect();
       

   }
  
   public PlayerScoreBoard FecthPlayerScoreBoardOnID(ulong clientid)
   {
       var scoreboard = player_UIholder.Select(data => data).Where(data => data.ClientID == clientid);
       Debug.Log("FecthPlayerScoreBoardOnID " + clientid + scoreboard.First().PlayerScoreBoardUnit);
       return scoreboard.First().PlayerScoreBoardUnit;
   }
  
  
   PlayerScoreBoard CreatePlayerScoreboard(SessionPlayerData playerData)
   {
        
       if(playerData.IsConnected)
       {
           var v = new PlayerScoreBoard();
           //  v.playerData = playerData;
           v.playerScoreUI = Instantiate(_gameSettings.playerScoreUI, _playerScoreUIParent);
           v.playerScoreUI.Init(v);
           return v;
       }else
       {
           var v = new PlayerScoreBoard();
           //  v.playerData = playerData;
           Debug.Log("Player not connected");

           return v;
       }
       
   }
  
  
   public void LastGameCleanUpCheckMulti()
   {
       if (player_UIholder != null)
       {
           foreach (var VARIABLE in player_UIholder)
           {
               VARIABLE.PlayerScoreBoardUnit.Clear();
           }
       }
        
        
       player_UIholder.Clear();

   }
   
   
   private void AddPlayerScoreboardFromNetwork()
   {
       player_UIholder.Clear();
       player_UIholder = new System.Collections.Generic.List<SessionPlayerUiData>();
       var players = _netManager.GetAllPlayerDataConnected();

       for (int i = 0; i < players.Count; i++)
       {
           if (players[i].IsConnected)
           {
               var v = CreatePlayerScoreboard(players[i]);
               var data = new SessionPlayerUiData(players[i].ClientID, v);
               player_UIholder.Add(data);
           }
       }
   }
  
   private void PlayerScoreboardCleanup()
   {
       foreach (var VARIABLE in player_UIholder)
       {
           VARIABLE.PlayerScoreBoardUnit.Clear();
       }

       AddPlayerScoreboardFromNetwork();
   }

  
    
    
    
    
}