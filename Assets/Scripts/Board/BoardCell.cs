using System;
using UnityEngine;

public class BoardCell : Cell
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public BoardCell NeighbourUp { get; set; }

    public BoardCell NeighbourRight { get; set; }

    public BoardCell NeighbourBottom { get; set; }

    public BoardCell NeighbourLeft { get; set; }

    public void Setup(int cellX, int cellY)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
    }

    public bool IsNeighbour(BoardCell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }
}
