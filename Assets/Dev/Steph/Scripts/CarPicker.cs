using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPicker : MonoBehaviour
{
    private UIMngr uiMan;

    [SerializeField] private List<GameObject> carModels;
    [SerializeField] private GameObject allCarsObject;
    public int currentCar;
    private bool isRotating = true;
    private bool areCarsMoving;

    private void Start()
    {
        uiMan = FindObjectOfType<UIMngr>();
        uiMan.ChangeVehicleInfoText();
    }

    void Update()
    {
        if (isRotating)
        {
            carModels[currentCar].transform.Rotate(0, 0.1f, 0);
        }
    }

    public void RightButtonPressed()
    {
        if (!areCarsMoving)
        {
            currentCar++;
            StartCoroutine(MoveCars(-20f));
        }
    }

    public void LeftButtonPressed()
    {
        if (!areCarsMoving)
        {
            currentCar--;
            StartCoroutine(MoveCars(20f));
        }
    }

    private void OnEnable()
    {
        if (currentCar < 2)
        {
            allCarsObject.transform.localPosition = new Vector3(20, 0, 0);
        }
        else
        {
            allCarsObject.transform.localPosition = Vector3.zero;
        }
    }

    public IEnumerator MoveCars(float howMuchMove)
    {
        areCarsMoving = true;
        if (currentCar == 3)
        {
            currentCar = 1;
            allCarsObject.transform.localPosition = new Vector3(40, 0, 0);
        }
        else if (currentCar == 0)
        {
            currentCar = 2;
            allCarsObject.transform.localPosition = new Vector3(-20, 0, 0);
        }

        yield return new WaitForEndOfFrame();
        isRotating = false;

        Vector3 startPos = allCarsObject.transform.localPosition;
        Vector3 endPos = new Vector3(allCarsObject.transform.localPosition.x + howMuchMove, allCarsObject.transform.localPosition.y, allCarsObject.transform.localPosition.z);

        float elapsedTime = 0;
        while (elapsedTime < 0.5f)
        {
            allCarsObject.transform.localPosition = Vector3.Lerp(startPos, endPos, (elapsedTime / 0.5f));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isRotating = true;
        PlayerPrefs.SetInt("Vehicle" + PlayerPrefs.GetString("SelectedFile"), currentCar);
        uiMan.ChangeVehicleInfoText();
        areCarsMoving = false;
    }
}
