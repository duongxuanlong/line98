
public enum  GameCommand
{
    NONE,
    //tile
    TILE_STOP_RECEIVE_INPUT,
    TILE_CAN_RECEIVE_INPUT
}
public struct GameEvent
{
    public GameCommand PCommand;
    public SimpleJSON.JSONNode PParams;
}
