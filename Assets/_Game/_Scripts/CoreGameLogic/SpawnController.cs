using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;


[Serializable]
public class SpawnController 
{
    [SerializeField]
    private Transform[] _spawnPoints;
    
    [HideInInspector]public GameManager _gameManager;
    
    public SpawnController(GameManager gameManager)
    {
     //   _gameManager = gameManager;
        init();

    }

    public void init()
    {
        _gameManager.PlayerSpawned += OnPlayerSpawned;
    }
    
    void OnDisable()
    {
        _gameManager.PlayerSpawned -= OnPlayerSpawned;
    }
    public void OnPlayerSpawned(ulong networkObJid)
    {
        Debug.Log("Player spawned on server");
        int i = Random.Range(0, _spawnPoints.Length);
        
        
        var g = _gameManager.GetNetManager().SpawnManager.SpawnedObjects[networkObJid];
       // var g = NetworkManager.SpawnedObjects[networkObJid];
        
        g.gameObject.GetComponent<Transform>().position = _spawnPoints[i].transform.position + new Vector3(0, 1, 0);

    }
}
