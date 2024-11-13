//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Anzeige : MonoBehaviour
{
    public GameObject Camera;
    public Stats objectStats;
    public GameObject Healthbar;
    public TextMeshPro DisplayName;
    
    // Start is called before the first frame update
    void Start()
    {
        Camera = GameObject.Find("/Player/Camera Rig/Main Camera");
        objectStats = transform.parent.GetComponent<Stats>();
        transform.parent.GetComponent<Stats>().Display = this;
        DisplayName.text = objectStats.Name;
    }

    // Update is called once per frame
    void Update()
    {
        if(objectStats.Hitpoints <= 0)
        {
            Healthbar.transform.localScale = new Vector3(0, 1, 1);
        }
        else
        {
            Healthbar.transform.localScale = new Vector3((float) objectStats.Hitpoints / objectStats.MaxHitpoints, 1, 1);
        }
        
        
        this.transform.LookAt( Camera.transform);
    }

    public void Highlight()
    {
        DisplayName.text = "<b><u>" + objectStats.Name + "</u></b>";
    }

    public void Lowlight()
    {
        DisplayName.text = objectStats.Name;
    }
}
