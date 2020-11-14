using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Smooth towards the current object
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    public Vector3 offset = new Vector3(0,10,-10);

    private void Start()
    {

        //if it's my avatar make the camera follow me
        if (gameObject.name == Net.myId)
        {
        }
        else
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        // Define a target position above and behind the target transform
        Vector3 targetPosition = transform.position + offset;

        // Smoothly move the camera towards that target position
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, targetPosition, ref velocity, smoothTime);
    }

}
