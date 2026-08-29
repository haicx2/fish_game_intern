using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsWin => showWinScreen;
    public bool IsGameOver => showWinScreen || showLoseScreen;
    public event Action<eStateGame> StateChangedAction = delegate { };

    private bool showWinScreen = false;
    private bool showLoseScreen = false;

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;
            StateChangedAction(m_state);
        }
    }

    private GameSettings m_gameSettings;
    private BoardController m_boardController;
    private UIMainManager m_uiMenu;
    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;
        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);
        m_uiMenu = FindObjectOfType<UIMainManager>();
        if (m_uiMenu != null) m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    void Update()
    {
        if (m_levelCondition != null) m_levelCondition.Tick();
        if (m_boardController != null) m_boardController.Tick();
    }

    internal void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        showWinScreen = false;
        showLoseScreen = false;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();

        bool allowReturningItems = (mode == eLevelMode.TIMER);
        m_boardController.StartGame(this, m_gameSettings, allowReturningItems);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            if (m_uiMenu != null) m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController, this);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            if (m_uiMenu != null) m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }

        if (m_levelCondition != null) m_levelCondition.ConditionCompleteEvent += GameOver;

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        if (State == eStateGame.GAME_OVER) return;
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController != null && m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);
        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    public void ShowWinScreen()
    {
        if (showLoseScreen) return;
        showWinScreen = true;
        GameOver();
    }

    public void ShowLoseScreen()
    {
        if (showWinScreen) return;
        showLoseScreen = true;
        GameOver();
    }

    private void OnGUI()
    {
        if (State == eStateGame.MAIN_MENU)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 60, 200, 50), "Play Normal"))
            {
                LoadLevel(eLevelMode.MOVES);
            }
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), "Time Attack"))
            {
                LoadLevel(eLevelMode.TIMER);
            }
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 60, 200, 50), "Autoplay (WIN)"))
            {
                LoadLevel(eLevelMode.MOVES);
                m_boardController.StartAutoplay(true);
            }
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 120, 200, 50), "Autoplay (LOSE)"))
            {
                LoadLevel(eLevelMode.MOVES);
                m_boardController.StartAutoplay(false);
            }
        }
    }
}