using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;

[Serializable]
public class UiManager
{
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
    
    
    public UiManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public void HandleUi( Action callback, GameManager.GameState _gameState, float _timeleftinGame)
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

        callback?.Invoke();
    }
    
   public IEnumerator OvertimeText(float timePassed)
    {
        _overtimeText.gameObject.SetActive(true);
        _overtimeText.text = "Overtime!" + timePassed.ToString();
        yield return new WaitForSeconds(3f);
       _overtimeText.gameObject.SetActive(false);

    }
    
    
    
    
    
    
    
    
}