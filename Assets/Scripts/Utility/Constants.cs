using System;

public class Constants 
{
    public const string GAME_SETTINGS_PATH = "gamesettings";

    public const string PREFAB_CELL_BACKGROUND = "prefabs/cellBackground";

    public const string PREFAB_NORMAL_TYPE_ONE = "prefabs/itemNormal01";

    public const string PREFAB_NORMAL_TYPE_TWO = "prefabs/itemNormal02";

    public const string PREFAB_NORMAL_TYPE_THREE = "prefabs/itemNormal03";

    public const string PREFAB_NORMAL_TYPE_FOUR = "prefabs/itemNormal04";

    public const string PREFAB_NORMAL_TYPE_FIVE = "prefabs/itemNormal05";

    public const string PREFAB_NORMAL_TYPE_SIX = "prefabs/itemNormal06";

    public const string PREFAB_NORMAL_TYPE_SEVEN = "prefabs/itemNormal07";

    public const string PREFAB_BONUS_HORIZONTAL = "prefabs/itemBonusHorizontal";

    public const string PREFAB_BONUS_VERTICAL = "prefabs/itemBonusVertical";

    public const string PREFAB_BONUS_BOMB = "prefabs/itemBonusBomb";
    
    public const string PREFAB_YELLOW_FISH = "prefabs/yellowFish";
    
    public const string PREFAB_PINK_FISH = "prefabs/pinkFish";
    
    public const string PREFAB_RED_FISH = "prefabs/redFish";
    
    public const string PREFAB_PURPLE_FISH = "prefabs/purpleFish";
    
    public const string PREFAB_GREEN_FISH = "prefabs/greenFish";
    
    public const string PREFAB_BLUE_FISH = "prefabs/blueFish";
    
    public const string PREFAB_SHRIMP = "prefabs/shrimp";
    
    public const string PREFAB_WAITING_CELL = "prefabs/waitingCell";

    public static readonly int NUMBER_OF_NORMAL_ITEM = Enum.GetValues(typeof(NormalItem.eNormalType)).Length;
}
