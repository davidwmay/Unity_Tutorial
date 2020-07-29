using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public GameObject[] hazards;
    public Vector3 spawnValues;
    public int hazardCount;
    public float spawnWait;
    public int startWait;
    public float waveWait;

    public Text scoreText;
    public Text restartText;
    public Text gameOverText;

    private int score;
    private bool gameOver;
    private bool restart;
    private bool rollHighScore;

    public GameObject highscoreTemplate;
    public GameObject enterNameUI;
    public GameObject enterNameButton;
    public TMP_InputField enterNameText;
    public Transform templateContainer;
    public Transform template;
    private List<Highscore> highscoreList;
    private List<Transform> highscoreTransformList;

    
    void Start()
    {
        gameOver = false;
        restart = false;
        rollHighScore = false;
        restartText.text = "";
        gameOverText.text = "";
        score = 0;
        UpdateScore();
        highscoreTemplate.SetActive(false);
        enterNameUI.SetActive(false);
        enterNameButton.SetActive(true);
        StartCoroutine(SpawnWaves());
    }

    void Update()
    {  
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                rollHighScore = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        // Start high score movement
        if (rollHighScore)
        {
            highscoreTemplate.transform.Translate(Vector3.up * Time.deltaTime * 22);
            gameOverText.text = "";
        }
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        while(true)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                GameObject hazard = hazards[Random.Range(0, hazards.Length)];
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);
                yield return new WaitForSeconds(spawnWait);
            }
            yield return new WaitForSeconds(waveWait);

            if (gameOver)
            {
                restartText.text = "Press 'R' to Restart";
                restart = true;
                break;
            }
        }
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text =  "Score: " + score;
    }

    public void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOver = true;

        enterNameUI.SetActive(true);

        Button uiOkButton = enterNameButton.GetComponent<Button>();
        uiOkButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() 
    {
        enterNameUI.SetActive(false);

        if (PlayerPrefs.HasKey("highscoreTable"))
        {
            AddHighScore(score, enterNameText.text);
            MakeHighScoreTable();
        } 
        else // highscoreTable hasn't been created yet 
        {
            MakeHighScoreTable();
        }

        highscoreTemplate.SetActive(true);
        rollHighScore = true;
    }

    void MakeHighScoreTable() 
    {
        Highscores highscores;
        string jsonString;
        if (PlayerPrefs.HasKey("highscoreTable"))
        {
            jsonString = PlayerPrefs.GetString("highscoreTable");
            highscores = JsonUtility.FromJson<Highscores>(jsonString);

            for (int i = 0; i < highscores.highscoreList.Count; i++) 
            {
                for (int j = i + 1; j < highscores.highscoreList.Count; j++)
                {
                    if (highscores.highscoreList[j].score > highscores.highscoreList[i].score)
                    {
                        Highscore swap = highscores.highscoreList[i];
                        highscores.highscoreList[i] = highscores.highscoreList[j];
                        highscores.highscoreList[j] = swap;
                    }
                }
            }
        } 
        else 
        {
            // Manually create list if it isn't stored in PlayerPrefs yet
            highscoreList = new List<Highscore>() 
            {
                new Highscore { score = score, name = enterNameText.text }
            };

            highscores = new Highscores { highscoreList = highscoreList };
            string json = JsonUtility.ToJson(highscores);
            
            PlayerPrefs.SetString("highscoreTable", json);
            PlayerPrefs.Save();
            
            jsonString = PlayerPrefs.GetString("highscoreTable");
            highscores = JsonUtility.FromJson<Highscores>(jsonString);
        }

        highscoreTransformList = new List<Transform>();
        
        foreach (Highscore highscore in highscores.highscoreList) 
        {
            AddHighScoreTransform(highscore, templateContainer, highscoreTransformList);
        }    
    }

    private void AddHighScoreTransform(Highscore highscoreEntry, Transform scoreContainer, List<Transform> transformList)
    {
        Transform newRow = Instantiate(template, templateContainer);
        newRow.localPosition = new Vector2 (0, -45 * transformList.Count);
        newRow.Find("HighscorePlaceTemplate").GetComponent<Text>().text = (transformList.Count + 1).ToString();
        newRow.Find("HighscoreNameTemplate").GetComponent<Text>().text = (highscoreEntry.name);
        newRow.Find("HighscoreScoreTemplate").GetComponent<Text>().text = (highscoreEntry.score).ToString();
        transformList.Add(newRow);
    }

    private void AddHighScore(int score, string name) 
    {
        Highscore highscore = new Highscore { score = score, name = name };

        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);
        highscores.highscoreList.Add(highscore);
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class Highscores 
    {
        public List<Highscore> highscoreList;
    }

    [System.Serializable]
    private class Highscore 
    {
        public string name;
        public int score;
    }
}
