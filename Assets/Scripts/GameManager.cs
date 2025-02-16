using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class EntityState
{
    public Vector3 position;
    public Quaternion rotation;

    public EntityState(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}

public class GameManager : MonoBehaviour
{
    public GameObject basicCam;
    public GameObject mainCamera;
    public GameObject player;

    public GameObject titleText;
    public GameObject startButton;
    public GameObject exitButton;
    // public GameObject restartButton;
    public GameObject winText;
    public GameObject lostText;

    public List<EnemyFSM> enemies = new List<EnemyFSM>();
    public List<GOAPAgent> goapEnemies = new List<GOAPAgent>();

    public static GameManager Instance { get; private set; }

    private EntityState playerStartState;
    private List<EntityState> enemyStartStates = new List<EntityState>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("Initializing game...");
        EnviromentManager.Instance.Initialize();

        foreach (var enemy in FindObjectsOfType<EnemyFSM>())
        {
            enemies.Add(enemy);
            enemy.DeactivateEnemy();
        }

        foreach(var enemy in FindObjectsOfType<GOAPAgent>())
        {
            goapEnemies.Add(enemy);
            enemy.DeactivateEnemy();
        }

        Debug.Log("Game Initialization Complete.");
    }

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        player.SetActive(true);
        basicCam.SetActive(true);

        playerStartState = new EntityState(player.transform.position, player.transform.rotation);

        titleText.SetActive(false);
        startButton.SetActive(false);
        exitButton.SetActive(false);

        foreach (var enemy in enemies)
        {
            enemy.ActivateEnemy();
            enemyStartStates.Add(new EntityState(enemy.transform.position, enemy.transform.rotation));
        }

        foreach(var enemy in goapEnemies)
        {
            enemy.ActivateEnemy();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");
        player.SetActive(true);
        basicCam.SetActive(true);

        winText.SetActive(false);
        lostText.SetActive(false);
        // restartButton.SetActive(false);
        exitButton.SetActive(false);

        player.transform.position = playerStartState.position;
        player.transform.rotation = playerStartState.rotation;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].transform.position = enemyStartStates[i].position;
            enemies[i].transform.rotation = enemyStartStates[i].rotation;

            enemies[i].ResetFSM();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GameWon()
    {
        Debug.Log("Game won!");
        basicCam.SetActive(false);
        player.SetActive(false);
        foreach (var enemy in enemies)
        {
            enemy.DeactivateEnemy();
        }
        mainCamera.transform.rotation = Quaternion.identity;

        winText.SetActive(true);
        // restartButton.SetActive(true);
        exitButton.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameLost()
    {
        Debug.Log("Game Lost :(");

        basicCam.SetActive(false);
        player.SetActive(false);
        foreach (var enemy in enemies)
        {
            enemy.DeactivateEnemy();
        }
        foreach(var enemy in goapEnemies)
        {
            enemy.DeactivateEnemy();
        }
        mainCamera.transform.rotation = Quaternion.identity;

        lostText.SetActive(true);
        // restartButton.SetActive(true);
        exitButton.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
