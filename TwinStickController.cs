using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinStickController : MonoBehaviour
{
    private Rigidbody rb;
    private CharacterController characterController;
    public float gravity = 1;
    public float movementSpeed = 10;
    public float angularVelocity = 1;
    public float DEAD_ZONE = 0.2f;

    public Vector3 moveDirection = Vector3.zero;
    public Vector3 lookDirection = Vector3.zero;
    

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
    }

    public void Update()
    {
        //Analog controls add a dead zone
        //must be configured in Player > Input
        if(Mathf.Abs(Input.GetAxis("HorizontalLeft")) > DEAD_ZONE || Mathf.Abs(Input.GetAxis("VerticalLeft")) > DEAD_ZONE)
            moveDirection = new Vector3(Input.GetAxis("HorizontalLeft"), moveDirection.y, Input.GetAxis("VerticalLeft"));
        else
            moveDirection = new Vector3(0, moveDirection.y, 0);

        //alternative keyboard control
        if (Input.GetKey(KeyCode.A))
            moveDirection.x = -1;
        if (Input.GetKey(KeyCode.D))
            moveDirection.x = 1;
        if (Input.GetKey(KeyCode.W))
            moveDirection.z = 1;
        if (Input.GetKey(KeyCode.S))
            moveDirection.z = -1;


        if (Mathf.Abs(Input.GetAxis("HorizontalRight")) > DEAD_ZONE || Mathf.Abs(Input.GetAxis("VerticalRight")) > DEAD_ZONE)
            lookDirection = new Vector3(Input.GetAxis("HorizontalRight"), 0, Input.GetAxis("VerticalRight"));

        //alt controls
        if (Input.GetKey(KeyCode.LeftArrow))
            lookDirection.x = -1;
        if (Input.GetKey(KeyCode.RightArrow))
            lookDirection.x = 1;
        if (Input.GetKey(KeyCode.UpArrow))
            lookDirection.z = 1;
        if (Input.GetKey(KeyCode.DownArrow))
            lookDirection.z = -1;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        
        Quaternion dir = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, dir, Time.deltaTime * angularVelocity);

        // Move the controller
        characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
        

    }
}
