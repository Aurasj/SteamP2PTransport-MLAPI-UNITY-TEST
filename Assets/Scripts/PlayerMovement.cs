using MLAPI;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public Transform Camera;

    public float movementSpeed;

    public float rotationSpeed;
    float rotX;
    float rotY;


    private void Start()
    {
        if (!IsLocalPlayer)
        {
            Camera.GetComponent<AudioListener>().enabled = false;
            Camera.GetComponent<Camera>().enabled = false;
        }
    }
    private void Update()
    {
        if (IsLocalPlayer)
        {
            Movement();
            Rotation();
        }
    }

    private void Movement()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(horizontal, 0, vertical) * (movementSpeed * Time.deltaTime));
    }
    private void Rotation()
    {
        rotX -= Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
        rotY += Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;

        if (rotX < -45)
        {
            rotX = -45f;
        }
        else if (rotX > 45)
        {
            rotX = 45;
        }

        transform.rotation = Quaternion.Euler(0, rotY, 0);
        Camera.transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }
}
