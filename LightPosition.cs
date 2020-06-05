using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPosition : MonoBehaviour
{
    public Material spiky;
    public GameObject lamp;

    void Update()
    {
        // Get the new light posiiton
        Vector4 change = new Vector4(
            lamp.transform.position.x,
            lamp.transform.position.y,
            lamp.transform.position.z,
            0);
        
        // Set the new light position
        spiky.SetVector("_LightPoint", change);
    }
}
