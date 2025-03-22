public class FishItem : Item
{
    public enum eFishType
    {
        YELLOW,
        PINK,
        RED,
        PURPLE,
        GREEN,
        BLUE,
        SHRIMP
    } 

    public eFishType ItemType;

    public void SetType(eFishType type)
    {
        ItemType = type;
    }
    
    protected override string GetPrefabName()
    {
        string prefabname = string.Empty;
        switch (ItemType)
        {
            case eFishType.YELLOW:
                prefabname = Constants.PREFAB_YELLOW_FISH;
                break;
            case eFishType.PINK:
                prefabname = Constants.PREFAB_PINK_FISH;
                break;
            case eFishType.RED:
                prefabname = Constants.PREFAB_RED_FISH;
                break;
            case eFishType.PURPLE:
                prefabname = Constants.PREFAB_PURPLE_FISH;
                break;
            case eFishType.GREEN:
                prefabname = Constants.PREFAB_GREEN_FISH;
                break;
            case eFishType.BLUE:
                prefabname = Constants.PREFAB_BLUE_FISH;
                break;
            case eFishType.SHRIMP:
                prefabname = Constants.PREFAB_SHRIMP;
                break;
        }
    
        return prefabname;
    }
}