using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(TheCamera == null)
        {
            TheCamera = GetComponent<Camera>();
        }

        if(TheCamera == null)
        {
            TheCamera = Camera.main;
        }

        if(TheCamera == null)
        {
            Debug.LogError("Could not find a camera");
            return;
        }
        cameraRig = TheCamera.transform.parent;
        RootRB = ShipRoot.GetComponent<Rigidbody>();

        if(PlayerContr == null)
        {
            PlayerContr = PlayerController.playerContr;
        }
    }

    public Camera TheCamera;
    public GameObject ShipRoot;
    public PlayerController PlayerContr;
    private Rigidbody RootRB; 
    private Transform cameraRig;
    private Vector3 lastMousePos;
    public LayerMask layerMask;

    public float OrbitSensitivity = 8;
    public bool HoldToOrbit = false;

    public float ZoomMultiplier = 1;
    public float minDistance = 2;
    public float maxDistance = 25;
    public bool InvertZoomDirection = false;
    public float PanSpeed = 0.1f;
    float prevDistance;

    // Update is called once per frame
    void Update() 
    {
        if(!PlayerContr.InvDisplayParent.activeSelf)        //Inventory currently not active
        {
            OrbitCamera();
            DollyCamera();
        }
    }

    void DollyCamera()      //Zoom Camera
    {
        Vector3 actualChange;
        Vector3 newPosition;
        
        RaycastHit hit;
        bool freeSight = true;
        if(Physics.Raycast(TheCamera.transform.position, cameraRig.position - TheCamera.transform.position, out hit, (TheCamera.transform.position - cameraRig.position).magnitude, layerMask))        //hit something
        {
            newPosition = hit.point;
            Transform HitGO = hit.collider.transform;
            freeSight = false;
            while(HitGO != null)
            {
                if(HitGO.GetComponent<PlayerController>())
                {
                    freeSight = true;
                    break;
                }
                else if(HitGO.GetComponent<NPCharacter>())
                {
                    freeSight = true;
                    break;
                }
                else
                {
                    HitGO = HitGO.parent;
                }
            }
        }
        
        float delta = -Input.GetAxis("Mouse ScrollWheel");
        if(freeSight)
        {
            newPosition = TheCamera.transform.localPosition;
            if(delta != 0)
            {
                if(InvertZoomDirection)
                {
                    delta = -delta;
                }
                
                actualChange = newPosition * ZoomMultiplier * delta;
                if(Physics.Raycast(cameraRig.transform.position, TheCamera.transform.position + actualChange - cameraRig.position, out hit, (TheCamera.transform.position + actualChange - cameraRig.position).magnitude, layerMask))
                {
                    newPosition = (hit.distance - 0.1f) * newPosition.normalized;
                }
                else
                {
                    newPosition += actualChange;
                    prevDistance = 0;
                }
            }
            else if(prevDistance != 0)
            {
                if(Physics.Raycast(cameraRig.transform.position, TheCamera.transform.position - cameraRig.position, out hit, prevDistance, layerMask))
                {
                    newPosition = (hit.distance - 0.1f) * newPosition.normalized;
                }
                else
                {
                    newPosition = prevDistance * newPosition.normalized;
                    prevDistance = 0;
                }
            }
        }
        else
        {
            if(prevDistance == 0)
            {
                prevDistance = (TheCamera.transform.position - cameraRig.position).magnitude;
            }
            Physics.Raycast(cameraRig.transform.position, TheCamera.transform.position - cameraRig.position, out hit, (TheCamera.transform.position - cameraRig.position).magnitude, layerMask);
            newPosition = (hit.distance - 0.1f) * TheCamera.transform.localPosition.normalized;
        }
        
        newPosition = newPosition.normalized * Mathf.Clamp(newPosition.magnitude, minDistance, maxDistance);
        if(TheCamera.transform.localPosition != newPosition)
        {
            if(newPosition.z > 0)      //wrong side
            {
                TheCamera.transform.localPosition = -newPosition.normalized * minDistance;
            }
            else
            {
                TheCamera.transform.localPosition = newPosition;
            }
        }
    }

    void OrbitCamera()      //rotate the Camera
    {
        if(Input.GetMouseButtonDown(1) == true)     //right Mousebutton was pressed this frame?
        {
            lastMousePos = Input.mousePosition;
        }
        if(UnityEngine.Cursor.lockState == CursorLockMode.Locked)     //right Mousebutton being pressed? Input.GetMouseButton(1) == true || 
        {
            Vector3 currentMousePos = Input.mousePosition;

            //Vector3 mouseMovement = currentMousePos - lastMousePos;     //the Movement of the Mouse
            Vector3 mouseMovement = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
            Vector3 posRelativeToRig = TheCamera.transform.localPosition;
            Vector3 rotationAngles = mouseMovement / OrbitSensitivity;

            if(HoldToOrbit)
            {
                rotationAngles *= Time.deltaTime;
            }
            //Quaternion theOrbitalRotation = Quaternion.Euler( rotationAngles.y, rotationAngles.x, 0);

            //posRelativeToRig = theOrbitalRotation * posRelativeToRig;

            TheCamera.transform.RotateAround(cameraRig.position, TheCamera.transform.right, -rotationAngles.y);     //rotate the Camera (up and down)
            ShipRoot.transform.Rotate( 0, rotationAngles.x, 0, Space.World);        //rotate the Player

            //Quaternion lookRotation = Quaternion.LookRotation(- TheCamera.transform.localPosition);
            //TheCamera.transform.rotation = lookRotation;

            //if(cameraRig.transform.parent != null) //is Flightmode enabled?
            if(!RootRB.isKinematic) //is Flightmode enabled?
            {
                TheCamera.transform.LookAt(cameraRig, Vector3.up);      //ShipRoot.transform.up
            }
            else
            {
                TheCamera.transform.LookAt(cameraRig);
            }

            if(HoldToOrbit == false)
            {
                lastMousePos = currentMousePos;
            }
        }
    }

    public void StraightenUp()
    {
        cameraRig.transform.position = cameraRig.transform.position + new Vector3( 0, 3, 0);
    }

    public void PlayerDeath()
    {
        
    }
}
