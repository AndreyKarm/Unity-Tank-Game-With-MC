using UnityEngine;

public class EnterTank : MonoBehaviour
{
    public GameObject tank;
    public GameObject player;
    public GameObject playerBackup;
    private bool inTank = false;
    PhysicsTank vehicleScript;
    void Start()
    {
        vehicleScript = GetComponent<PhysicsTank>();
        vehicleScript.enabled = false;
        playerBackup.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerBackup.SetActive(true);
            player.SetActive(false);
            player.transform.parent = tank.transform;
            vehicleScript.enabled = true;
            inTank = true;
        }
    }

    void Update()
    {
        if (inTank == true && Input.GetKeyDown(KeyCode.E))
        {
            playerBackup.SetActive(true);
            player.transform.parent = null;
            playerBackup.SetActive(false);
            vehicleScript.enabled = false;
            inTank = false;
        }
    }
}
