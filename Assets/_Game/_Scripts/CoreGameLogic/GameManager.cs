using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Random = UnityEngine.Random;


public class GameManager : NetworkBehaviour
{


    public enum GameState
    {
        Loading,
        Start,
        Play,
        End
    }
    [Header("Configuration Settings")]
    [Space(10)]

    [SerializeField]
    private GameSettings _gameSettings;

    [Header("Player Elements")]
    [Space(10)]

    [SerializeField]
    private Transform[] _spawnPoints;


    [Header("UI Elements")]
    [Space(10)]
    [SerializeField]
    public UiManager UIManager;

    [Header("Testing Elements")]
    [Space(10)]
    [SerializeField]
    private bool _isTesting;

    [SerializeField]
    private TextMeshProUGUI _testModeText;

    [HideInInspector]
    public string _winnerName;
    
    
    [Header("Multiplayer Variables")]
    [SerializeField]private NetworkVariable<float> _timeleftinGame = new(writePerm: NetworkVariableWritePermission.Server);
    [SerializeField] public List<ISessionPlayerUiData> player_holder = new List<ISessionPlayerUiData>();
    [SerializeField]private NetworkVariable<GameState> _gameState = new(writePerm: NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<ISessionPlayerData> _playerwinner = new(writePerm: NetworkVariableWritePermission.Server);


    //gameclientonlyvariable
    bool startedclient;

    NetManage _netManager;
    


    //begin
    private void Awake()
    {
        UIManager._gameManager = this;
        _netManager = GameObject.FindObjectOfType<NetManage>();
        EventDelegates();

    }

    void EventDelegates()
    {
        //EventManager.OnGameStart += OnGameStart;
        // EventManager.OnGameEnd += OnGameEnd;
        // EventManager.OnGameFull += OnGameFull;
        // EventManager.OnPlayerLeft += OnPlayerScored;
        // EventManager.OnPlayerJoined += OnPlayerScored;
    }



    void Start()
    {
        if (!_isTesting)
        {
            _testModeText?.gameObject.SetActive(false);

            //  StartCoroutine(VeryFirstGameLoop());
        }
        else
        {
            ResetGameState();
            _testModeText.gameObject.SetActive(true);
            // _animatorCamera.Play("_gameidle");


        }
        
    }



    public IEnumerator VeryFirstGameLoop()
    {
        if (IsServer)
        {
            _gameState.Value = GameState.Loading;
        }

        // while (!_animatorCamera.GetCurrentAnimatorStateInfo(0).IsName("_gameidle"))
        // {
        //     yield return null;
        // }
        yield return new WaitForSeconds(0.1f);
        ResetGameState();

    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            GameHeartbeat();
        }
        else
        {
            GameClientUpdate();
        }

        GameUIHandling();

    }

    void ResetGameState()
    {
        // PlayerSetups();
        UIManager._resetButton?.SetActive(false);
        

        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.Start;
        }

        if (IsServer)
        {
            _timeleftinGame.Value = _gameSettings.gameTimer;

        }

        UIManager._AnnoucePanel.SetActive(true);
        UIManager._overtimeText.gameObject.SetActive(false);
        UIManager._annoucerText.text = "Get Ready!";
        UIManager._HUDPanel.SetActive(false);
        StartCoroutine(StartGame());

    }

    IEnumerator StartGame()
    {
        UIManager._CountdownPanel.SetActive(true);
        UIManager._countdownText.text = _gameSettings.gameStartDelay.ToString("0");
        var timeleft = 0f;
        for (timeleft = _gameSettings.gameStartDelay; timeleft > 0; timeleft -= Time.deltaTime)
        {
            UIManager._CountdownPanel.SetActive(true);
            UIManager._countdownText.text = timeleft.ToString("0");
            yield return null;
        }

        UIManager._CountdownPanel.SetActive(false);
        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.Play;
        }

        EventManager.StartGame();
    }

    IEnumerator EndGame()
    {
        //  _coinDropController.CancelHeartbeat();
        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.End;
            var scorebaord =  checkWinner();

            var winner = scorebaord.First();
            ProcessEndGame(winner.Key,winner.Value);
            
        }

        UIManager._resetButton.SetActive(true);
        EventManager.EndGame();
        yield return new WaitForSeconds(_gameSettings.gameEndDelay);
    }

    Dictionary<ulong,PlayerScoreBoard> checkWinner()
    {
        
        var dictionaryofwin = new Dictionary<ulong,PlayerScoreBoard>();

        var winner = new PlayerScoreBoard();
        
        Debug.Log(player_holder.Count);

        //win condition here
         foreach (var player in player_holder)
         {
             // if (player.playerData.Playerscore > winner.playerData.Playerscore)
             // {
             //     winner = player;
             // }
             Debug.Log("player score is " + player.ClientID);
             var data = _netManager.GetPlayerDataBasedOnClientID(player.ClientID);
             
             if(data.ClientID == 1)
             {
                 dictionaryofwin.Add(player.ClientID, player.PlayerScoreBoardUnit);
                 winner = player.PlayerScoreBoardUnit;

             }
             
         }

        _winnerName = winner.playerName;
        UIManager._annoucerText.text = winner.playerName + " Wins!";

        return dictionaryofwin;


    }

    bool checkNoDraws()
    {
        Dictionary<ulong, PlayerScoreBoard> potentialwinners = new Dictionary<ulong, PlayerScoreBoard>();
        var winner = _netManager.playerList[0];
        
        bool draw = false;
        foreach (var player in player_holder)
        {

            var v = player.ClientID;
            
            var playerdata = _netManager.GetPlayerDataBasedOnClientID(v);
            
            //check list to see if player is drawing
            if (playerdata.playerScore == winner.playerScore)
            {
                //if player is drawing add to list
                potentialwinners.Add(player.ClientID,player.PlayerScoreBoardUnit);
            }
            else if (playerdata.playerScore > winner.playerScore)
            {

                //if player is not drawing and has a higher score than the current winner and clear the list
                winner = playerdata;
                
                potentialwinners.Clear();
                potentialwinners.Add(v,player.PlayerScoreBoardUnit);
            }
        }

        if (potentialwinners.Count > 1)
        {
            IncreaseTimeDueToDraw();
            return false;
        }


        return true;
    }

    void IncreaseTimeDueToDraw()
    {
        if (IsServer)
        {
            _timeleftinGame.Value += _gameSettings.gameOvertime;
        }

        StartCoroutine(UIManager.OvertimeText(_gameSettings.gameOvertime));
    }

    void GameHeartbeat()
    {

        if (_gameState.Value != GameState.Play)
        {
            return;
        }

        if (IsServer)
        {
            _timeleftinGame.Value -= Time.unscaledDeltaTime;
        }
        //uncomment for draw
        // if(_timeleftinGame <= 5)
        // {
        //     checkNoDraws();
        // }

        // if(_timeleftinGame <= 0 && checkNoDraws())
        // {
        //     StartCoroutine(EndGame());
        // }
        if (_timeleftinGame.Value <= 0)
        {
            StartCoroutine(EndGame());
        }
    }


    #region GameUI

    void GameUIHandling()
    {
        UIManager.HandleUi(CheckUIForPlayers, _gameState.Value, _timeleftinGame.Value);
    }

    // IEnumerator OvertimeText()
    // {
    //     UIManager._overtimeText.gameObject.SetActive(true);
    //     UIManager._overtimeText.text = "Overtime!" + _gameSettings.gameOvertime.ToString();
    //     yield return new WaitForSeconds(3f);
    //     UIManager._overtimeText.gameObject.SetActive(false);
    //
    // }

    
  


    #endregion




    #region PlayerSetup

    public delegate void onScoreChanged();

    public onScoreChanged OnScoreChanged;
    

    #endregion


    #region CleanUp

   
    
    // private void LastGameCleanUpCheck()
    // {
    //     if (player_holder != null)
    //     {
    //         ClearPlayers();
    //     }
    // }
    //
    // void ClearPlayers()
    // {
    //     foreach (var VARIABLE in player_holder)
    //     {
    //         Destroy(VARIABLE.PlayerScoreBoardUnit.playerScoreUI.gameObject);
    //         Destroy(VARIABLE.PlayerScoreBoardUnit.playerObject.gameObject);
    //
    //     }
    //         
    // }

    #endregion


    #region Multiplayer only
    
    public void LastGameCleanUpCheckMulti()
    {
        if (player_holder != null)
        {
            foreach (var VARIABLE in player_holder)
            {
                VARIABLE.PlayerScoreBoardUnit.Clear();
            }
        }
        
        
        player_holder.Clear();

        if (IsServer)
        {
           // _playerdata.Clear();
           _netManager.ClearPlayerData();
        }
    }
    void GameClientUpdate()
    {
        if (_gameState.Value == GameState.Start && !startedclient)
        {
            startedclient = true;
            ResetGameState();
            
        }
    }

    public void ProcessEndGame(ulong clientid,PlayerScoreBoard winner)
    {
        if (!IsServer)
        {
            return;
        }
        
        var clientdata = _netManager.GetPlayerDataBasedOnClientID(clientid);

        _playerwinner.Value = clientdata;
        List<RPCManager> v = new List<RPCManager>();
        foreach (var VARIABLE in FindObjectsOfType<RPCManager>())
        {
            v.Add(VARIABLE);
            VARIABLE.WinnerAnimState(clientid);
        }
        
        
        EndGameUi(_playerwinner.Value.ClientID);
        RPCWinnerAnnoucedClientRPC();

    }
    
    public void ProcessEndGameClientSide()
    {
        if (!IsServer)
        {

           // Debug.Log("winner is " + _playerwinner.Value.ClientID);
            
            EndGameUi(_playerwinner.Value.ClientID);
        }


    }

    void EndGameUi(ulong clientid)
    {
        UIManager._timeLeftText.text = "0:00";

        var v = _netManager.GetAllPlayerDataConnected();

        var winner = v.Select(data => data).Where(data => data.ClientID == clientid);
        
        var winnerscoreboard = player_holder.Select(data => data).Where(data => data.ClientID == clientid);
        
        winnerscoreboard.First().PlayerScoreBoardUnit.playerScoreUI.Winner();
        
    }
   
    

    public bool HasGameStarted()
    {
        if (_gameState.Value == GameState.Play)
        {
            return true;
        }

        return false;
    }
    
    #region UIMultiplayer
    bool recheck = false;

    void CheckUIForPlayers()
    {
        if(_netManager.GetTotalNumberOfPlayers() == 0){return;}
      // Debug.Log("Checking UI for players" + _netManager.GetTotalNumberOfConnectedPlayers() + " || " + player_holder.Count + " || "+ _netManager.playerList[0].ClientID + " || " + _netManager.playerList[1].ClientID);
        if(player_holder.Count != _netManager.GetTotalNumberOfConnectedPlayers() && _gameState.Value != GameState.Play)
        {
            PlayerScoreboardCleanup();
        }else if(recheck)
        {
            recheck = false;
            
            PlayerScoreboardCleanup();
           
        }

    }

    private void PlayerScoreboardCleanup()
    {
        foreach (var VARIABLE in player_holder)
        {
            VARIABLE.PlayerScoreBoardUnit.Clear();
        }

      

        AddPlayerScoreboardFromNetwork();
    }

    private void AddPlayerScoreboardFromNetwork()
    {
        player_holder.Clear();
        player_holder = new System.Collections.Generic.List<ISessionPlayerUiData>();
        var players = _netManager.GetAllPlayerDataConnected();

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].IsConnected)
            {
                var v = CreatePlayerScoreboard(players[i]);
                var data = new ISessionPlayerUiData(players[i].ClientID, v);
                player_holder.Add(data);
            }
        }
    }

    private void checkUIOnReconnect(ulong obj)
    {
        recheck = true;
    }
    void FuzzyCheckForPlayerStillConnected(ulong clientid)
    {
       // Debug.Log("Fuzzy check for player still connected");
    
         var scoreboard = player_holder.Select(data => data).Where(data => data.ClientID == clientid);
        scoreboard.First().PlayerScoreBoardUnit.disconnect();
       

    }

    
    PlayerScoreBoard CreatePlayerScoreboard(ISessionPlayerData playerData)
    {
        
        if(playerData.IsConnected)
        {
            var v = new PlayerScoreBoard();
          //  v.playerData = playerData;
            v.playerScoreUI = Instantiate(_gameSettings.playerScoreUI, UIManager._playerScoreUIParent);
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
    
    
    
    
    #endregion
    
    
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientDisconnectCallback += FuzzyCheckForPlayerStillConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += checkUIOnReconnect;
        
        
        if(IsServer)
        {
            GetComponent<NetManage>().OnAllPlayersConnected += BeginGame;
           // NetManage.OnAllPlayersConnected += BeginGame;
        }


    }

    void BeginGame( object sender, EventArgs e)
    {
        
        
        
        StartCoroutine(VeryFirstGameLoop());
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.OnClientDisconnectCallback -= FuzzyCheckForPlayerStillConnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= checkUIOnReconnect;
        

        
        if(IsServer)
        {
            GetComponent<NetManage>().OnAllPlayersConnected -= BeginGame;
           // NetManage.OnAllPlayersConnected -= BeginGame;
        }
        //NetManage.OnAllPlayersConnected -= BeginGame;

    }
    

    [ClientRpc]
    void RPCWinnerAnnoucedClientRPC()
    {
        ProcessEndGameClientSide();
    }

 
    public void OnPlayerSpawned(ulong networkObJid)
    {
        Debug.Log("Player spawned on server");
       // var clientdata = _netManager.GetPlayerDataBasedOnClientID(clientid);
        int i = Random.Range(0, _spawnPoints.Length);
            
            var g = NetworkManager.SpawnManager.SpawnedObjects[networkObJid];
          //  g.gameObject.GetComponent<NetworkTransform>().Teleport(_spawnPoints[i].transform.position,g.transform.rotation,g.transform.localScale);
            g.gameObject.GetComponent<Transform>().position = _spawnPoints[i].transform.position + new Vector3(0, 1, 0);

    }
    
    
    
    


}



#endregion



