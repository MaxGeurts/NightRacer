using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

//PlayerPref Keys
//String - SelectedFile - Name of which File You're Playing On
//Int    - AmountFiles  - Keeps track of the amount of existing files
//String - Name9        - 9 can be any number, keeps track of which of the existing files has which name
//Int    - VehicleXXX   - XXX can be any name, keeps track of which of the existing files has which vehicle last selected
//String - XXXTimer9    - XXX can be any name, 9 can be any number (are the checkpoints), keeps track of the segment timers + total time

public class UIMngr : MonoBehaviour
{
    private CarPicker carPicker;

    [SerializeField] private List<GameObject> menus;

    [SerializeField] private TMP_InputField fileNameInput;
    [SerializeField] private GameObject nameAlreadyExists;

    [SerializeField] private GameObject contentList;
    [SerializeField] private GameObject fileButtonPrefab;

    [SerializeField] private TextMeshProUGUI[] carText;
    [SerializeField] private GameObject[] powerupImage;

    [SerializeField] private List<GameObject> CreatedFilesObj;
    [SerializeField] private List<File> CreatedFilesScripts;

    [SerializeField] private GameObject carsViewThing;
    [SerializeField] private Animator loadingScreen;

    private Animator animCamera;

    public bool fileSelected;
    public File FileLastSelected;

    private int selectedFileNumber;

    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        carPicker = FindObjectOfType<CarPicker>();
        animCamera = Camera.main.GetComponent<Animator>();
        animCamera.enabled = false;
        carsViewThing.SetActive(false);
        loadingScreen.enabled = false;

        int amountFiles = PlayerPrefs.GetInt("AmountFiles", 0);
        for (int i = 0; i < amountFiles; i++)
        {
            CreateNewFile(false); //LoadFiles
            CreatedFilesScripts[i].fileName.text = PlayerPrefs.GetString("Name" + i.ToString());
        }

        foreach (GameObject screen in menus)
        {
            screen.SetActive(false);
        }
        menus[0].SetActive(true); //turn on first Main Menu
    }

    public void ChangeVehicleInfoText()
    {
        if (PlayerPrefs.HasKey("Vehicle" + PlayerPrefs.GetString("SelectedFile")))
        {
            carPicker.currentCar = PlayerPrefs.GetInt("Vehicle" + PlayerPrefs.GetString("SelectedFile"));
        }
        else
        {
            carPicker.currentCar = 1;
        }

        switch (carPicker.currentCar)
        {
            case 1:
                carText[0].text = "Golden Sled";
                powerupImage[0].SetActive(true);
                powerupImage[1].SetActive(false);
                break;
            case 2:
                carText[0].text = "Silver Sled";
                powerupImage[0].SetActive(false);
                powerupImage[1].SetActive(true);

                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeScreen(string whichScreenOn)
    {
        for (int i = 0; i < menus.Count; i++)
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

    public void PlayFile()
    {
        CheckLastFileSelected();

        PlayerPrefs.SetString("SelectedFile", CreatedFilesScripts[selectedFileNumber].fileName.text);
        ChangeVehicleInfoText();
        carText[1].text = "File: " + PlayerPrefs.GetString("Name" + (selectedFileNumber));
        ChangeScreen("Car Picker");
    }

    public void CheckName()
    {
        if (PlayerPrefs.HasKey("Name" + fileNameInput.text))
        {
            StartCoroutine(NameWrong());
        }
        else
        {
            nameAlreadyExists.SetActive(false);
        }
    }

    private IEnumerator NameWrong()
    {
        nameAlreadyExists.SetActive(true);
        yield return new WaitForSeconds(3);
        nameAlreadyExists.SetActive(false);
    }

    public async void CreateNewFile(bool save)
    {
        if (PlayerPrefs.HasKey("Name" + fileNameInput.text) && save)
        {
            StartCoroutine(NameWrong());
        }
        else
        {
            menus[1].SetActive(true);
            GameObject newFile = Instantiate(fileButtonPrefab, contentList.transform);
            CreatedFilesObj.Add(newFile);
            CreatedFilesScripts.Add(newFile.GetComponent<File>());

            if (save)
            {
                PlayerPrefs.SetString("Name" + fileNameInput.text, fileNameInput.text);
                PlayerPrefs.SetString("Name" + (CreatedFilesScripts.Count - 1), fileNameInput.text);

                File fileScript = newFile.GetComponent<File>();
                await Task.Yield();
                menus[1].SetActive(false);
                fileScript.fileName.text = PlayerPrefs.GetString("Name" + fileNameInput.text, fileNameInput.text);
            }

            PlayerPrefs.SetInt("AmountFiles", CreatedFilesObj.Count);
            ChangeScreen("Files");
        }
    }

    public void DeleteSelectedFile()
    {
        CheckLastFileSelected();

        Destroy(CreatedFilesObj[selectedFileNumber]);
        CreatedFilesObj.RemoveAt(selectedFileNumber);

        for (int i = 0; i < 4 ; i++)
        {
            PlayerPrefs.DeleteKey(CreatedFilesScripts[selectedFileNumber].fileName.text + "Timer" + i);
        }

        PlayerPrefs.DeleteKey("Name" + selectedFileNumber);
        PlayerPrefs.DeleteKey("Name" + CreatedFilesScripts[selectedFileNumber].fileName.text);
        PlayerPrefs.DeleteKey("Vehicle" + CreatedFilesScripts[selectedFileNumber].fileName.text);

        CreatedFilesScripts.RemoveAt(selectedFileNumber);

        for (int i = 0; i < PlayerPrefs.GetInt("AmountFiles"); i++) //These 2 for-loops make sure the numbers of saved names ("Name9") align with the amount of files, so there won't be an index error
        {
            if (!PlayerPrefs.HasKey("Name" + i)) //Checks which "file" is empty in the amount files
            {
                for (int x = 0; x < (PlayerPrefs.GetInt("AmountFiles") - i); x++) //checks how many files have to be turned 1 slot down
                {
                    PlayerPrefs.SetString("Name" + i, PlayerPrefs.GetString("Name" + (i + 1))); //turns slots down ("Name3" becomes "Name2")
                }
            }
        }

        PlayerPrefs.SetInt("AmountFiles", CreatedFilesObj.Count);
    }

    public void CheckLastFileSelected()
    {
        for (int i = 0; i < CreatedFilesScripts.Count; i++)
        {
            if (CreatedFilesScripts[i] == FileLastSelected)
            {
                selectedFileNumber = i;
            }
        }
    }
    public void ViewCars(bool view)
    {
        carsViewThing.SetActive(view);
    }
    public void StartGame()
    {
        PlayerPrefs.SetInt("Vehicle" + PlayerPrefs.GetString("SelectedFile"), carPicker.currentCar);
        Debug.Log(carPicker.currentCar);
        animCamera.enabled = true;
        StartCoroutine(ChangeScene());
    }

    private IEnumerator ChangeScene()
    {
        foreach (GameObject screen in menus)
        {
            screen.SetActive(false);
        }
        yield return new WaitForSeconds(2.65f);
        loadingScreen.enabled = true;
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(1);
        Invoke("Main scene", 1f);
    }

}



