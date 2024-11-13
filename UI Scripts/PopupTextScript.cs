using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupTextScript : MonoBehaviour
{
    public TextMeshPro textMesh;
    public Color BaseColor;
    public Color CritColor;
    public Color DeathColor;
    
    public static PopupTextScript Create(Transform pfDamagePopup, Vector3 position, int damageAmount, int crit)
    {
        Transform damagePopupTransform = Instantiate(pfDamagePopup, position, Quaternion.identity);
        PopupTextScript damagePopup = damagePopupTransform.GetComponent<PopupTextScript>();
        damagePopup.Setup(damageAmount, crit);

        return damagePopup;
    }

    public void Setup(int damageAmount, int crit)
    {
        textMesh.SetText(damageAmount.ToString());
        if(crit == 1)
        {
            textMesh.color = BaseColor;
        }
        else if(crit == 2)
        {
            textMesh.color = CritColor;
        }
        else if(crit == 3)
        {
            textMesh.color = DeathColor;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position += new Vector3(0, 0.1f * Time.deltaTime, 0);
        this.gameObject.transform.LookAt(Camera.main.transform);
        this.gameObject.transform.Rotate(0, 180, 0, Space.Self);
    }
}
