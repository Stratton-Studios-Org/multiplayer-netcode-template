using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


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
    
    private IGameRules _gameRules;

    [Header("Player Elements")]
    [Space(10)]

    [SerializeField]public SpawnController spawnController;

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

    public List<IGameRules> WinConditions;

    [Header("Multiplayer Variables")]
    [SerializeField]
    private NetworkVariable<float> _timeleftinGame = new(writePerm: NetworkVariableWritePermission.Server);

    [SerializeField]
    private NetworkVariable<GameState> _gameState = new(writePerm: NetworkVariableWritePermission.Server);

    [SerializeField]
    private NetworkVariable<SessionPlayerData> _playerwinner = new(writePerm: NetworkVariableWritePermission.Server);

    //gameclientonlyvariable
    bool startedclient;

    NetManage _netManager;

    public event EventHandler Spawned;

    public event EventHandler Despawned;

    public event EventHandler CountdownStarted;
    public event Action<float> CountdownTick;
    public event EventHandler CountdownFinished;

    public event EventHandler GameReset;

    public Action<GameState,float> GameTick;
    public event Action<float> GameOvertimeTriggered;


    public Action<ulong> PlayerSpawned;



    //begin
    private void Awake()
    {

        _netManager = GameObject.FindObjectOfType<NetManage>();
        UIManager._gameManager = this;
        UIManager._gameSettings = _gameSettings;
        UIManager._netManager = _netManager;
        _gameRules = new WinOnTimeLimit(this,_netManager);
        
        spawnController._gameManager = this;
        spawnController.init();
  
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
        GameTick?.Invoke(_gameState.Value, _timeleftinGame.Value);
    }

    void ResetGameState()
    {
        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.Start;
        }

        if (IsServer)
        {
            _timeleftinGame.Value = _gameSettings.gameTimer;

        }
        GameReset?.Invoke(this, EventArgs.Empty);

        StartCoroutine(StartGame());

    }

    IEnumerator StartGame()
    {
       
        CountdownStarted?.Invoke(this, EventArgs.Empty);
        var timeleft = 0f;
        for (timeleft = _gameSettings.gameStartDelay; timeleft > 0; timeleft -= Time.deltaTime)
        {
        
            CountdownTick?.Invoke(timeleft);
            
            yield return null;
        }
        CountdownFinished?.Invoke(this, EventArgs.Empty);
        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.Play;
        }

        GamePlayEventManager.StartGame();
    }

    IEnumerator EndGame()
    {
        //  _coinDropController.CancelHeartbeat();
        ///serverlogic
        if (IsServer)
        {
            _gameState.Value = GameState.End;
           // var scorebaord = checkWinner();
           var scorebaord = _gameRules.FindWinner();

            

            var winner = scorebaord.First();
            ProcessEndGame(winner.Key, winner.Value);
            GamePlayEventManager.EndGame();


        }

       
        

        yield return new WaitForSeconds(_gameSettings.gameEndDelay);
    }

    public void RequestTimeIncreaseDueToOvertime()
    {
        if (IsServer)
        {
            _timeleftinGame.Value += _gameSettings.gameOvertime;
        }

        GameOvertimeTriggered?.Invoke(_gameSettings.gameOvertime);
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


    #region Multiplayer only

    void GameClientUpdate()
    {
        if (_gameState.Value == GameState.Start && !startedclient)
        {
            startedclient = true;
            ResetGameState();

        }
    }

    public void ProcessEndGame(ulong clientid, PlayerScoreBoard winner)
    {
        if (!IsServer)
        {
            return;
        }

        var clientdata = _netManager.GetPlayerDataBasedOnClientID(clientid);

        _playerwinner.Value = clientdata;
        // List<RPCManager> v = new List<RPCManager>();
        // foreach (var VARIABLE in FindObjectsOfType<RPCManager>())
        // {
        //     v.Add(VARIABLE);
        //     VARIABLE.WinnerAnimState(clientid);
        // }

    }

    void OnPlayerWinnerDecalred(SessionPlayerData before, SessionPlayerData winner)
    {
        if (IsServer)
        {
            return;
        }
        GamePlayEventManager.EndGame();

     //   ProcessEndGameClientSide();
    }
    


    public bool HasGameStarted()
    {
        if (_gameState.Value == GameState.Play)
        {
            return true;
        }

        return false;
    }

    public ulong getWinner()
    {
        return _playerwinner.Value.ClientID;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Spawned.Invoke(this, EventArgs.Empty);
        if (IsServer)
        {
            GetComponent<NetManage>().OnAllPlayersConnected += BeginGame;
            // NetManage.OnAllPlayersConnected += BeginGame;
        }else
        {
            _playerwinner.OnValueChanged += OnPlayerWinnerDecalred;
        }
    }

    void BeginGame(object sender, EventArgs e)
    {
        StartCoroutine(VeryFirstGameLoop());
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Despawned.Invoke(this, EventArgs.Empty);
        if (IsServer)
        {
            GetComponent<NetManage>().OnAllPlayersConnected -= BeginGame;
        }else
        {
            _playerwinner.OnValueChanged -= OnPlayerWinnerDecalred;
        }
    }

    // public void OnPlayerSpawned(ulong networkObJid)
    // {
    //     Debug.Log("Player spawned on server");
    //     int i = Random.Range(0, _spawnPoints.Length);
    //
    //     var g = NetworkManager.SpawnManager.SpawnedObjects[networkObJid];
    //     g.gameObject.GetComponent<Transform>().position = _spawnPoints[i].transform.position + new Vector3(0, 1, 0);
    //
    // }
    
    public NetworkManager GetNetManager()
    {
        return NetworkManager;
    }
    

    #endregion


}







