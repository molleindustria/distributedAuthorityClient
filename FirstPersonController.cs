using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a standard first person controller meant to work with Unity's CharacterController component
//Note how there is no server communication here. 
//Since it controls an avatar NetObject the transform is automatically updated

//This script doesn't use physics, the player collides but it doesn't have a rigidbody
//you can find examples of physics-based character controller

public class FirstPersonController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float lookYLimit = 45.0f;


    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    
    float lookY = 0;

    [HideInInspector]
    public bool canMove = true;
    
    Quaternion targetRotation;
    
    void Start()
    {
        
        //must start disabled for some reason
        characterController = GetComponent<CharacterController>();
        characterController.enabled = true;
        // Lock cursor a pain when testing
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        lookY = transform.rotation.y;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            ///standard
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            lookY = Mathf.LerpAngle(playerCamera.transform.localEulerAngles.y, 0, 0.1f);
            //print(playerCamera.transform.forward.y +", "+transform.forward.y+" "+newY);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, lookY, 0);
            
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            
        }
        else
        {
            //talk/emote mode
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            //playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            lookY += Input.GetAxis("Mouse X") * lookSpeed;
            lookY = Mathf.Clamp(lookY, -lookYLimit, lookYLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, lookY, 0);
        
        }

        

    }
    
}
