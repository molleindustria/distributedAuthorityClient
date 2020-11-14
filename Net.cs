using UnityEngine;
using System.Collections.Generic;

public static class Net
{

    //Player objects not really used here but it's a useful data to have locally
    public static Dictionary<string, Player> players;

    //NetObject dictionary, all the objects that are syncronized
    public static Dictionary<string, NetObject> objects;

    //reference to netmanager component
    public static NetManager manager;

    //automatic transform update rate for netobjects
    public static float UPDATE = 1/30; //s
    
    //the client socket id
    public static string myId = "";
    //am I the authority?
    public static bool authority = false;
    public static string authorityId = "";
    //is the socket connected?
    public static bool connected = false;
    //after the character selection
    public static bool playing = false;

    //netobject types, they must match the same constant on the server side
    public static int TEMPORARY = 0;
    public static int PRIVATE = 1;
    public static int SHARED = 2;
    public static int PERSISTENT = 3;

    public static AvatarData myAvatarData;
    
    //global shortcuts and overloads
    //an overload is simply a version of a function with different parameters
    
    public static void Instantiate(string prefabName, int type)
    {
        manager.NetInstantiate(prefabName, type, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public static void Instantiate(string prefabName, int type, Vector3 position)
    {
        manager.NetInstantiate(prefabName, type, position, Quaternion.identity, Vector3.one);
    }

    public static void Instantiate(string prefabName, int type, Vector3 position, Quaternion rotation)
    {
        manager.NetInstantiate(prefabName, type, position, rotation, Vector3.one);
    }

    public static void Instantiate(string prefabName, int type, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        manager.NetInstantiate(prefabName, type, position, rotation, localScale);
    }

    public static void Destroy(string id)
    {
        manager.NetDestroy(id);
    }

    public static void RequestOwnership(string uniqueId)
    {
        manager.RequestOwnership(uniqueId);
    }
    
    public static void UpdateTransform(GameObject gameObject)
    {
        manager.UpdateTransform(gameObject);
    }

    public static void SetVariables(string uniqueId, NetVariables netVars)
    {
        manager.SetVariables(uniqueId, netVars);
    }

    public static void ChangeType(string uniqueId, int type)
    {
        manager.ChangeType(uniqueId, type);
    }
    
    public static void Function(string objectName, string componentName, string functionName)
    {
        manager.NetFunction(objectName, componentName, functionName, "");
    }

    public static void Function(string objectName, string componentName, string functionName, string argument)
    {
        manager.NetFunction(objectName, componentName, functionName, argument);
    }

}
