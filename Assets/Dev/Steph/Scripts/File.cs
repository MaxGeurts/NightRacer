using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class File : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private UIMngr uiMan;

    public TextMeshProUGUI fileName;
    [SerializeField] private TextMeshProUGUI[] textsTimes;
    [SerializeField] private List<string> times;

    private GameObject infoPanel;
    private GameObject infoPanelSelected;

    private void Awake()
    {
        fileName = GetComponentInChildren<TextMeshProUGUI>();
        uiMan = FindObjectOfType<UIMngr>();
    }

    private void Start()
    {
        infoPanel = GameObject.Find("/Canvas/Files/InfoPanelFile");
        infoPanelSelected = GameObject.Find("/Canvas/Files/ActualInfo");

        textsTimes = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();

    }
    public void OnSelect(BaseEventData eventData)
    {
        uiMan.fileSelected = true;
        StartCoroutine(InfoPanel(true, 0.20f));

    }

    public void OnDeselect(BaseEventData eventData)
    {
        uiMan.fileSelected = false;
        uiMan.FileLastSelected = this;
        StartCoroutine(InfoPanel(false, 0.15f));
    }

    private IEnumerator InfoPanel(bool onOrOff, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        SetRaceNumbers();

        infoPanelSelected.SetActive(onOrOff);
        infoPanel.SetActive(onOrOff);
    }

    private void SetRaceNumbers()
    {
        if (PlayerPrefs.HasKey(fileName.text + "Timer" + 0.ToString()))
        {
            for (int i = 0; i < textsTimes.Length; i++)
            {
                times.Add(PlayerPrefs.GetString(fileName.text + "Timer" + i.ToString()));
                textsTimes[i].text = times[i];
            }
        }
        else
        {
            for (int i = 0; i < textsTimes.Length; i++)
            {
                textsTimes[i].text = "0.00.00";
            }
        }
    }

    private void OnDisable()
    {
        uiMan.fileSelected = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!uiMan.fileSelected)
        {
            SetRaceNumbers();
        }
        infoPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!uiMan.fileSelected)
        {
            infoPanel.SetActive(false);
        }
    }
}
