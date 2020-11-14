using UnityEngine;
using System;
using System.Collections.Generic;
using UnitySocketIO;
using UnitySocketIO.Events;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEditor;


//This is the "brain" of the whole network architecture
//Make sure there is only one in the scene and on the same component as SocketIOController
//Configure the SocketIOController fields

public class NetManager : MonoBehaviour
{

    //for quick deployment I have different presets for Glitch and Heroku
    public bool GLITCH = false;
    public bool HEROKU = false;
    //if both are false go with the values in SocketIOController

    //the socket or, the pipe connecting this client to the server
    public SocketIOController socket;

    //a game menu script for character selection and such
    //if not specified connects directly
    public GameMenu gameMenu;

    //names of prefabs in the resource folder (optional)
    public string MyAvatarPrefabName ="";
    public string OtherAvatarPrefabName = "";
    
    //reference to my avatar (if any)
    public GameObject myAvatar;
    
    public Vector3 spawnPoint = new Vector3(11.19f, 0, 29.6f);
    public float spawnRadius = 2;

    
    void Start()
	{
        Application.targetFrameRate = 60;

        socket = GetComponent<SocketIOController>();
        gameMenu = GetComponent<GameMenu>();

        //to switch quickly between local and online you can override the settings here
        if (GLITCH)
        {
            socket.settings.url = "yourglitchdomain.glitch.me";
            socket.settings.port = 0 ;
            socket.settings.sslEnabled = true;
        }
        else if (HEROKU)
        {
            socket.settings.url = "yourherokudomain.herokuapp.com";
            socket.settings.port = 5000;
            socket.settings.sslEnabled = true;
        }

        //connect to the server
        socket.Connect();

        Net.connected = false;
        
        //Listeners connecting socket events from the server and the functions below
        socket.On("socketConnect", OnSocketConnect);
        
        socket.On("nameError", OnNameError);
        
        //any player connected including me
        socket.On("playerJoin", OnPlayerJoin);

        //just connected, server sends me all the player info
        socket.On("addPlayerData", OnAddPlayerData);
        
        //another player disconnected
        socket.On("playerDisconnect", OnPlayerDisconnect);
        
        //a net object is instantiated
        socket.On("instantiate", OnNetInstantiate);

        //or destroyed
        socket.On("destroy", OnNetDestroy);

        //a net object position has changed
        socket.On("updateTransform", OnUpdateTransform);

        //a net object changed ownership
        socket.On("changeOwner", OnChangeOwner);

        //a net object changed type TEMPORARY, PRIVATE, SHARED... 
        socket.On("changeType", OnChangeType);
        
        //net variables changed
        socket.On("setVariables", OnSetVariables);

        //a net function has been called
        socket.On("netFunction", OnNetFunction);
        
        //the server changes the authority
        socket.On("setAuthority", OnSetAuthority);

        //a chat message arrives
        socket.On("message", OnMessage);
        
        //Net.cs is just a global class
        //every variable in there is visible from anywhere

        //initialize players
        Net.players = new Dictionary<string, Player>();
        //initialize net objects
        Net.objects = new Dictionary<string, NetObject>();
        
        //make a global reference to this script
        Net.manager = this;
        

        ServerMessage("Connecting to server...");

        Invoke("Timeout", 10);

    }//end start

    public void Timeout()
    {
        if (!Net.connected)
        {
            ServerMessage("Unable to connect");
        }
    }


    //server responded with connection success
    public void OnSocketConnect(SocketIOEvent e)
    {
        IntData data = JsonUtility.FromJson<IntData>(e.data.ToString());

        //special case: the server restarted, restart client
        if (Net.connected)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            ServerMessage("SERVER RESTARTED...");
            print("Attention: attempt to reconnect after the server went down, reloading the scene.");
            return;
        }
        else
        {
            ServerMessage("Connected. Players online: " + data.num);

            //if no game menu generate random nickname and send the data
            if (gameMenu == null)
            {
                AvatarData d = new AvatarData();
                d.nickName = "user"+UnityEngine.Random.Range(0, 10000);
                SendAvatarData(d);
            }

            Net.connected = true;
        }
    }

    //send data for validation
    public void SendAvatarData(AvatarData aData)
    {
        if(socket != null)
           socket.Emit("avatarData", JsonUtility.ToJson(aData));
    }


    public void OnNameError(SocketIOEvent e)
    {
        IntData data = JsonUtility.FromJson<IntData>(e.data.ToString());

        if(data.num == 4)
            ServerMessage("Invalid admin password");
        if (data.num == 5)
            ServerMessage("You can't have a blank name");
        if (data.num == 0)
            ServerMessage("Sorry, the name is reserved or already in use");
        if (data.num == 3)
            ServerMessage("Sorry, only Western Latin character allowed");

    }

    //somebody joined (including me)
    public void OnPlayerJoin(SocketIOEvent e)
    {
        //read the data (just the id)
        AvatarData data = JsonUtility.FromJson<AvatarData>(e.data.ToString());
        Debug.Log("[SocketIO] Player Connected: " + data.id + " " + data.nickName);

        //add a player object to my dictionary
        Player p = new Player();
        p.id = data.id;
        p.nickName = data.nickName;
        p.DNA = data.DNA;

        //is it me?
        if (data.id == socket.SocketID)
        {

            //close the menu, change the camera
            if(gameMenu != null)
            {
                gameMenu.CloseMenu();
            }

            if (MyAvatarPrefabName != "")
            {
                print("I officially join the game as " + data.nickName);
                
                Net.playing = true;
                
                //do I have to create an avatar?
                Net.myId = socket.SocketID;

                //just in case
                if (myAvatar != null)
                    Destroy(myAvatar);

                //instantiate on the scene
                myAvatar = Instantiate(Resources.Load(MyAvatarPrefabName) as GameObject);
                myAvatar.name = socket.SocketID;


                //fetch or add the netcomponent object
                NetObject netObj = myAvatar.GetComponent<NetObject>();

                if (netObj == null)
                    netObj = myAvatar.AddComponent<NetObject>();

                Net.objects[socket.SocketID] = netObj;
                netObj.owner = socket.SocketID;
                netObj.type = Net.TEMPORARY;
                netObj.prefabName = MyAvatarPrefabName;

                //other client should instantiate a different avatar without controls
                //and camera but if it's not provided that's fine too
                if (OtherAvatarPrefabName == "")
                    OtherAvatarPrefabName = MyAvatarPrefabName;

                //tell the server to tell the other clients to make a puppet avatar
                InstantiationData iData = new InstantiationData();
                iData.prefabName = OtherAvatarPrefabName;
                iData.uniqueId = socket.SocketID;
                iData.type = Net.TEMPORARY;

                //instantiate all avatars around 0,0 by default
                //this is game logic and can be changed here on the server or on the netObject upon instantiation 
                Vector2 spawnRange = UnityEngine.Random.insideUnitCircle * spawnRadius;
                myAvatar.transform.position = iData.position = new Vector3(spawnPoint.x + spawnRange.x, spawnPoint.y, spawnPoint.z + spawnRange.y);
                myAvatar.transform.localScale = iData.localScale = Vector3.one;
                myAvatar.transform.rotation = iData.rotation = Quaternion.identity;

                print("Instantiation " + myAvatar.transform.position);
                socket.Emit("instantiateAvatar", JsonUtility.ToJson(iData));

            }//end avatar creation

            //find all the static netObjects - they are the ones in the Unity scene with a netObject attached
            //their uniqueId is their gameObject name 
            NetObject[] sceneObjects = FindObjectsOfType(typeof(NetObject)) as NetObject[];

            foreach (NetObject item in sceneObjects)
            {
                if (item.owner == "")
                {
                    Debug.Log(item.gameObject.name + " is a netObject without an owner in the scene, make sure the server knows about it");

                    //if the object is orphan assign ownership to me and make sure the server knows about it
                    //this should happen only to the first user logging after the server starts
                    InstantiationData iData = new InstantiationData();
                    iData.prefabName = item.gameObject.name;
                    iData.uniqueId = item.gameObject.name;
                    iData.position = item.transform.position;
                    iData.localScale = item.transform.localScale;
                    iData.rotation = item.transform.rotation;
                    iData.netVariables = item.netVariables;
                    iData.type = Net.PERSISTENT;
                    //item.OnVariableInit();
                    //if the server knows about it there will be no followup
                    socket.Emit("registerObject", JsonUtility.ToJson(iData));

                }
            }


        }//is it me?

        //add the player object to the players dictionary
        Net.players[p.id] = p;

    }
    
	public void Update() { }
    

    public void OnAddPlayerData(SocketIOEvent e)
    {
        //read the data (just the id)
        AvatarData data = JsonUtility.FromJson<AvatarData>(e.data.ToString());
        
        //add a player object to my dictionary
        Player p = new Player();
        p.id = data.id;
        p.nickName = data.nickName;
        p.DNA = data.DNA;

        

        //add the player object to the players dictionary
        Net.players[p.id] = p;
    }

    
    

    //Instantiation for networked objects
    public void NetInstantiate(string prefabName, int type, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        //tell the server to tell the other clients to do the same
        InstantiationData data = new InstantiationData();
        data.prefabName = prefabName;
        data.position = position;
        data.localScale = localScale;
        data.rotation = rotation;
        data.type = type;

        
        socket.Emit("instantiate", JsonUtility.ToJson(data));
    }

    //from the server: a networked game object has been instantiated
    public void OnNetInstantiate(SocketIOEvent e)
    {   
        //this is the data coming from the server as JSON, it needs to be parses into
        //usable variables according to a serializable class defined at the bottom of this script
        InstantiationData data = JsonUtility.FromJson<InstantiationData>(e.data.ToString());

        //print("Net instantiate " + data.uniqueId+" "+ data.position);

        //see if there is already an object with that id
        GameObject o = GameObject.Find(data.uniqueId);
        
        //if not instantiate on the scene
        if(o == null)
           o = Instantiate(Resources.Load(data.prefabName) as GameObject);

        
        o.transform.position = data.position;
        o.transform.localScale = data.localScale;
        o.transform.rotation = data.rotation;
        o.name = data.uniqueId;
        

        //fetch or add the netcomponent object
        NetObject netObj = o.GetComponent<NetObject>();

        if (netObj == null)
        {
            netObj = o.AddComponent<NetObject>();
        }

        netObj.type = data.type;
        netObj.owner = data.owner;
        netObj.prefabName = data.prefabName;
        
        Net.objects[data.uniqueId] = netObj;

       netObj.OnVariableInit();
    }

    //destroy a net object
    public void NetDestroy(string uniqueId)
    {
        IdData data = new IdData();
        data.id = uniqueId;
        socket.Emit("destroy", JsonUtility.ToJson(data));
    }
    

    public void OnNetDestroy(SocketIOEvent e)
       {
        IdData data = JsonUtility.FromJson<IdData>(e.data.ToString());
        
        if (Net.objects.ContainsKey(data.id))
        {


            //destroy the object on stage
            if (Net.objects[data.id] != null)
            {
                //is it an avatar thingy
                if (Net.objects[data.id].prefabName == "OtherAvatarPrefab") {
                    //do something when another avatar disconnects
                }

                Destroy(Net.objects[data.id].gameObject);
            }
            //remove the object reference
            Net.objects.Remove(data.id);
        }
        else
        {
            print("Warning OnNetDestroy: I can't find a netobject named "+data.id);
        }
    }

    //request ownership
    public void RequestOwnership(string uniqueId)
    {
        OwnershipData data = new OwnershipData();
        data.uniqueId = uniqueId;
        data.owner = Net.myId;
        socket.Emit("requestOwnership", JsonUtility.ToJson(data));
    }


    public void OnChangeOwner(SocketIOEvent e)
    {
        OwnershipData data = JsonUtility.FromJson<OwnershipData>(e.data.ToString());
        
        if (Net.objects.ContainsKey(data.uniqueId))
        {
            Net.objects[data.uniqueId].owner = data.owner;
        }
        else
        {
            print("Warning OnChangeOwner: I can't find a netobject named " + data.uniqueId);
        }
    }

    //request a change of type
    public void ChangeType(string uniqueId, int type)
    {
        TypeData data = new TypeData();
        data.uniqueId = uniqueId;
        data.type = type;
        socket.Emit("changeType", JsonUtility.ToJson(data));
    }


    public void OnChangeType(SocketIOEvent e)
    {
        TypeData data = JsonUtility.FromJson<TypeData>(e.data.ToString());

        if (Net.objects.ContainsKey(data.uniqueId))
        {
            Net.objects[data.uniqueId].type = data.type;
        }
        else
        {
            print("Warning OnChangeType: I can't find a netobject named " + data.uniqueId);
        }
    }

    public void OnSetAuthority(SocketIOEvent e)
    {
        IdData data = JsonUtility.FromJson<IdData>(e.data.ToString());
        Debug.Log("Authority is set to: " + data.id);
        Net.authorityId = data.id;

        Net.authority = (data.id == Net.myId && data.id != "");
        
        if (Net.authority)
            print("I AM THE AUTHORITY!");
     }

    public void UpdateTransform(GameObject o)
    {
        //tell the server to tell the other clients to do the same
        TransformData data = new TransformData();
        data.uniqueId = o.name;
        data.position = o.transform.position;
        data.rotation = o.transform.rotation;
        data.localScale = o.transform.localScale;
        
        socket.Emit("updateTransform", JsonUtility.ToJson(data));
    }

    //from the server: a transform on a netObject has changed
    public void OnUpdateTransform(SocketIOEvent e)
    {
        TransformData data = JsonUtility.FromJson<TransformData>(e.data.ToString());
        
        //is there a networked object with that id?
        if (Net.objects.ContainsKey(data.uniqueId))
        {
            Net.objects[data.uniqueId].targetPosition = data.position;
            Net.objects[data.uniqueId].targetRotation = data.rotation;
            Net.objects[data.uniqueId].targetLocalScale = data.localScale;
        }
        else
        {
            print("Warning: no networked object named " + data.uniqueId);
        }
        
    }
    
    //from the server: a player disconnected
    public void OnPlayerDisconnect(SocketIOEvent e)
    {
        IdData data = JsonUtility.FromJson<IdData>(e.data.ToString());
        Debug.Log("[SocketIO] Player Disconnected: " + data.id);
        
        //remove the reference in the dictionary
        if (Net.players.ContainsKey(data.id))
        {
            Net.players.Remove(data.id);
        }

    }

    //calls a function on a script by name 
    public void NewMessage(string msg)
    {
        //tell the server to tell the other clients to do the same
        MessageData data = new MessageData();
        data.id = Net.myId;
        data.message = msg;
        socket.Emit("message", JsonUtility.ToJson(data));
    }

    //message from user or server
    public void OnMessage(SocketIOEvent e)
    {
        MessageData data = JsonUtility.FromJson<MessageData>(e.data.ToString());
        
        if (Net.players.ContainsKey(data.id))
        {
            //visualize chat message
        }
        else
        {
            ServerMessage(data.message);
        }
    }
    
    //calls a function on a script by name 
    public void NetFunction(string objectName, string componentName, string functionName, string argument)
    {
       
        //tell the server to tell the other clients to do the same
        NetFunctionData data = new NetFunctionData();
        data.objectName = objectName;
        data.componentName = componentName;
        data.functionName = functionName;
        data.argument = argument;
        socket.Emit("netFunction", JsonUtility.ToJson(data));
    }

    //from the server: a function is being called
    public void OnNetFunction(SocketIOEvent e)
    {
        
        NetFunctionData data = JsonUtility.FromJson<NetFunctionData>(e.data.ToString());
        Component comp = GetObjectComponent(data.objectName, data.componentName);
        
        if (comp != null)
        {
            //Uses a rather obscure system called Reflection to find a function
            MethodInfo method = comp.GetType().GetMethod(data.functionName);

            if (method != null)
                method.Invoke(comp, new object[] {data.argument});
            else
                print("Warning: there is no PUBLIC function named " + data.functionName + " on object " + data.objectName + " and component " + data.componentName);
        }
    }

    
    //send a variable change
    public void SetVariables(string uniqueId, NetVariables vars)
    {
        vars.uniqueId = uniqueId;
        
        socket.Emit("setVariables", JsonUtility.ToJson(vars));   
    }

    //from the server: a NetVariable changed
    public void OnSetVariables(SocketIOEvent e)
    {
        NetVariables data = JsonUtility.FromJson<NetVariables>(e.data.ToString());
        
        if (Net.objects.ContainsKey(data.uniqueId))
        {
            NetObject netObject = Net.objects[data.uniqueId];

            if(netObject.netVariables == null)
                netObject.netVariables = new NetVariables();

            Type myObjectType = data.GetType();

            FieldInfo[] myPropertyInfo = typeof(NetVariables).GetFields();
            
            //bool changed = false;
            //for all the fields in NetVariables
            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                //print(myPropertyInfo[i].ToString());
                //print(myPropertyInfo[i].GetValue(netObject.netVariables));
                var v1 = myPropertyInfo[i].GetValue(data);
                var v2 = myPropertyInfo[i].GetValue(netObject.netVariables);

                
                if (v1 != null && !System.Object.Equals(v1, v2))
                {
                    //set the variable
                    try
                    {
                        myPropertyInfo[i].SetValue(netObject.netVariables, v1);
                    }
                    catch(Exception err)
                    {
                        Debug.LogException(err, this);
                    }
                    
                    netObject.OnVariableChange(myPropertyInfo[i].Name);
                    
                }
            }//iteration
            
        }
    }
    
    //get a component from a NetObject only using names
    public Component GetObjectComponent(string objectName, string componentName)
    {
        Component comp = null;
        GameObject obj = GameObject.Find(objectName);

        //is there a networked object with that id?
        if (obj != null)
        {
            comp = obj.GetComponent(componentName);

            if (comp == null)
               print("Warning: there is no component on " + objectName + " named " + componentName);
        }
        else
            print("Warning: there is no networked object named " + objectName);
        
        return comp;
    }

    //Socket disconnection is not immediate so I detect when a client quits  
    void OnApplicationQuit()
    {
        IdData data = new IdData();
        data.id = Net.myId;

        if(socket != null)
            socket.Emit("quit", JsonUtility.ToJson(data));
    }

    void ServerMessage(string msg)
    {
        Debug.Log("Server message: " + msg);
    }

}


//a data structure with information about a player
public class Player
{
    public string id;
    public string nickName;
    public float[] DNA;
    public GameObject gameObject;
}


/*
 * All the data received from the server needs to comply to these classes below, otherwise it can't be parsed
 */


[Serializable]
public class MessageData
{
    public string id;
	public string message;
}

[Serializable]
public class InstantiationData
{
    public string owner;
    public string prefabName;
    public string uniqueId;
    public int type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public NetVariables netVariables;
}

[Serializable]
public class IdData
{
    public string id;
}

public class IntData
{
    public int num;
}

[Serializable]
public class OwnershipData
{
    public string uniqueId;
    public string owner;
}

[Serializable]
public class TypeData
{
    public string uniqueId;
    public int type;
}

[Serializable]
public class TransformData
{
    public string uniqueId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
}

[Serializable]
public class NetFunctionData
{
    public string objectName;
    public string componentName;
    public string functionName;
    public string argument;
}


[Serializable]
public class AvatarData
{
    public string id; //socketId
    public string nickName;
    public float[] DNA;
}