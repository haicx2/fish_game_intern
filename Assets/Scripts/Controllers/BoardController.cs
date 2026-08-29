using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;
    private GameManager m_gameManager;
    private Camera m_cam;
    private GameSettings m_gameSettings;
    private List<Cell> m_potentialMatch = new List<Cell>();
    private float m_timeAfterFill;
    private bool m_hintIsShown;
    private bool m_gameOver;
    private bool m_canReturnItems;

    public Transform[] TraySlots;
    private List<Item> m_trayItems = new List<Item>();

    public void StartGame(GameManager gameManager, GameSettings gameSettings, bool canReturnItems)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;

        m_canReturnItems = canReturnItems;

        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;

        TraySlots = new Transform[5];
        GameObject tray = GameObject.Find("BottomTray");
        if (tray != null)
        {
            for (int i = 0; i < 5; i++)
            {
                TraySlots[i] = tray.transform.GetChild(i);
            }
        }

        m_board = new Board(this.transform, gameSettings);
        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
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
                m_gameOver = true;
                StopHints();
                break;
        }
    }

    public void Tick()
    {
        if (m_gameOver) return;
        if (IsBusy) return;
        if (m_gameManager.IsGameOver) return;

        if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                ShowHint();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell clickedCell = hit.collider.GetComponent<Cell>();
                if (clickedCell != null && clickedCell.Item != null)
                {
                    StopHints();
                    MoveItemToBottomTray(clickedCell);
                }
                else
                {
                    Item clickedTrayItem = m_trayItems.FirstOrDefault(i => i.View == hit.collider.transform);
                    if (clickedTrayItem != null)
                    {
                        ReturnItemToBoard(clickedTrayItem);
                    }
                }
            }
        }
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
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }
        m_potentialMatch.Clear();
    }

    private void CheckTrayForMatches()
    {
        List<Item> matchOfThree = new List<Item>();

        for (int i = 0; i < m_trayItems.Count; i++)
        {
            Item currentItem = m_trayItems[i];
            matchOfThree.Clear();

            for (int j = 0; j < m_trayItems.Count; j++)
            {
                if (m_trayItems[j].IsSameType(currentItem))
                {
                    matchOfThree.Add(m_trayItems[j]);
                }
            }

            if (matchOfThree.Count == 3)
            {
                break;
            }
        }

        if (matchOfThree.Count == 3)
        {
            foreach (Item matchedItem in matchOfThree)
            {
                m_trayItems.Remove(matchedItem);
                matchedItem.ExplodeView();
            }
            SlideTrayItemsLeft();
        }
    }

    private void SlideTrayItemsLeft()
    {
        for (int i = 0; i < m_trayItems.Count; i++)
        {
            Transform targetSlot = TraySlots[i];
            m_trayItems[i].View.DOMove(targetSlot.position, 0.2f);
        }
    }

    private void CheckWinCondition()
    {
        bool isBoardEmpty = m_board.CheckIfAllCellsAreEmpty();

        if (isBoardEmpty)
        {
            m_gameOver = true;
            m_gameManager.ShowWinScreen();
        }
    }

    private void MoveItemToBottomTray(Cell clickedCell)
    {
        if (m_trayItems.Count >= 5) return;

        Item itemToMove = clickedCell.DetachItem();
        if (itemToMove == null) return;
        OnMoveEvent();
        m_trayItems.Add(itemToMove);

        int slotIndex = m_trayItems.Count - 1;
        Transform targetSlot = TraySlots[slotIndex];

        itemToMove.View.DOMove(targetSlot.position, 0.3f);
        itemToMove.View.DOScale(0.8f, 0.3f);

        CheckTrayForMatches();
        CheckWinCondition();
    }

    private void ReturnItemToBoard(Item item)
    {
        if (!m_canReturnItems) return;

        m_trayItems.Remove(item);
        SlideTrayItemsLeft();

        Cell originalCell = item.OriginalCell;
        if (originalCell != null && originalCell.IsEmpty)
        {
            originalCell.Assign(item);
            item.View.DOMove(originalCell.transform.position, 0.3f);
            item.View.DOScale(1f, 0.3f);
        }
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    public void StartAutoplay(bool winIntent)
    {
        StartCoroutine(AutoplayRoutine(winIntent));
    }

    private IEnumerator AutoplayRoutine(bool winIntent)
    {
        while (!m_gameOver)
        {
            yield return new WaitForSeconds(0.5f);

            Cell targetCell = winIntent ? GetCellForWin() : GetCellForLose();
            if (targetCell != null)
            {
                MoveItemToBottomTray(targetCell);
            }
        }
    }

    private Cell GetCellForWin()
    {
        var cells = m_board.GetAllCells().Where(c => !c.IsEmpty).ToList();

        foreach (var trayItem in m_trayItems)
        {
            var match = cells.FirstOrDefault(c => c.Item.IsSameType(trayItem));
            if (match != null) return match;
        }
        return cells.FirstOrDefault();
    }

    private Cell GetCellForLose()
    {
        var cells = m_board.GetAllCells().Where(c => !c.IsEmpty).ToList();

        var badMatch = cells.FirstOrDefault(c => !m_trayItems.Any(t => t.IsSameType(c.Item)));
        return badMatch != null ? badMatch : cells.FirstOrDefault();
    }
}