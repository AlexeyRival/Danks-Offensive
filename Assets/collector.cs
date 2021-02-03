using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class collector : NetworkBehaviour
{
    private GameObject progressbar;
    public GameObject citygenerator;
    public AudioSource source;
    public AudioClip[] clips;
    private Text progressbartext;
    [SyncVar]
    public int matter=0;
    [SyncVar]
    public int maxmatter = 1000;
    [SyncVar]
    public int level = 0;
    public int locallevel=0;
    [SyncVar]
    private bool updateui,levelup;
    private byte timer = 0;

    [Command]
    void CmdGetMatter(int amout) {
        matter += amout;
        updateui = true;
        if (matter >= maxmatter) {
            matter -= maxmatter;
            maxmatter = (int)(maxmatter * 1.25f);
            levelup = true;
        }
    }
    [Command]
    void CmdLvlUp() {
        updateui = true;
        levelup = false;
        ++level;
        citygenerator.GetComponent<citygenerator>().ResetGeneration();
    }
    [Command]
    void CmdSetUIUpdate(bool value) {
        updateui = value;
    }
    private void FixedUpdate()
    {
        if (levelup) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            bool isAllPlayersNear=true;
            for (int i = 0; i < players.Length; ++i) {
                if (Vector3.Distance(players[i].transform.position, transform.position) > 25f) {
                    isAllPlayersNear = false;
                    break;
                }
            }
            if (isAllPlayersNear) {
                CmdLvlUp();
            }
        }
        if (locallevel < level&&timer==0) {
            locallevel = level;
            if (GameObject.Find("Boss_3(Clone)"))
            {
                source.clip = clips[2];
            }
            else if (GameObject.Find("Boss_2(Clone)"))
            {
                source.clip = clips[1];
            }
            else  { 
                source.clip = clips[0];
            }
            source.Play();
        }
        ++timer;
        if (updateui)
        {
            if (progressbar)
            {
                progressbar.transform.localScale = new Vector3(1f + (2f * matter / maxmatter), 1, 1);
                if (!levelup) { progressbartext.text = (100f * matter / maxmatter) + "%"; } else { progressbartext.text = "Проследуйте к коллектору"; }
            }
            else
            {
                progressbar = GameObject.Find("progressbar");
                progressbartext = GameObject.Find("progressbartext").GetComponent<Text>();
            }
            CmdSetUIUpdate(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isServer) {
            if (other.CompareTag("Player")) {
                CmdGetMatter(other.GetComponent<player_tank>().GetMatter());
            }
        }
    }
    public void GiveMatter(int amout) {
        CmdGetMatter(amout);
        print(amout);
    }
}
