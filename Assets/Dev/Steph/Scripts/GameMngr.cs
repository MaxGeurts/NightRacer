using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;


public class GameMngr : MonoBehaviour
{
    private CarController carControlScript;
    private CameraFollow camFollowScript;
    private GameObject car;

    [SerializeField] private GameObject[] timeLines;
    [SerializeField] private Animator endTransition;

    [SerializeField] private float raceTimer;
    [SerializeField] private List<string> raceTimerTracker = new();
    [SerializeField] private List<TextMeshProUGUI> timers;
    [SerializeField] private List<TextMeshProUGUI> lapTexts;

    [SerializeField] private GameObject[] menus;

    [SerializeField] private GameObject LBSpotPrefab;
    [SerializeField] private GameObject LBSpotEmptyPrefab;
    [SerializeField] private GameObject contentTop10;
    [SerializeField] private GameObject instantiatedLBObject;
    private List<GameObject> instantiatedLBSpots = new();
    private List<GameObject> sortedLBSpots = new();
    private List<float> instantiatedLBSpotsNames = new();
    private List<GameObject> emptyTop10Spots = new();

    [SerializeField] private TextMeshProUGUI[] top3Texts;
    [SerializeField] private TextMeshProUGUI[] lapsReviewTexts;


    public int curCheckpoint = 0;
    public int CurrentLap;
    public bool timerOn = false;
    public bool CutsceneIsOver;
    private bool isGamePaused;

    private int min;
    private int sec;
    private float ms;

    private Vector3 carStartPos;
    private Quaternion carStartRot;

    private void Start()
    {
        carControlScript = FindObjectOfType<CarController>();
        camFollowScript = Camera.main.GetComponent<CameraFollow>();
        camFollowScript.enabled = false;

        car = GameObject.Find("Car 1 1");
        carControlScript.lastReachedCheckpointPos = car.transform.position;
        carControlScript.lastReachedCheckpointRot = car.transform.rotation;
        carStartPos = car.transform.position;
        carStartRot = car.transform.rotation;

        timeLines[0].SetActive(false);
        timeLines[1].SetActive(false);
        menus[0].SetActive(false);
        StartCoroutine(WaitTilLoadDone());

        MakeTop10();

        timers[5].text = PlayerPrefs.GetString(PlayerPrefs.GetString("SelectedFile") + "Timer" + 4);


        RestartGame();
    }

    void Update()
    {
        if (timerOn)
        {
            raceTimer += Time.deltaTime;

            min = Mathf.FloorToInt(raceTimer / 60);
            sec = Mathf.FloorToInt(raceTimer % 60);
            ms = (raceTimer % 1) * 100;

            timers[4].text = string.Format("{0:0}:{1:00}:{2:00}", min, sec, ms);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && CurrentLap < 4)
        {
            PauseGame();
        }
    }

    public async void MakeTop10()
    {
        if (instantiatedLBSpots.Count > 0 || sortedLBSpots.Count > 0 || instantiatedLBObject.transform.childCount > 0)
        {
            for (int i = 0; i < instantiatedLBObject.transform.childCount; i++)
            {
                Destroy(instantiatedLBObject.transform.GetChild(i).gameObject);
            }
            
            foreach (GameObject sortedSpot in sortedLBSpots)
            {
                Destroy(sortedSpot);
            }
            foreach (GameObject emptySpot in emptyTop10Spots)
            {
                Destroy(emptySpot);
            }


            instantiatedLBSpots.Clear();
            sortedLBSpots.Clear();
            emptyTop10Spots.Clear();
            instantiatedLBSpotsNames.Clear();
            await Task.Delay(1);
        }

        for (int i = 0; i < PlayerPrefs.GetInt("AmountFiles"); i++)
        {
            if (PlayerPrefs.HasKey(PlayerPrefs.GetString("Name" + i) + "Timer2"))
            {
                GameObject newTop10Spot = Instantiate(LBSpotPrefab, instantiatedLBObject.transform);
                FillInLBPrefab(newTop10Spot, i);
                await Task.Delay(1);
            }
        }

        CheckTop10Times();
    }

    private void FillInLBPrefab(GameObject instantiatedLB, int index)
    {
        LBPlace script = instantiatedLB.GetComponent<LBPlace>();
        script.FileName = PlayerPrefs.GetString("Name" + index);
        script.FileTime = PlayerPrefs.GetString(script.FileName + "Timer3");
        script.FileDeltaTime = PlayerPrefs.GetFloat(script.FileName + "TimerDelta").ToString();
        instantiatedLB.name = PlayerPrefs.GetFloat(script.FileName + "TimerDelta").ToString();

        instantiatedLBSpots.Add(instantiatedLB);
        instantiatedLBSpotsNames.Add(float.Parse(instantiatedLB.name));
    }

    private void CheckTop10Times()
    {
        float[] sortedNames = instantiatedLBSpotsNames.ToArray();
        Array.Sort(sortedNames);

        for (int i = 0; i < sortedNames.Length; i++)
        {
            for (int x = 0; x < instantiatedLBSpots.Count; x++)
            {
                if (instantiatedLBSpots[x].name == sortedNames[i].ToString())
                {
                    sortedLBSpots.Add(instantiatedLBSpots[x]);
                    break;
                }
            }
        }

        {

        }
        for (int i = 0; i < sortedLBSpots.Count; i++)
        {
            if (i <= 10)
            {
                sortedLBSpots[i].GetComponent<LBPlace>().UpdateUIText(i, contentTop10);
            }
        }

        for (int i = 0; i < 10 - sortedLBSpots.Count; i++)
        {
            GameObject emptySpot = Instantiate(LBSpotEmptyPrefab, contentTop10.transform);
            emptyTop10Spots.Add(emptySpot);
        }

        UpdateFinishScreenStats();
    }

    private IEnumerator WaitTilLoadDone()
    {
        yield return new WaitForSeconds(3.6f);
        timeLines[0].SetActive(true);
        timeLines[1].SetActive(true);
        yield return new WaitForSeconds(1f);
        camFollowScript.enabled = true;
    }

    public void RestartGame()
    {
        raceTimerTracker.Clear();
        Camera.main.GetComponent<CameraFollow>().canCamMove = false;

        if (curCheckpoint < 4)
        {
            car.transform.SetPositionAndRotation(carStartPos, carStartRot);
        }

        foreach (TextMeshProUGUI text in timers)
        {
            text.text = "";
        }

        if (PlayerPrefs.HasKey("SelectedFile"))
        {
            timers[5].text = PlayerPrefs.GetString(PlayerPrefs.GetString("SelectedFile") + "Timer3");
        }

        raceTimer = 0;
        carControlScript.lastReachedCheckpointPos = car.transform.position;
        carControlScript.lastReachedCheckpointRot = car.transform.rotation;

        if (CutsceneIsOver)
        {
            curCheckpoint = 0;
            timerOn = true;
        }

        Camera.main.GetComponent<CameraFollow>().canCamMove = true;
        Camera.main.GetComponent<CameraFollow>().LookAtCar = false;
    }

    public void ChangeScreen(string whichScreenOn)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].name == whichScreenOn)
            {
                foreach (GameObject screen in menus)
                {
                    screen.SetActive(false);
                }
                menus[i].SetActive(true);
            }
        }
    }

    public void ReturnToMainMenu()
    {
        endTransition.enabled = true;
        StartCoroutine(ChangeScene(0, "MainMenuScene"));
    }

    private IEnumerator ChangeScene(int sceneIndex, string sceneName)
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(sceneIndex);
        Invoke(sceneName, 1f);
    }

    public void PauseGame()
    {
        if (CutsceneIsOver && CurrentLap < 4)
        {
            isGamePaused = !isGamePaused;
            if (curCheckpoint != 4)
            {
                timerOn = !isGamePaused;
            }

            carControlScript.FreezeCarMovement(isGamePaused);
            camFollowScript.canCamMove = !isGamePaused;
            menus[0].SetActive(!isGamePaused); //play menu
            menus[1].SetActive(isGamePaused); //pause menu
            if (!isGamePaused) { menus[2].SetActive(isGamePaused); } //controls menu
            
            foreach (SnowBall snowBall in FindObjectsOfType<SnowBall>())
            {
                snowBall.gameObject.GetComponent<Animator>().enabled = !isGamePaused;
                snowBall.gameObject.GetComponent<Rigidbody>().isKinematic = !isGamePaused;
            }
        }
    }

    public void RestartScene()
    {
        endTransition.enabled = true;
        StartCoroutine(ChangeScene(1, "Main scene"));
    }
    public void FinishLaps()
    {
        carControlScript.FreezeCarMovement(true);
        camFollowScript.canCamMove = false;
        ChangeScreen("Finish Laps");
    }


    public void SaveSegmentTime()
    {
        raceTimerTracker.Add(timers[4].text);
        timers[curCheckpoint].text = timers[4].text;

        if (curCheckpoint == 3)
        {
            curCheckpoint = 4;
        }

        if (CurrentLap == 3)
        {
            lapTexts[1].text = "Final";
        }

        if (CurrentLap < 4)
        {
            lapTexts[0].text = CurrentLap.ToString() + "/3";
        }

        if (PlayerPrefs.HasKey("SelectedFile") && curCheckpoint == 4)
        {
            if (PlayerPrefs.HasKey(PlayerPrefs.GetString("SelectedFile") + "Timer1") && PlayerPrefs.HasKey(PlayerPrefs.GetString("SelectedFile") + "TimerDelta"))
            {
                if (PlayerPrefs.GetFloat(PlayerPrefs.GetString("SelectedFile") + "TimerDelta") > raceTimer)
                {
                    RewriteTimes();
                }
            }
            else
            {
                RewriteTimes();
            }
        }
    }

    private void RewriteTimes()
    {
        for (int i = 0; i < raceTimerTracker.Count; i++)
        {
            PlayerPrefs.SetString(PlayerPrefs.GetString("SelectedFile") + "Timer" + i.ToString(), raceTimerTracker[i]);
        }
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("SelectedFile") + "TimerDelta", raceTimer);
    }

    public void SaveLapTime()
    {
        PlayerPrefs.SetString(PlayerPrefs.GetString("SelectedFile") + "Lap" + (CurrentLap - 1) + "Time", raceTimerTracker[3]);
    }

    private void UpdateFinishScreenStats()
    {
        for (int i = 0; i < 3; i++)
        {
            if (contentTop10.transform.GetChild(i).TryGetComponent(out LBPlace tempScriptSave))
            {
                top3Texts[i].text = tempScriptSave.FileName + " - " + tempScriptSave.FileTime;
            }
        }

        lapsReviewTexts[0].text = PlayerPrefs.GetString(PlayerPrefs.GetString("SelectedFile") + "Timer3");

        for (int i = 1; i < 4; i++)
        {
            if (PlayerPrefs.HasKey(PlayerPrefs.GetString("SelectedFile") + "Lap" + i + "Time"))
            {
                lapsReviewTexts[i].text = PlayerPrefs.GetString(PlayerPrefs.GetString("SelectedFile") + "Lap" + i + "Time");
            }
        }
    }
}