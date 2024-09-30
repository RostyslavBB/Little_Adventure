using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public GameManager gameManager;
    public TMPro.TextMeshProUGUI coinText;
    public Slider healthSlider;
    public GameObject UIPause;
    public GameObject UIGameOver;
    public GameObject UIGameIsFinished;

    private enum gameUIState
    {
        GamePlay,
        Pause,
        GameOver,
        GameIsFinished
    }
    private gameUIState _currentState;
    private void Start()
    {
        SwitchUIState(gameUIState.GamePlay);
    }
    private void Update()
    {
        healthSlider.value = gameManager.playerCharacter.GetComponent<Health>().currentHealthPercentage;
        coinText.text = gameManager.playerCharacter.coin.ToString();
    }
    private void SwitchUIState(gameUIState state) 
    {
        UIPause.SetActive(false);
        UIGameOver.SetActive(false);
        UIGameIsFinished.SetActive(false);
        Time.timeScale = 1.0f;
        switch(state)
        {
            case gameUIState.GamePlay:
                break;
            case gameUIState.Pause:
                Time.timeScale = 0f;
                UIPause.SetActive(true);
                break;
            case gameUIState.GameOver:
                UIGameOver.SetActive(true);
                break;
            case gameUIState.GameIsFinished:
                UIGameIsFinished.SetActive(true);
                break;
        }
        _currentState = state;
    }
    public void TogglePauseUI()
    {
        if (_currentState == gameUIState.GamePlay)
            SwitchUIState(gameUIState.Pause);
        else if (_currentState == gameUIState.Pause)
            SwitchUIState(gameUIState.GamePlay);
    }
    public void MainMenuButton()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }
    public void RestartButton()
    {
        gameManager.Restart();
    }
    public void ShowGameOverUI()
    {
        SwitchUIState(gameUIState.GameOver);
    }
    public void ShowGameIsFinishedUI()
    {
        SwitchUIState(gameUIState.GameIsFinished);
    }
}
