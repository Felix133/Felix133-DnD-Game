using UnityEngine;
using TMPro;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class scriptDamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public Color BaseColor;
    public Color CritColor;
    public Color DeathColor;
    private Vector3 direction;
    private float speed;
    
    public static scriptDamagePopup Create(Transform parent, string damageAmount, int crit, Vector3 tempDirection)
    {
        Transform damagePopupTransform;
        damagePopupTransform = Instantiate(MainMenu.mainMenu.pfDamagePopup, parent.position, Quaternion.identity, parent);

        scriptDamagePopup damagePopup = damagePopupTransform.GetComponent<scriptDamagePopup>();
        damagePopup.direction = tempDirection;
        damagePopup.Setup(damageAmount, crit);
        
        return damagePopup;
    }

    public void Setup(string damageAmount, int crit)
    {
        textMesh.SetText(damageAmount);
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
        Destroy(this.gameObject, 2f);
        if(direction == Vector3.zero)       //no direction given -> random
        {
            direction = new Vector3( Random.Range(-0.6f, 0.6f), Random.Range(1.0f, 1.5f), 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position += (direction + new Vector3( 0, -2f * speed, 0.001f)) * Time.deltaTime;
        speed += Time.deltaTime;
    }
}
