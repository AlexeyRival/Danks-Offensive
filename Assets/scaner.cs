using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scaner : MonoBehaviour
{
    public enemy mothertank;
    private void OnTriggerEnter(Collider other)
    {
        mothertank.Detect(other.gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        mothertank.Detect(other.gameObject);
    }
    private void FixedUpdate()
    {
        if (mothertank.target)
        {
            GetComponent<SphereCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            GetComponent<SphereCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = true;
        }
    }
}
