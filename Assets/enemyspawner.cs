using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class enemyspawner : NetworkBehaviour
{
    public spawnersize size;
    public GameObject[] singles, smallSquads, mediumScuads, largeScuads, miniBosses, bosses;

    [SyncVar]
    private bool isGenerated=false;

    private bool generateNow=false;
    private int difficulty=0;
    private bool miniboss,boss;

    [Command]
    void CmdSpawnNow(GameObject point, int difficulitylevel, spawnersize size) {
        if (isGenerated||Random.Range(0,60)==0) { return; }
        isGenerated = true;
        GameObject ob=null;
        int index,sindex;
        if (boss)
        {
            boss = false;
            if (difficulitylevel < 100)
            {
                ob = Instantiate(bosses[Random.Range(0, (int)(bosses.Length * 0.01f * difficulitylevel))], point.transform.position, point.transform.rotation);
                NetworkServer.Spawn(ob);
            }
            else
            {
                for (int i = 0; i < difficulitylevel / 50; ++i)
                {//(int)(bosses.Length * 0.01f * (difficulitylevel % 100))
                    ob = Instantiate(bosses[Random.Range(0, bosses.Length)], point.transform.position, point.transform.rotation);
                    NetworkServer.Spawn(ob);
                }
                return;
            }
        }else
        if (miniboss)
        {
            miniboss = false;
            if (difficulitylevel < 30)
            {
                ob = Instantiate(miniBosses[Random.Range(0, (int)(miniBosses.Length / 30f * difficulitylevel))], point.transform.position, point.transform.rotation);
                NetworkServer.Spawn(ob);
            }
            else
            {
                for (int i = 0; i < difficulitylevel / 30; ++i)
                {
                    ob = Instantiate(miniBosses[Random.Range(0, (int)(miniBosses.Length /30f * (difficulitylevel % 30)))], point.transform.position, point.transform.rotation);
                    NetworkServer.Spawn(ob);
                }
                return;
            }
        }else
        switch (size)
        {
            case spawnersize.single:
                    if (difficulitylevel < 20)
                    {
                        ob = Instantiate(singles[Random.Range(0, (int)(singles.Length *0.05f * difficulitylevel))], point.transform.position, point.transform.rotation);
                        NetworkServer.Spawn(ob);
                    }
                    else
                    {
                        ob = Instantiate(singles[singles.Length-1], point.transform.position, point.transform.rotation);
                        NetworkServer.Spawn(ob);
                    }
                    break;
            case spawnersize.small:
                    if (difficulitylevel < 20)
                    {
                        index = Random.Range(0, (int)(smallSquads.Length * 0.05f * difficulitylevel));
                        sindex = Random.Range(0, (int)(singles.Length * 0.05f * difficulitylevel));
                    }
                    else
                    {
                        index = smallSquads.Length - 1;
                        sindex = Random.Range(0, singles.Length);
                    }
                    for (int i = 0; i < smallSquads[index].transform.childCount; ++i)
                    {
                        ob = smallSquads[index];
                        ob.transform.rotation=point.transform.rotation;
                        ob = Instantiate(singles[sindex], point.transform.position + ob.transform.GetChild(i).localPosition, ob.transform.GetChild(i).rotation);
                        NetworkServer.Spawn(ob);
                    }
                    break;
            case spawnersize.medium:
                    if (difficulitylevel < 20)
                    {
                        index = Random.Range(0, (int)(mediumScuads.Length * 0.05f * difficulitylevel));
                        sindex = Random.Range(0, (int)(singles.Length * 0.05f * difficulitylevel));
                    }
                    else
                    {
                        index = mediumScuads.Length - 1;
                        sindex = Random.Range(0, singles.Length);
                    }
                    for (int i = 0; i < mediumScuads[index].transform.childCount; ++i)
                    {
                        ob = mediumScuads[index];
                        ob.transform.rotation = point.transform.rotation;
                        ob = Instantiate(singles[sindex], point.transform.position + ob.transform.GetChild(i).localPosition, ob.transform.GetChild(i).rotation);
                        NetworkServer.Spawn(ob);
                    }
                    break;
            case spawnersize.large:
                    if (difficulitylevel < 20)
                    {
                        index = Random.Range(0, (int)(largeScuads.Length * 0.05f * difficulitylevel));
                        sindex = Random.Range(0, (int)(singles.Length * 0.05f * difficulitylevel));
                    }
                    else
                    {
                        index = largeScuads.Length - 1;
                        sindex = Random.Range(0, singles.Length);
                    }
                    for (int i = 0; i < largeScuads[index].transform.childCount; ++i)
                    {
                        ob = largeScuads[index];
                        ob.transform.rotation = point.transform.rotation;
                        ob = Instantiate(singles[sindex], point.transform.position + ob.transform.GetChild(i).localPosition, ob.transform.GetChild(i).rotation);
                        NetworkServer.Spawn(ob);
                    }
                    break;
        }
        Destroy(point);
      //  NetworkServer.Destroy(gameObject);
    }
    private void Update()
    {
        if (!isGenerated) {
            if (generateNow)
            {
                for (int i = 0; i < transform.childCount; ++i)
                {
                    if(transform.GetChild(i).CompareTag("AISpawner"))CmdSpawnNow(transform.GetChild(i).gameObject,difficulty, transform.GetChild(i).GetComponent<spawnerData>().size);
                }
            }
        }
    }
    public void SpawnNow(int difficulitylevel, bool miniboss, bool boss) {
        //  isGenerated = false;
        if (!isGenerated) {
            generateNow = true;
            difficulty = difficulitylevel;
            this.miniboss = miniboss;
            this.boss = boss;
        }
        //CmdSpawnNow(difficulitylevel, size, miniboss, boss);
    }

    public enum spawnersize : byte { 
        single,
        small,
        medium,
        large
    }
}
