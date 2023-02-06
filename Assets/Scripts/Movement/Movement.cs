using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;   
    public float jumpSpeed; 
    public float gravity;   

    private CharacterController controller; 
    private Vector3 MoveDir;

    private float camRotX;
    private float camRotY;
    public float sensitivity;

    void Start()
    {
        MoveDir = Vector3.zero;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            MoveDir = transform.TransformDirection(MoveDir);

            MoveDir *= speed;

            if (Input.GetButton("Jump"))
                MoveDir.y = jumpSpeed;

        }

        MoveDir.y -= gravity * Time.deltaTime;

        controller.Move(MoveDir * Time.deltaTime);

        camRotX += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        camRotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        camRotX = Mathf.Clamp(camRotX, -90, 90);

        transform.rotation = Quaternion.Euler(0, camRotY, 0);
    }

    private void LateUpdate()
    {
        Camera.main.transform.rotation = Quaternion.Euler(camRotX, camRotY, 0);
    }
}