using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public interface ILevelStartObserver
{
    void LevelStart();
}

public interface IWinObserver
{
    void WinScenario();
}

public class GameManager : MonoBehaviour, IWinObserver
{
    public static GameManager Instance;
    private List<IWinObserver> winObservers;
    private List<ILevelStartObserver> levelStartObservers;

    [SerializeField] GameObject startPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] ParticleSystem fireworks0, fireworks1;
    [SerializeField] TextMeshProUGUI scoreTMP;

    private void Awake()
    {
        Instance = this;

        levelStartObservers = new List<ILevelStartObserver>();
        winObservers = new List<IWinObserver>();
    }

    void Start()
    {
        Add_WinObserver(this);

        Globals.score = PlayerPrefs.GetInt("score");
        scoreTMP.text = Globals.score.ToString();
    }

    public void StartButton()
    {
        Notify_LevelStartObservers();
        startPanel.SetActive(false);
    }

    public void NextLevelButton()
    {
        SceneManager.LoadScene(0);
    }

    public void WinScenario()
    {
        StartCoroutine(DelayedWinPanel());
        UpdateScore(1);

        fireworks0.Play();
        fireworks1.Play();
    }

    IEnumerator DelayedWinPanel()
    {
        yield return new WaitForSeconds(1);

        winPanel.SetActive(true);
    }

    public void UpdateScore(int value)
    {
        Globals.score += value;
        PlayerPrefs.SetInt("score", Globals.score);
        scoreTMP.text = Globals.score.ToString();
    }



    #region Observer Functions

    #region Level Start Observer
    public void Add_LevelStartObserver(ILevelStartObserver observer)
    {
        levelStartObservers.Add(observer);
    }
    public void Remove_LevelStartObserver(ILevelStartObserver observer)
    {
        levelStartObservers.Remove(observer);
    }
    public void Notify_LevelStartObservers()
    {
        foreach (ILevelStartObserver observer in levelStartObservers.ToArray())
        {
            if (levelStartObservers.Contains(observer))
                observer.LevelStart();
        }
    }
    #endregion

    #region Win Observer
    public void Add_WinObserver(IWinObserver observer)
    {
        winObservers.Add(observer);
    }
    public void Remove_WinObserver(IWinObserver observer)
    {
        winObservers.Remove(observer);
    }
    public void Notify_WinObservers()
    {
        foreach (IWinObserver observer in winObservers.ToArray())
        {
            if (winObservers.Contains(observer))
                observer.WinScenario();
        }
    }
    #endregion

    #endregion
}