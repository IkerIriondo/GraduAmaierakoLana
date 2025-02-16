using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject basicCam;
    public GameObject combatCam;
    public GameObject topdownCam;

    public CameraStyle currentStyle;

    public enum CameraStyle{
        Basic,
        Combat,
        Topdown
    }

    private void Start(){
        /*Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
    }

    private void Update(){
        // jokalariaren orientation biratu
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // Player objektua biratu
        if(currentStyle == CameraStyle.Basic){

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if(inputDir != Vector3.zero){
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }else if(currentStyle == CameraStyle.Combat){

            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }else if(currentStyle == CameraStyle.Topdown){

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if(inputDir != Vector3.zero){
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        
    }

    private void SwitchCameraStyle(CameraStyle newStyle){
        
        combatCam.SetActive(false);
        basicCam.SetActive(false);
        topdownCam.SetActive(false);

        if(newStyle == CameraStyle.Basic) basicCam.SetActive(true);
        if(newStyle == CameraStyle.Combat) combatCam.SetActive(true);
        if(newStyle == CameraStyle.Topdown) topdownCam.SetActive(true);

        currentStyle = newStyle;
    }
}
