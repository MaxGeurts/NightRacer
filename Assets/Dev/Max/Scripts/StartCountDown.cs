using UnityEngine;

public class StartCountDown : MonoBehaviour
{
    private void Start()
    {
        FindObjectOfType<CarController>().MovementOn = true;
        FindAnyObjectByType<GameMngr>().CutsceneIsOver = true;
        FindAnyObjectByType<GameMngr>().timerOn = true;
    }
}
