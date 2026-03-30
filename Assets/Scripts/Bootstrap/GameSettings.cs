using UnityEngine;

public class GameSettings : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        Application.targetFrameRate = -1; // uncapped
    }
}