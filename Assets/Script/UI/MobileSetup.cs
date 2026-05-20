using UnityEngine;

public class MobileSetup : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}