using UnityEngine;
using System.Collections;

public class ControlPlayer : MonoBehaviour {

    public float movementSpeed;
    public float rotationSpeed;
    private Vector3 movement;
    private Rigidbody playerRigidbody;
    public float MouseSensitivity;
    private static float mouseX;
    private static float mouseY;
    private const float MAX_MOUSE_Y = 360f;
    private const float MIN_MOUSE_Y = 0f;

    void Awake()
    {
        playerRigidbody = this.GetComponent<Rigidbody>();
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");
    }

	void Update () {
        /**
         * WASDQE movement control
         */
        if (Input.GetKey(KeyCode.W))
            playerRigidbody.AddForce(this.transform.forward * movementSpeed);
        if (Input.GetKey(KeyCode.A))
            playerRigidbody.AddForce(-this.transform.right * movementSpeed);
        if (Input.GetKey(KeyCode.S))
            playerRigidbody.AddForce(-this.transform.forward * movementSpeed);
        if (Input.GetKey(KeyCode.D))
            playerRigidbody.AddForce(this.transform.right * movementSpeed);
        if (Input.GetKey(KeyCode.Q))
            this.transform.rotation *= Quaternion.Euler(0, 0, rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.E))
            this.transform.rotation *= Quaternion.Euler(0, 0, -rotationSpeed * Time.deltaTime);

        /**
         * Mouse movement control
         */
        float mouseXUpdate = Input.GetAxisRaw("Mouse X");
        float mouseYUpdate = Input.GetAxisRaw("Mouse Y");

        if (mouseX != mouseXUpdate || mouseY != mouseYUpdate)
        {
            this.transform.rotation *= Quaternion.Euler(-(mouseYUpdate - mouseY) * MouseSensitivity * Time.deltaTime, 
                (mouseXUpdate - mouseX) * MouseSensitivity * Time.deltaTime, 0);
        }

    }
}
