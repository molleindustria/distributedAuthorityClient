using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Appearance : MonoBehaviour
{
    //the container of all interchangeable heads
    //it's part of the skeleton
    //it needs to be dragged in the field
    public GameObject head;

    //the current head mesh, changes every time
    public MeshRenderer headMesh;

    //the body mesh (rigged)
    public SkinnedMeshRenderer bodyMesh;
    
    public string nickName;
    
    
    // Start is called before the first frame update
    void Start()
    {

        //an avatar with the appearance has been created, initialize the appearance, reference this object in the global player container
        if (Net.connected)
        {
            if (Net.players.ContainsKey(gameObject.name))
            {
                //this is in-game, fetch the name
                Debug.Log("Init " + Net.players[gameObject.name].nickName);
                nickName = Net.players[gameObject.name].nickName;
                setAppearance(Net.players[gameObject.name].DNA);

                //add a reference to this game object
                Net.players[gameObject.name].gameObject = gameObject;

            }
            else
                print("Error no player data corresponding to " + gameObject.name);
        }

        //make MY head invisible to my client
        if (gameObject.name == Net.myId || gameObject.name == "Player")
        {
            //invisible but cast shadows
            headMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;            
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    //from data received from the server
    public void setAppearance(float[] DNA)
    {
        changeHead(DNA[1]);
        changeSkinColor(DNA[0]);
    }
    
    //deactivate all heads except for the one at the corresponding index
    public void changeHead(float value)
    {
        var headIndex = Mathf.RoundToInt((head.transform.childCount-1) * value);
        
        for (int i = 0; i < head.transform.childCount; i++)
        {
            GameObject o = head.transform.GetChild(i).gameObject;
            bool isActive = (headIndex == i);
            o.SetActive(isActive);

            if (isActive)
            {
                headMesh = o.GetComponent<MeshRenderer>();
            }
        }

    }

    public void changeSkinColor(float value)
    {
        float H, S, V;
        Color.RGBToHSV(bodyMesh.material.color, out H, out S, out V);

        bodyMesh.material.color = Color.HSVToRGB(value, 1, V);
        headMesh.material.color = Color.HSVToRGB(value, 1, V);   
    }

    
}


