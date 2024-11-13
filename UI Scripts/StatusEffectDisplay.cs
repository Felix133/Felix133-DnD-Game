using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Header("Display Texts")]
    public TMP_Text damageMultText;
    public TMP_Text attackSpeedMultText;
    public TMP_Text tempRKText;
    public TMP_Text tempETW0Text;
    public TMP_Text SpeedMultText;
    public TMP_Text HitpointsMultText;
    public TMP_Text ManaMultText;
    

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.playerContr.playerStats.damageMult != 1)
        {
            damageMultText.text = "damageMult: " + PlayerController.playerContr.playerStats.damageMult;
            damageMultText.gameObject.SetActive(true);
        }
        else
        {
            damageMultText.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.attackSpeedMult != 1)
        {
            attackSpeedMultText.text = "attackSpeedMult: " + PlayerController.playerContr.playerStats.attackSpeedMult;
            attackSpeedMultText.gameObject.SetActive(true);
        }
        else
        {
            attackSpeedMultText.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.tempRK != 0)
        {
            tempRKText.text = "tempRK: " + PlayerController.playerContr.playerStats.tempRK;
            tempRKText.gameObject.SetActive(true);
        }
        else
        {
            tempRKText.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.tempETW0 != 0)
        {
            tempETW0Text.text = "tempETW0: " + PlayerController.playerContr.playerStats.tempETW0;
            tempETW0Text.gameObject.SetActive(true);
        }
        else
        {
            tempETW0Text.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.speedMult != 1)
        {
            SpeedMultText.text = "speedMult: " + PlayerController.playerContr.playerStats.speedMult;
            SpeedMultText.gameObject.SetActive(true);
        }
        else
        {
            SpeedMultText.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.HitpointsMult != 1)
        {
            HitpointsMultText.text = "HitpointsMult: " + PlayerController.playerContr.playerStats.HitpointsMult;
            HitpointsMultText.gameObject.SetActive(true);
        }
        else
        {
            HitpointsMultText.gameObject.SetActive(false);
        }

        if(PlayerController.playerContr.playerStats.ManaMult != 1)
        {
            ManaMultText.text = "ManaMult: " + PlayerController.playerContr.playerStats.ManaMult;
            ManaMultText.gameObject.SetActive(true);
        }
        else
        {
            ManaMultText.gameObject.SetActive(false);
        }
    }
}
