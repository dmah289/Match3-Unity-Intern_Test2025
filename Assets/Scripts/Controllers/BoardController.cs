using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BottomQueue;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    public Board Board { get; private set; }

    private GameManager m_gameManager;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<BoardCell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    public bool IsGameOver { get; private set; }

    private bool task1_Completed;
    
    private WaitingCellQueue m_waitingCellQueue;

    public void StartGame(GameManager gameManager, GameSettings gameSettings, WaitingCellQueue waitingCellQueue)
    {
        m_gameManager = gameManager;
        
        m_waitingCellQueue = waitingCellQueue;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        Board = new Board(this.transform, gameSettings);

        Fill();
    }
    
    private void Fill()
    {
        // m_board.Fill();
        Board.FillDivisableBy3();
        // FindMatchesAndCollapse();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                IsGameOver = true;
                StopHints();
                break;
        }
    }

    public void Update()
    {
        if (!task1_Completed && Input.GetKeyDown(KeyCode.Space))
        {
            task1_Completed = true;
            Board.Task1();
        }
            
        
        if (IsGameOver) return;
        if (IsBusy) return;
        if (m_gameManager.State != GameManager.eStateGame.GAME_STARTED)
            return;

        if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                // ShowHint();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                m_isDragging = true;
                m_hitCollider = hit.collider;
            }
        }

        if (Input.GetMouseButtonUp(0) && m_hitCollider)
        {
            if(m_gameManager.LevelMode == GameManager.eLevelMode.MOVES || m_gameManager.LevelMode == GameManager.eLevelMode.TIMER)
            {
                if (m_hitCollider.TryGetComponent(out BoardCell currCell))
                {
                    StartCoroutine(m_waitingCellQueue.OnItemSelected(currCell.Item, currCell.BoardX, currCell.BoardY));
                    currCell.Free();
                }
            }
            ResetRayCast();
        }
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    private void FindMatchesAndCollapse(BoardCell cell1, BoardCell cell2)
    {
        if (cell1.Item is BonusItem)
        {
            cell1.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else if (cell2.Item is BonusItem)
        {
            cell2.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else
        {
            List<BoardCell> cells1 = GetMatches(cell1);
            List<BoardCell> cells2 = GetMatches(cell2);

            List<BoardCell> matches = new List<BoardCell>();
            matches.AddRange(cells1);
            matches.AddRange(cells2);
            matches = matches.Distinct().ToList();

            if (matches.Count < m_gameSettings.MatchesMin)
            {
                Board.Swap(cell1, cell2, () =>
                {
                    IsBusy = false;
                });
            }
            else
            {
                OnMoveEvent();

                CollapseMatches(matches, cell2);
            }
        }
    }

    private void FindMatchesAndCollapse()
    {
        List<BoardCell> matches = Board.FindFirstMatch();

        if (matches.Count > 0)
        {
            CollapseMatches(matches, null);
        }
        else
        {
            m_potentialMatch = Board.GetPotentialMatches();
            if (m_potentialMatch.Count > 0)
            {
                IsBusy = false;

                m_timeAfterFill = 0f;
            }
            else
            {
                //StartCoroutine(RefillBoardCoroutine());
                StartCoroutine(ShuffleBoardCoroutine());
            }
        }
    }

    private List<BoardCell> GetMatches(BoardCell boardCell)
    {
        List<BoardCell> listHor = Board.GetHorizontalMatches(boardCell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<BoardCell> listVert = Board.GetVerticalMatches(boardCell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<BoardCell> matches, BoardCell boardCellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            Board.ConvertNormalToBonus(matches, boardCellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        Board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        Board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        Board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        // m_board.Fill();
        Board.FillDivisableBy3();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        Board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        FindMatchesAndCollapse();
    }


    private void SetSortingLayer(BoardCell cell1, BoardCell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(BoardCell cell1, BoardCell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        Board.Clear();
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        
        if (m_potentialMatch != null && m_potentialMatch.Count > 0)
        {
            foreach (var cell in m_potentialMatch)
            {
                cell.StopHintAnimation();
            }

            m_potentialMatch.Clear();
        }
    }

    public void CheckWin()
    {
        Board.OnItemSelected();

        if (Board.RemainingCells == 0)
        {
            m_gameManager.SetState(GameManager.eStateGame.GAME_WON);
        }
    }

    public Cell GetCell(int x, int y)
    {
        return Board.Cells[x, y];
    }
}
