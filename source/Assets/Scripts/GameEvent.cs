
public enum  GameCommand
{
    NONE,
    //BOARD
    BOARD_START,
    BOARD_STOP_RECEIVE_INPUT,
    BOARD_CAN_RECEIVE_INPUT,
    BOARD_END,
    //tile
    TILE_START,
    TILE_SELECT,
    TILE_FINISH_MOVE_TO_TARGET,
    TILE_END
}
public struct GameEvent
{
    public GameCommand PCommand;
    public SimpleJSON.JSONNode PParams;
}
