using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Turret Settings")]
    public Transform turret;
    public Transform barrel;
    public Transform barrelEnd;
    [Header("Turret Controls")]
    public float turnSpeed = 10.0f;
    [Header("Barrel Controls")]
    public float barrelSpeed = 10.0f;
    public float barrelMaxAngle = 45.0f;
    public float barrelMinAngle = -5.0f;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    // Fixed issue with barrel rotation going beyond 180 degrees causing inversion
    {
        float turnInput = Input.GetAxis("Mouse X");
        float barrelInput = Input.GetAxis("Mouse Y");
    }
}
