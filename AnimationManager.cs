using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{

    public Animator animator;
    public CharacterController characterController;
    public Vector3 previousPosition;

    private bool wasGrounded;

    public float TOLERANCE = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 relativePosition = transform.InverseTransformPoint(previousPosition);
        
            if (relativePosition.z < -TOLERANCE)
            {
                animator.SetInteger("walkType", 1);
            }
            else if (relativePosition.z > TOLERANCE)
            {
                animator.SetInteger("walkType", 3);
            }
            else if (relativePosition.x < -TOLERANCE)
            {
                animator.SetInteger("walkType", 2);
            }
            else if (relativePosition.x > TOLERANCE)
            {
                animator.SetInteger("walkType", 4);
            }
            else
            {
                animator.SetInteger("walkType", 0);
            }

            //animator.SetInteger("jump", 0);
        

        previousPosition = transform.position;
        
    }
}
