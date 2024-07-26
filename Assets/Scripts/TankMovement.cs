using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsTank : MonoBehaviour
{
    [Header("Tank Settings")]
    public Camera tankCamera;
    [Tooltip("Top speed of the tank in km/h.")]
    public float topSpeed = 76.0f;
    [Tooltip("For tanks with front/rear wheels defined, this is how far those wheels turn.")]
    public float steeringAngle = 30.0f;
    [Tooltip("Power of any wheel listed under powered wheels.")]
    public float motorTorque = 10.0f;
    [Tooltip("Turn rate that is \"magically\" applied regardless of what the physics state of the tank is.")]
    public float magicTurnRate = 45.0f;
    [Tooltip("Brake force applied when no input is detected.")]
    public float brakeForce = 100.0f;
    [Tooltip("Assign this to override the center of mass. This can be useful to make the tank more stable and prevent it from flipping over. \n\nNOTE: THIS TRANSFORM MUST BE A CHILD OF THE ROOT TANK OBJECT.")]
    public Transform centerOfMass;
    [Tooltip("This prefab will be instantiated as a child of each wheel object and mimic the position/rotation of that wheel. If the prefab has a diameter of 1m, it will scale correct to match the wheel radius.")]
    public Transform wheelModelPrefab;
    [Tooltip("Scale multiplier for the wheel model prefab. This is used to adjust the size of the wheel model to match the wheel collider.")]
    public float scaleMultiplier = 100.0f;
    [Header("Wheels and Steering")]
    [Tooltip("Front wheels used for steering by rotating the wheels left/right.")]
    public WheelCollider[] front;
    [Tooltip("Front understeer wheels used for steering by rotating the wheels left/right.")]
    public WheelCollider[] understeer;
    [Tooltip("Rear wheels for steering by rotating the wheels left/right.")]
    public WheelCollider[] rear;
    [Tooltip("Wheels that provide power and move the tank forwards/reverse.")]
    public WheelCollider[] poweredWheels;
    [Tooltip("Wheels on the left side of the tank that are used for differential steering.")]
    public WheelCollider[] left;
    [Tooltip("Wheels on the right side of the tank that are used for differential steering.")]
    public WheelCollider[] right;
    [Header("Tracks")]
    public WheelCollider leftMainWheel;
    public WheelCollider rightMainWheel;
    public Renderer leftTracksRenderer;
    public Renderer rightTracksRenderer;
    private Vector2 leftTextureOffset = Vector2.zero;
    private Vector2 rightTextureOffset = Vector2.zero;
    public float textureSpeedMultiplier = 1f;
    public float textureRotationMultiplier = 0.5f;
    [Header("Ui Debug")]
    public bool showUiDebug = false;
    public TextMeshProUGUI speedText;

    private Rigidbody rigid;
    private float forwardInput, turnInput = 0.0f;
    private Dictionary<WheelCollider, Transform> WheelToTransformMap;

    private void Awake()
    {
        tankCamera.enabled = false;
        rigid = GetComponent<Rigidbody>();
        WheelToTransformMap = new Dictionary<WheelCollider, Transform>(poweredWheels.Length);
    }

    private void Start()
    {
        if (centerOfMass != null)
        {
            if (centerOfMass.parent == transform)
                rigid.centerOfMass = centerOfMass.localPosition;
            else
                Debug.LogWarning(name + ": PhysicsTank cannot override center of mass when " + centerOfMass.name + " is not a child of " + transform.name);
        }

        if (wheelModelPrefab != null)
        {
            InstantiateWheelModelsFromPrefab(front);
            InstantiateWheelModelsFromPrefab(understeer);
            InstantiateWheelModelsFromPrefab(rear);
            InstantiateWheelModelsFromPrefab(poweredWheels);
            InstantiateWheelModelsFromPrefab(left);
            InstantiateWheelModelsFromPrefab(right);
        }
    }

    private void Update()
    {
        forwardInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        if (showUiDebug)
        {
            float speed = rigid.velocity.magnitude * 3.6f;
            speedText.text = "Speed: " + speed.ToString("F2") + " km/h";
        }
    }

    private void FixedUpdate()
    {
        RunPoweredWheels();
        RunDifferentialSteeringWheels();
        RunFourWheelSteeringWheels();
        RunMagicRotation();
        if (leftTracksRenderer && rightTracksRenderer != null)
            UpdateTextureOffset();
    }
    private void RunPoweredWheels()
    {
        foreach (WheelCollider wheel in poweredWheels)
        {

            if (rigid.velocity.magnitude <= topSpeed / 3.6f)
                wheel.motorTorque = forwardInput * motorTorque;
            else
                wheel.motorTorque = 0.0f;
            if (Mathf.Abs(forwardInput) < 0.1f && Mathf.Abs(turnInput) < 0.1f)
            {
                wheel.brakeTorque = brakeForce;
            }
            else
            {
                wheel.brakeTorque = 0.0f;
            }

            if (wheelModelPrefab != null && WheelToTransformMap.ContainsKey(wheel))
            {
                Vector3 position;
                Quaternion rotation;
                wheel.GetWorldPose(out position, out rotation);
                WheelToTransformMap[wheel].position = position;
                WheelToTransformMap[wheel].rotation = rotation;
            }
        }
    }
    private void RunDifferentialSteeringWheels()
    {
        foreach (WheelCollider wheel in left)
            wheel.motorTorque += motorTorque * turnInput;
        foreach (WheelCollider wheel in right)
            wheel.motorTorque -= motorTorque * turnInput;
    }
    private float currentSteeringAngle = 0.0f;
    private void RunFourWheelSteeringWheels()
    {
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, turnInput * steeringAngle, Time.deltaTime * 5);
        if (front != null)
            foreach (WheelCollider wheel in front)
                wheel.steerAngle = currentSteeringAngle;
        if (understeer != null)
            foreach (WheelCollider wheel in understeer)
                wheel.steerAngle = currentSteeringAngle / 2;
        if (rear != null)
            foreach (WheelCollider wheel in rear)
                wheel.steerAngle = -currentSteeringAngle;
    }
    private void RunMagicRotation()
    {
        Quaternion magicRotation = transform.rotation * Quaternion.AngleAxis(magicTurnRate * turnInput * Time.deltaTime, transform.up);
        rigid.MoveRotation(magicRotation);
    }
    private void InstantiateWheelModelsFromPrefab(WheelCollider[] wheels)
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (WheelToTransformMap.ContainsKey(wheel) == false)
            {
                Transform temp = Instantiate(wheelModelPrefab, wheel.transform, false);
                temp.localScale = wheelModelPrefab.localScale * scaleMultiplier;
                WheelToTransformMap.Add(wheel, temp);
            }
        }
    }

    private void UpdateTextureOffset()
    {
        float rotationL = leftMainWheel.rotationSpeed * textureRotationMultiplier;
        float rotationR = rightMainWheel.rotationSpeed * textureRotationMultiplier;

        leftTextureOffset.y -= rotationL * (textureSpeedMultiplier * 0.0001f);
        rightTextureOffset.y -= rotationR * (textureSpeedMultiplier * 0.0001f);

        leftTracksRenderer.material.mainTextureOffset = leftTextureOffset;
        rightTracksRenderer.material.mainTextureOffset = rightTextureOffset;
    }
}
