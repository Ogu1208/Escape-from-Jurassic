using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum enemyState
{
    IDLE,
    ALERT,
    PATROL,
    FOLLOW,
    EXPLORE,
    FURY,
    ATTACK,
    SHIELD,
    DEAD
}

public enum GameState
{
    GAMEPLAY,
    GAMEOVER,
}


public class GameManager : MonoBehaviour
{
    [Header("Game State Manager")]
    public GameState gameState;
    
    
    [Header("Slime AI")]
    public float slimeIdleWaitTime;
    public Transform[] slimeWayPoints;
    public float slimeDistanceToAttack = 2.25f;
    public float slimeAlertTime = 3f;
    public float slimeAttackDelay = 1f;
    public float slimeLookAtSpeed = 1f;
    public float slimeTimeFollowLimit = 2f;

    [Header("Turtle AI")]
    public float turtleIdleWaitTime;
    public Transform[] turtleWayPoints;
    public float turtleDistanceToAttack = 2.25f;
    public float turtleAlertTime = 3f;
    public float turtleAttackDelay = 1f;
    public float turtleLookAtSpeed = 1f;
    public float turtleTimeFollowLimit = 2f;
    public float turtleTimeShield = 3f;


    [Header("UI")]
    public TextMeshProUGUI textGemsUI;


    [Header("Player")]
    public Transform player;
    public int gems;

    [Header("Drop Item")]
    public GameObject gemPrefab;
    public int percDrop = 25;

    public GameObject ClearPopup;
    public GameObject FailPopup;
    public GameObject GameStartPopup;
    public GameObject Portal;


    private void Start() {

        textGemsUI.text = gems.ToString() + " / 8" ;
        Time.timeScale = 0;
    }

    public void GameStart()
    {
        GameStartPopup.SetActive(false);
        Time.timeScale = 1;
    }
    public void ChangeGameState(GameState newState)
    {
        gameState = newState;
        if (gameState == GameState.GAMEOVER)
        {
            FailPopup.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Estudo");
    }
    public void SetGems(int amount)
    {
        gems += amount;
        textGemsUI.text = gems.ToString()+ " / 8" ;

        if (gems == 8)
        {
            Portal.SetActive(true);
            //ClearPopup.SetActive(true);
            //Time.timeScale = 0;
        }
    }
    
    public bool Perc(int p)
    {
        int temp = Random.Range(0,100);
        bool retort = temp <= p ? true : false;
        return retort;
    }
}
