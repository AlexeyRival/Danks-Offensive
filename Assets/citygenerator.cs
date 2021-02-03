using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class citygenerator : NetworkBehaviour
{
    public GameObject[] fragments;
    public GameObject corruptedThorn,collector;
    [SyncVar]
    private bool isGenerated=false,isEnemyGenerated=false;
    [SyncVar]
    public int seed;
    [Command]
    void CmdReset() {
        seed = Random.Range(0,int.MaxValue);
        isGenerated = false;
        isEnemyGenerated = false;
    }
    [Command]
    void CmdGenerate()
    {
        isGenerated = true;
        Random.seed = seed;
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("fragment").Length; ++i)
        {
            Destroy(GameObject.FindGameObjectsWithTag("fragment")[i]);
            NetworkServer.Destroy(GameObject.FindGameObjectsWithTag("fragment")[i]);
        }
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Enemy").Length; ++i)
        {
            Destroy(GameObject.FindGameObjectsWithTag("Enemy")[i]);
            NetworkServer.Destroy(GameObject.FindGameObjectsWithTag("Enemy")[i]);
        }
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("CorruptedCrystall").Length; ++i)
        {
            Destroy(GameObject.FindGameObjectsWithTag("CorruptedCrystall")[i]);
            NetworkServer.Destroy(GameObject.FindGameObjectsWithTag("CorruptedCrystall")[i]);
        }
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Matter").Length; ++i)
        {
            Destroy(GameObject.FindGameObjectsWithTag("Matter")[i]);
            NetworkServer.Destroy(GameObject.FindGameObjectsWithTag("Matter")[i]);
        }

        transform.position = new Vector3(0,0,25);

        for (int x = 0; x < 5; ++x)
        {
            for (int y = 0; y < 5; ++y)
            {

                if (x + y != 0)
                {
                    GameObject ob = Instantiate(fragments[Random.Range(0, fragments.Length)], transform.position, transform.rotation);
                    ob.transform.Rotate(0, 90 * Random.Range(0, 4), 0);
                    NetworkServer.Spawn(ob);
                }
                transform.Translate(-70, 0, 0);
            }
            transform.Translate(70 * 5, 0, 70);
        }
        collector.transform.position = new Vector3(Random.Range(0,5)*-70,collector.transform.position.y,Random.Range(0,5)*70);
        if (collector.GetComponent<collector>().level >= 15)
        {
            int xx;
            for (int i = 0; i < (collector.GetComponent<collector>().level < 30 ? (collector.GetComponent<collector>().level - 15) / 5 : 5); ++i)
            {
                xx = Random.Range(0, 5) * -70;
                GameObject obj = Instantiate(corruptedThorn, new Vector3(xx, collector.transform.position.y, Random.Range(xx == -70 ? 1 : 0, 5) * 70), transform.rotation);
                NetworkServer.Spawn(obj);
            }
        }
    }
    [Command]
    void CmdGenerateEnemy() {
        if (isEnemyGenerated||!isServer) { return; }
        isEnemyGenerated = true;
        int bossescount = 0;
        byte boss = 0;
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("fragment").Length; ++i) {
            //GameObject.FindGameObjectsWithTag("AISpawner")[i].GetComponent<NetworkIdentity>().AssignClientAuthority(GameObject.FindGameObjectWithTag("Player").GetComponent<NetworkIdentity>().connectionToClient);
            GameObject.FindGameObjectsWithTag("fragment")[i].GetComponent<enemyspawner>().SpawnNow(collector.GetComponent<collector>().level,Random.Range(0,25)==0&&bossescount++<5, collector.GetComponent<collector>().level%5==0&& collector.GetComponent<collector>().level>9&& Random.Range(0,5)==0&&(boss++==1));
        }
    }
    public override void OnStartServer()
    {
        seed = Random.Range(0,int.MaxValue);
        base.OnStartServer();
    }
    public void Update()
    {
        if (!isGenerated)
        {
            CmdGenerate();
        }
        else {
            if (!isEnemyGenerated) {
                CmdGenerateEnemy();   
            }
        }
    }
    public void ResetGeneration() {
        CmdReset();
    }
}
