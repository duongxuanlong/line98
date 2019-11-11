public class Constant
{
    #region unity unit
    public const float UNIY_HALF_WIDTH = 10f;
    public static float UNITY_HALF_HEIGHT = 0F;
    public const float TILE_UNIT = 2f;
    #endregion

    #region board params
    public const int BOARD_COLUMN = 9;
    public const int BOARD_ROW = 9;

    public const int NUMBER_BALLS = 100;
    public static float BOARD_START_X = Constant.TILE_UNIT - UNIY_HALF_WIDTH;
    public static float BOARD_START_Y = 0;
    #endregion

    #region layer name
    public const string LAYER_BACKGROUND = "BACKGROUND";
    public const string LAYER_TILE = "TITLE";
    public const string LAYER_BALL = "BALL";
    #endregion 
}
