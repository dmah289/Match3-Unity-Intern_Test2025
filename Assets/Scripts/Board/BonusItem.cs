using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusItem : Item
{
    public enum eBonusType
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    public eBonusType ItemType;

    public void SetType(eBonusType type)
    {
        ItemType = type;
    }

    protected override string GetPrefabName()
    {
        string prefabname = string.Empty;
        switch (ItemType)
        {
            case eBonusType.NONE:
                break;
            case eBonusType.HORIZONTAL:
                prefabname = Constants.PREFAB_BONUS_HORIZONTAL;
                break;
            case eBonusType.VERTICAL:
                prefabname = Constants.PREFAB_BONUS_VERTICAL;
                break;
            case eBonusType.ALL:
                prefabname = Constants.PREFAB_BONUS_BOMB;
                break;
        }

        return prefabname;
    }

    internal override bool IsSameType(Item other)
    {
        BonusItem it = other as BonusItem;

        return it != null && it.ItemType == this.ItemType;
    }

    internal override void ExplodeView()
    {
        ActivateBonus();

        base.ExplodeView();
    }

    private void ActivateBonus()
    {
        switch (ItemType)
        {
            case eBonusType.HORIZONTAL:
                ExplodeHorizontalLine();
                break;
            case eBonusType.VERTICAL:
                ExplodeVerticalLine();
                break;
            case eBonusType.ALL:
                ExplodeBomb();
                break;

        }
    }

    private void ExplodeBomb()
    {
        BoardCell cell = Cell as BoardCell;
        
        List<BoardCell> list = new List<BoardCell>();
        if (cell.NeighbourBottom) list.Add(cell.NeighbourBottom);
        if (cell.NeighbourUp) list.Add(cell.NeighbourUp);
        if (cell.NeighbourLeft)
        {
            list.Add(cell.NeighbourLeft);
            if (cell.NeighbourLeft.NeighbourUp)
            {
                list.Add(cell.NeighbourLeft.NeighbourUp);
            }
            if (cell.NeighbourLeft.NeighbourBottom)
            {
                list.Add(cell.NeighbourLeft.NeighbourBottom);
            }
        }
        if (cell.NeighbourRight)
        {
            list.Add(cell.NeighbourRight);
            if (cell.NeighbourRight.NeighbourUp)
            {
                list.Add(cell.NeighbourRight.NeighbourUp);
            }
            if (cell.NeighbourRight.NeighbourBottom)
            {
                list.Add(cell.NeighbourRight.NeighbourBottom);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            list[i].ExplodeItem();
        }
    }

    private void ExplodeVerticalLine()
    {
        List<BoardCell> list = new List<BoardCell>();

        BoardCell newcell = Cell as BoardCell;
        while (true)
        {
            BoardCell next = newcell.NeighbourUp;
            if (next == null) break;

            list.Add(next);
            newcell = next;
        }

        newcell = Cell as BoardCell;
        while (true)
        {
            BoardCell next = newcell.NeighbourBottom;
            if (next == null) break;

            list.Add(next);
            newcell = next;
        }


        for (int i = 0; i < list.Count; i++)
        {
            list[i].ExplodeItem();
        }
    }

    private void ExplodeHorizontalLine()
    {
        List<BoardCell> list = new List<BoardCell>();

        BoardCell newcell = Cell as BoardCell;
        while (true)
        {
            BoardCell next = newcell.NeighbourRight;
            if (next == null) break;

            list.Add(next);
            newcell = next;
        }

        newcell = Cell as BoardCell;
        while (true)
        {
            BoardCell next = newcell.NeighbourLeft;
            if (next == null) break;

            list.Add(next);
            newcell = next;
        }


        for (int i = 0; i < list.Count; i++)
        {
            list[i].ExplodeItem();
        }

    }
}
