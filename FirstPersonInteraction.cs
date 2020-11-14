using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This script shows several kinds of interaction with NetObjects
//It mostly works by raycasting, the objects must have a collider and be 
//in the center of the viewport to be interacted with

public class FirstPersonInteraction : MonoBehaviour
{
    //The camera is usually on another component so drag and drop it here
    public Camera playerCamera;

    //I can only grab one object
    public GameObject grabbedObject;

    // Start is called before the first frame update
    void Start()
    {
        if(playerCamera == null)
        {
            playerCamera = transform.Find("AvatarCamera").gameObject.GetComponent<Camera>();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        //To configure buttons: Unity > edit > project settings > Input Manager
        //Fire1 is mapped to right click by default
        //to be sure I check if it's me
        if (gameObject.name == Net.myId)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //spawn in front of the player
                float spawnDistance = 1;
                Vector3 spawnPos = transform.position + transform.forward * spawnDistance;

                ///////////////////////////////
                //EXAMPLE Instantiate NetObject

                //instantiate a Cube (prefab name in Resources folder) at the spawning position and with default rotation and scale 
                Net.Instantiate("Cube", Net.PERSISTENT, spawnPos);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                float spawnDistance = 1;
                Vector3 spawnPos = transform.position + transform.forward * spawnDistance;
                Net.Instantiate("Ball", Net.SHARED, spawnPos);
            }

            if (playerCamera != null)
            {
                //interact with an object in front of you

                //create a raycast from the center of the camera view
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                RaycastHit hit;
                float maxDistance = 2;

                //shoot the ray and see if it hits a collider
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    //print("I'm looking at " + hit.transform.name);

                    //make sure it's a net object
                    if (grabbedObject == null)
                    {
                        NetObject nObj = hit.transform.gameObject.GetComponent<NetObject>();

                        //if I just clicked
                        if (nObj != null && Input.GetButtonDown("Fire1"))
                        {
                            //grab
                            if (nObj.type == Net.SHARED || nObj.type == Net.PERSISTENT)
                            {
                                grabbedObject = nObj.gameObject;

                                /////////////////////
                                //EXAMPLE ownership

                                //you need to request a change of ownership to propagate the position
                                Net.RequestOwnership(grabbedObject.name);

                                //if you don't want avatar to be able to snatch an object from other people's hands you can make it private
                                //note: this can only be done to netObjects you own, it won't take effect on somebody else's private and temporary netObjects
                                //Net.ChangeType(grabbedObject.name, Net.PRIVATE);

                                //spawn in front of the player
                                float grabDistance = 1;
                                Vector3 grabPosition = transform.position + transform.forward * grabDistance;
                                grabPosition.y = 0.8f;
                                grabbedObject.transform.position = grabPosition;
                                //parent it to the player object in the position it is now
                                grabbedObject.transform.SetParent(transform, true);

                                //if has rigidbody set it as isKinematic so it doesn't fall
                                if (grabbedObject.GetComponent<Rigidbody>() != null)
                                    grabbedObject.GetComponent<Rigidbody>().isKinematic = true;

                                /////////////////////
                                //EXAMPLE NetVars

                                /*
                                //create a NetVariables object to send to the server
                                //these variables will be automatically synced
                                NetVariables netVars = new NetVariables();
                                //change any of the properties contained in it
                                netVars.exampleVar = Random.Range(0, 3);
                                //in this case the variable changes the color, see NetVariables.cs
                                Net.SetVariables(grabbedObject.name, netVars);
                                */
                            }

                        }

                        ////////////////////////////
                        //EXAMPLE Destroy NetObject

                        //if I just right clicked on a cube
                        if (nObj != null && Input.GetButtonDown("Fire2"))
                            if (nObj.prefabName == "Cube")
                            {
                                GameObject clickedObject = nObj.gameObject;

                                //make sure I own it
                                Net.RequestOwnership(clickedObject.name);

                                //destroy it
                                Net.Destroy(clickedObject.name);
                            }

                        //////////////////////
                        //EXAMPLE NetFunction

                        //if I just right clicked on a ball
                        if (nObj != null && Input.GetButtonDown("Fire2"))
                            if (nObj.prefabName == "Ball")
                            {
                                //make sure I own it
                                Net.RequestOwnership(nObj.gameObject.name);

                                //Call a PUBLIC function (method) on an object - it will be called on the object on all clients
                                //Mind that if this function changes something that is not in the transform (position,rotation,scale)
                                //or in the netvars these changes will NOT be stored in the server. 
                                //Eg. if you change a texture, all the clients should do the same 
                                //BUT a new player logging in after the fact won't see the change!
                                //"hello" is an optional string parameter
                                Net.Function(nObj.gameObject.name, "BallBehavior", "CustomFunction", "hello");
                            }

                    }
                    else
                    {
                        //didn't hit anything with the raycast
                        //print("I'm looking at nothing!");
                    }
                }//raycast

                //drop
                if (Input.GetButtonUp("Fire1") && grabbedObject != null)
                {
                    //print("DROP");

                    //drop
                    if (grabbedObject.GetComponent<Rigidbody>() != null)
                        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;

                    grabbedObject.transform.parent = null;

                    //make it shared again so it can be picked up by others
                    //Net.ChangeType(grabbedObject.name, Net.SHARED);

                    //you can use the prefabName property of the NetObject to differentiate the 
                    //various classes of netobjects, in this case if it's a ball kick it
                    if (grabbedObject.GetComponent<NetObject>().prefabName == "Ball")
                    {
                        grabbedObject.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * 20, ForceMode.Impulse);
                    }

                    grabbedObject = null;

                }
            }
        }
    }
}
