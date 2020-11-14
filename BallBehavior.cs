using UnityEngine;

//This is a barebone object behavior script
//it illustrates the user of authority and net functions

public class BallBehavior : MonoBehaviour
{
    void Start() {}

    void Update() {}

    ////////////////////////////
    //EXAMPLE Authority mechanic

    // This is a standard Unity collision detection. 
    // The problem: if we want to end a match right after it, for example,  
    // we may end up with many duplicate events because each client will detect it
    // one way to deal with it is to use the Net.authority boolean which is automatically
    // assigned to one and only one player in the game
    void OnCollisionEnter(Collision collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "Goal")
        {
            if (Net.authority)
            {
                Debug.Log("I am the game master and I decide to do something here");
            }
        }
    }

    
    //Can be invoked with Net.Function, it needs to be public and to have a string as argument
    public void CustomFunction(string arg)
    {
        print("Netfunction says " + arg);
        gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
    }
}
