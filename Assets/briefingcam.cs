using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class briefingcam : MonoBehaviour
{

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(0.05f, 0,0);
        if (transform.position.z<-77||Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
    }
}
