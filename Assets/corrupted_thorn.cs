using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class corrupted_thorn : NetworkBehaviour
{
    public GameObject line;
    public GameObject enemy;
    private GameObject target;
    private bool shake=false;
    private short agressiontimer = 0,awaketimer=0,hp=10;
    private Vector3[] spawnpoints;
    private GameObject[] players;
    private Vector3 linepos;
    private int i;
    private void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        spawnpoints = new Vector3[25];
        int ir = 0;
        GameObject[] points = GameObject.FindGameObjectsWithTag("AIMarker");
        print(points.Length);
        for (i = 0; i < points.Length; ++i) {
            if (Vector3.Distance(points[i].transform.position, transform.position) < 60) {
                spawnpoints[ir] = points[i].transform.position;
                ir++;
                Debug.DrawRay(transform.position, points[i].transform.position-transform.position,Color.black,20f);
            }
            if (ir == spawnpoints.Length) break;
        }
        linepos = transform.position + new Vector3(0,4,0);
    }
    void Update()
    {
        if (!isServer) { return; }
        if(!target)for (i = 0; i < players.Length; ++i) {
            if (Vector3.Distance(players[i].transform.position, transform.position) < 40f) {
                    target = players[i];
                    agressiontimer = 1500;
                    break;
            }
        }
        if (target)
        {
            line.transform.position = (shake = !shake) ? linepos : (target.transform.position+new Vector3(0,1.5f,0));

            if (agressiontimer % 500 == 0)
            {
                target.GetComponent<player_tank>().CmdAddMatter(15);
                int r = Random.Range(0, spawnpoints.Length);
                GameObject en = Instantiate(enemy, spawnpoints[r], transform.rotation);
                en.GetComponent<enemy>().target = target;
                en.GetComponent<enemy>().timer = 1000;
                NetworkServer.Spawn(en);
                hp--;
            }
            if (Vector3.Distance(transform.position, target.transform.position) > 70f) {
                line.transform.position = linepos;
                target = null;
                agressiontimer = 0;
            }
        }
        if (agressiontimer >= 0)
        {
            --agressiontimer;
        }
        else {
            target = null;
            line.transform.position = linepos;
        }
        awaketimer++;
        if (awaketimer % 1000 == 0) { players = GameObject.FindGameObjectsWithTag("Player"); }
        if (awaketimer > 16000) { awaketimer = 0; }
        if (hp <= 0) { Destroy(gameObject); }
    }
}
