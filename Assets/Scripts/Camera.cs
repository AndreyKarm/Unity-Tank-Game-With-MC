using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Turret Settings")]
    [Header("Camera Settings")]
    public bool LockCursor = false;
    float x, y;
    public float sensetivity, distance;
    public Vector2 xminmax;
    public Transform target;
    void Start()
    {
        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        x += Input.GetAxis("Mouse Y") * sensetivity * -1;
        y += Input.GetAxis("Mouse X") * sensetivity;

        x = Mathf.Clamp(x, xminmax.x, xminmax.y);
        
        transform.eulerAngles = new Vector3(x, y+180, 0);

        transform.position = target.position - transform.forward * distance;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            // Debug.Log("Hit: " + hit.transform.name);
        }

        Debug.DrawLine(transform.position, transform.position + transform.forward * 100, Color.red);
    }
}