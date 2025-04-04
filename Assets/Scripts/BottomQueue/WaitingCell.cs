using System.Collections;
using UnityEngine;

namespace BottomQueue
{
    public class WaitingCell : Cell
    {
        private WaitingCellQueue waitingCellQueue;
        
        public int Index { get; private set; }
        
        public void Setup(int index, WaitingCellQueue waitingCellQueue)
        {
            Index = index;
            this.waitingCellQueue = waitingCellQueue;
        }

        private void OnMouseDown()
        {
            if (waitingCellQueue.GameManager.LevelMode == GameManager.eLevelMode.TIMER && !waitingCellQueue.IsBusy)
            {
                BoardCell boardCell = waitingCellQueue.GameManager.BoardController.GetCell(Item.LastPosition.x, Item.LastPosition.y) as BoardCell;
            
                StartCoroutine(ReturnToBoard(boardCell));
            
                waitingCellQueue.Queue[Index].Free();
            }
        }

        private IEnumerator ReturnToBoard(BoardCell boardCell)
        {
            while (waitingCellQueue.IsBusy)
                yield return null;

            if (Item != null)
            {
                waitingCellQueue.IsBusy = true;
            
                Item.SetViewRoot(waitingCellQueue.GameManager.BoardController.Board.Root);
            
                boardCell.Assign(Item);
                boardCell.ApplyItemMoveToPosition();
            
                yield return new WaitForSeconds(0.2f);

                for (int i = Index; i < waitingCellQueue.TotalItemsOnQueue - 1; i++)
                {
                    waitingCellQueue.Queue[i].Assign(waitingCellQueue.Queue[i+1].Item);
                    waitingCellQueue.Queue[i].ApplyItemMoveToPosition();
                    
                    yield return new WaitForSeconds(0.2f);
                }

                waitingCellQueue.TotalItemsOnQueue--;
            
                waitingCellQueue.IsBusy = false;
            }
        }
    }
}