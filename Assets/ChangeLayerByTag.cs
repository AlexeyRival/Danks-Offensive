using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChangeLayerByTag : MonoBehaviour{

    public string tagSearched="tag";
    public int layer=0;
    public float Y=0;

    [ContextMenu("___Change Layer by Tag___")]

    public void ChangeLayer()
    {
        Debug.LogFormat(this + "==>Change layer by tag ");
        GameObject[] go = GameObject.FindGameObjectsWithTag(tagSearched);
        for (int i = 0; i < go.Length; i++)
        {
            Debug.LogFormat("==>{0} layer changed to {1} ", go[i].name, layer);
            go[i].layer = layer;
        }

    }
    [ContextMenu("___ChangeY by Tag___")]

    public void ChangeY()
    {
        Debug.LogFormat(this + "==>Change Y by tag");
        GameObject[] go = GameObject.FindGameObjectsWithTag(tagSearched);
        for (int i = 0; i < go.Length; i++)
        {
            Debug.LogFormat("==>{0} Y changed to {1} ", go[i].name, Y);
            go[i].transform.position=go[i].transform.position+new Vector3(0,-go[i].transform.position.y+Y,0);
        }

    }

}
