using UnityEngine;

public class Cell : MonoBehaviour
{
    public Item Item { get; set; }
    
    public bool IsEmpty => Item == null;
    
    public void Free()
    {
        Item = null;
    }
    
    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }
    
    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }
    
    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }
    
    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }
    
    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
    
    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }
}