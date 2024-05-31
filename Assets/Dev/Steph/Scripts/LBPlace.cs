using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LBPlace : MonoBehaviour
{
    private TextMeshProUGUI[] TMPtexts;
    public string FileName;
    public string FileTime;
    public string FileDeltaTime;

    private readonly float top3NameSize = 50;
    private readonly float top3TimeSize = 55;

    void Start()
    {
        TMPtexts = GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void UpdateUIText(int lbSpot, GameObject parent)
    {
        transform.SetParent(parent.transform);


        if (lbSpot <= 2)
        {
            TMPtexts[0].fontSize = top3NameSize;
            TMPtexts[1].fontSize = top3TimeSize;
      
            if (lbSpot == 0)
            {
                TMPtexts[0].color = Color.yellow;
                TMPtexts[1].color = Color.yellow;
            }
            if (lbSpot == 1)
            {
                TMPtexts[0].color = new Color(0.7f, 0.7f, 0.7f, 1);
                TMPtexts[1].color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            if (lbSpot == 2)
            {
                TMPtexts[0].color = new Color(1f, 0.5f, 0f, 1);
                TMPtexts[1].color = new Color(1f, 0.5f, 0f, 1);
            }
        }

        TMPtexts[0].text = FileName + " - ";
        TMPtexts[1].text = FileTime;
    }

}
