using UnityEngine;
using System;

//This a script makes a gameObject "Networked"
//it keeps track of transform changes, ownership, type and netVariables

public class NetObject : MonoBehaviour
{
    //the id of the current owner
    public string owner;

    //the prefab the gameObject was instantiated from
    public string prefabName;
    
    public int type = Net.SHARED;

    //Modify NetVariables.cs to add your own
    public NetVariables netVariables;

    //The rigidbody is not necessary
    private Rigidbody rb;
    private bool defaultKinematic;

    private float update;

    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public Vector3 targetLocalScale;

    //precision vs smooth
    public float INTERPOLATION = 0.2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) defaultKinematic = rb.isKinematic;

        print("Netobject starts" + transform.position);

        update = 0;
        
        targetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        targetRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        targetLocalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //are we connected?
        if (Net.connected) {

            update += Time.deltaTime;

            //time to do the update
            if (update >= Net.UPDATE)
            {
                update = 0;

                //did the transform change since last time it was set to false?
                if (transform.hasChanged)
                {
                    transform.hasChanged = false;

                    //is this object owned by this client?
                    if (owner == Net.myId)
                    {
                        //update all the others
                        Net.UpdateTransform(gameObject);
                    }
                    else 
                    {
                        //if not mine and has a rigid body behaves as kinematic
                        //meaning it won't be affected by gravity while still existing in the physics world
                        if (rb != null)
                            rb.isKinematic = true;
                    }
                }
            }
        }

        //is this object owned by this client?
        if (owner != Net.myId)
        {
            //print(transform.position + " >>> " + targetPosition);
            //print("Dista"+Vector3.Distance(transform.position, targetPosition));
            float dist = Vector3.Distance(transform.position, targetPosition);

            //if dist is too much teleport
            if (dist > 1f)
            {
                transform.position = targetPosition;
            }
            else if (dist > 0.001f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, INTERPOLATION);
            }

            if (Math.Abs(Quaternion.Angle(transform.rotation, targetRotation)) > 1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, INTERPOLATION);
            }

            if (Vector3.Distance(transform.localScale, targetLocalScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetLocalScale, INTERPOLATION);
            }
        }

    }//end update

    //called at the beginning when vars are initialized
    public void OnVariableInit()
    {
        if (prefabName == "OtherAvatarPrefab" || prefabName == "AvatarPrefab")
        {
            //set current state, animation etc
        }
    }

    //This function is called every time one or more variables are changed with Net.SetVariables
    //Here you can customize the effects of such changes
    public void OnVariableChange(string varName)
    {
        
        if (prefabName == "OtherAvatarPrefab" || prefabName == "AvatarPrefab")
        {
            //do something when an avatar variable changes
       
        }
        
    }
}

