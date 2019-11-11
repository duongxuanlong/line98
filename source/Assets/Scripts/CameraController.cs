using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static Camera SInstance;
    private void Awake() {
        if (SInstance == null)
            SInstance = Camera.main;

        float width = Screen.width;
        float height = Screen.height;
        Constant.UNITY_HALF_HEIGHT = Constant.UNIY_HALF_WIDTH * height / width;
        SInstance.orthographicSize = Constant.UNITY_HALF_HEIGHT;

        Constant.BOARD_START_Y = -Constant.UNITY_HALF_HEIGHT + ((Constant.TILE_UNIT * Constant.BOARD_ROW) + Constant.TILE_UNIT);
        Debug.Log("Long Board start y: " + Constant.BOARD_START_Y);
    }
}
