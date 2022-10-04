using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CreateAssetMenu(fileName = "Data", menuName = "GameManager/Settings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("Music Settings")]
    public float musicVolume;
    public float sfxVolume;
    public float masterVolume;
    [Space(10)]
    [Header("Game Settings")]
    public float gameOvertime;
    public float gameStartDelay;
    public float gameEndDelay;
    public float gameTimer;
    public int maxPlayers;
    public int minPlayers;
    [Space(10)] 
    [Header("Coin Settings")]
    public float coinSpawnDelayMin;
    public float coinSpawnDelayMax;
    public int coinSpawnAmountMax;
    public int maxCoinsinScene;
    
    [Header("Player Settings")]
    [Space(10)] 
    public GameObject playerPrefabs;
    public PlayerScoreUI playerScoreUI;
}
