using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using BottomQueue;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES,
        AUTOPLAY,
        AUTOLOSE
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WON
    }

    public eLevelMode LevelMode { get; private set; }

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

    public BoardController BoardController { get; private set; }

    public WaitingCellQueue WaitingCellQueue { get; private set; }
    
    public UIMainManager UIMenu { get; private set; }

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        UIMenu = FindObjectOfType<UIMainManager>();
        UIMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (BoardController != null) BoardController.Update();
    }
    
    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
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
        LevelMode = mode;
        
        WaitingCellQueue = new GameObject("WaitingCellQueue").AddComponent<WaitingCellQueue>();
        WaitingCellQueue.SetUp(m_gameSettings, this);
        
        BoardController = new GameObject("BoardController").AddComponent<BoardController>();
        
        BoardController.StartGame(this, m_gameSettings, WaitingCellQueue);

        if (mode == eLevelMode.MOVES || mode == eLevelMode.AUTOLOSE)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, UIMenu.GetLevelConditionView(), BoardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, UIMenu.GetLevelConditionView(), this);
        }
        else if (mode == eLevelMode.AUTOPLAY)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.BoardSizeX * m_gameSettings.BoardSizeY + 10, UIMenu.GetLevelConditionView(), BoardController);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;

        State = eStateGame.GAME_STARTED;

        if (LevelMode == eLevelMode.AUTOPLAY)
        {
            StartCoroutine(AutoPlay());
        }
        else if(LevelMode == eLevelMode.AUTOLOSE)
            StartCoroutine(AutoLose());
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        if (BoardController)
        {
            BoardController.Clear();
            Destroy(BoardController.gameObject);
            BoardController = null;
            
            WaitingCellQueue.Clear();
            Destroy(WaitingCellQueue.gameObject);
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (BoardController.IsBusy)
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

    private IEnumerator AutoPlay()
    {
        while (!BoardController.Board.IsEmpty)
        {
            for (int x = 0; x < m_gameSettings.BoardSizeX; x++)
            {
                for (int y = 0; y < m_gameSettings.BoardSizeY; y++)
                {
                    Cell currCell = BoardController.GetCell(x, y);

                    if (currCell.Item != null)
                    {
                        if(WaitingCellQueue.TotalItemsOnQueue == 0)
                        {
                            yield return WaitingCellQueue.OnItemSelected(currCell.Item, 0, 0);
                            currCell.Free();

                            yield return new WaitForEndOfFrame();
                        
                            yield return new WaitForSeconds(0.5f);
                        }
                        else
                        {
                            if (WaitingCellQueue.CountType(currCell.Item) != 0)
                            {
                                yield return WaitingCellQueue.OnItemSelected(currCell.Item, 0, 0);
                                currCell.Free();
                                
                                yield return new WaitForEndOfFrame();
                                
                                yield return new WaitForSeconds(0.5f);
                            }
                        }
                    } else Debug.Log($"{x} - {y} : NULL");
                }
            }
        
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator AutoLose()
    {
        for (int x = 0; x < m_gameSettings.BoardSizeX; x++)
        {
            for (int y = 0; y < m_gameSettings.BoardSizeY; y++)
            {
                if (State == eStateGame.GAME_OVER)
                    break;
                
                Cell currCell = BoardController.GetCell(x, y);

                if (currCell.Item != null)
                {
                    if(WaitingCellQueue.TotalItemsOnQueue == 0)
                    {
                        yield return WaitingCellQueue.OnItemSelected(currCell.Item, 0, 0);
                        currCell.Free();

                        yield return new WaitForEndOfFrame();
                        
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        if (WaitingCellQueue.CountType(currCell.Item) <= 1)
                        {
                            yield return WaitingCellQueue.OnItemSelected(currCell.Item, 0, 0);
                            currCell.Free();
                                
                            yield return new WaitForEndOfFrame();
                                
                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                } else Debug.Log($"{x} - {y} : NULL");
                
                yield return new WaitForEndOfFrame();
            }
        }
        
        yield return new WaitForEndOfFrame();
    }
}
