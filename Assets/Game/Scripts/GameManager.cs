using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GameUIManager gameUIManager;
    public Character playerCharacter;

    private bool _gameIsOver;

    private void Awake()
    {
        playerCharacter = GameObject.FindWithTag("Player").GetComponent<Character>();
    }
    private void GameOver()
    {
        gameUIManager.ShowGameOverUI();
    }
    public void GameIsFinished()
    {
        gameUIManager.ShowGameIsFinishedUI();
    }
    private void Update()
    {
        if (_gameIsOver)
            return;
        if(Input.GetKeyUp(KeyCode.Escape))
            gameUIManager.TogglePauseUI();
        if(playerCharacter.currentState == Character.characterState.Dead) 
        {
            _gameIsOver = true;
            GameOver();
        }
    }
    public void ReturnToTheMainMenu()
    {
        gameUIManager.MainMenuButton();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
