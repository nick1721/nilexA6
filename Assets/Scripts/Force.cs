using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Force : MonoBehaviour
{

    bool launched;
    // Start is called before the first frame update
    void Start()
    {
        launched = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!launched)
        {
            Vector3 impulse = new Vector3(0.0f, 20.0f, 0.0f);
            //launched = true;
            GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
        }
        
    }
}
