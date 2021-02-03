using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class player_camera : MonoBehaviour
{
    private Vector2 rot;
    public GameObject head, cam, targetplane,preMuzzle, actualtargetplane;
    public static bool whatcam;
    public static float range = 60f;
    public Texture2D aimtexture;
    public float towerspeed=1f;
    private RaycastHit hit;
    private int LayerMaskWithourMarkers;
    private Vector3 targetorigin;
    private float fff=0f,dff=0f;
    private player_tank tank;
    private GameObject[] obs;
    void Start()
    {
        targetorigin = actualtargetplane.transform.localPosition;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        LayerMaskWithourMarkers = ~(1<<9);
        tank = transform.parent.GetComponent<player_tank>();
    }
    public void Rot(float angle) {
        transform.Rotate(0,angle,0);
        fff += angle;
    }
    void Update()
    {
        if (tank.hp < 0) { return; }
        if (range > 120f) { range = 120f; }
        if (range < 60f && !whatcam) { range = 60f; }
        if (range > (whatcam ? 30f : 60f)) { range -= 0.1f; }
        targetplane.transform.localScale = new Vector3(range, range, 1f);
        rot = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        transform.Rotate(0, rot.x * (whatcam ? 0.5f : 10f), 0);
        fff += rot.x * (whatcam ? 0.5f : 10f);
        if (preMuzzle.transform.localRotation.y < .05f && -cam.transform.localRotation.z > preMuzzle.transform.localRotation.y) {
            preMuzzle.transform.Rotate(0, whatcam ? 0.1f : 0.35f, 0);
        }
        if (preMuzzle.transform.localRotation.y > -.05f && -cam.transform.localRotation.z < preMuzzle.transform.localRotation.y) {

            preMuzzle.transform.Rotate(0, whatcam ? -0.1f : -0.35f, 0);
        }
        cam.transform.Rotate(0, 0, rot.y * (whatcam ? 0.5f : 5f));
        if (dff < fff) {
            head.transform.Rotate(0, (dff - fff < -1f ? (1f * towerspeed) : 0.1f), 0);
            dff += (dff - fff < -1f ? (1f * towerspeed) : 0.1f);
        }
        if (dff > fff)
        {
            head.transform.Rotate(0, (dff - fff > 1f ? -(1f * towerspeed) : -0.1f), 0);
            dff += (dff - fff > 1f ? -(1f * towerspeed) : -0.1f);

        }
        if (fff > dff + 180) { fff -= 360; }
        if (fff < dff - 180) { fff += 360; }
        Debug.DrawRay(preMuzzle.transform.position, -preMuzzle.transform.right, Color.green);
        if (Physics.Raycast(preMuzzle.transform.position, -preMuzzle.transform.right, out hit, 20f, LayerMaskWithourMarkers))
        {
            actualtargetplane.transform.position = hit.point;
            //actualtargetplane.transform.localPosition += new Vector3(2,0,0);
        }
        else {
            actualtargetplane.transform.localPosition = targetorigin;
        }
        actualtargetplane.transform.LookAt(transform.position);
        actualtargetplane.transform.Rotate(90, 0, 0);
        obs = GameObject.FindGameObjectsWithTag("QuickChatIcon");
        for (int i = 0; i < obs.Length; ++i) {
            obs[i].transform.LookAt(cam.transform.GetChild(0).position);
            obs[i].transform.Rotate(90, 0, 0);
        }
    }
    private void OnGUI()
    {
      //  GUI.Box(new Rect(Screen.width - 120, Screen.height * 0.5f, 120, 20), "" + fff);
      //  GUI.Box(new Rect(Screen.width - 120, Screen.height * 0.5f+20, 120, 20), "" + dff);
        if (whatcam)GUI.DrawTexture(new Rect(Screen.width * 0.5f - aimtexture.width * 0.5f, Screen.height * 0.5f - aimtexture.height * 0.5f, aimtexture.width, aimtexture.height), aimtexture);
    }
}
