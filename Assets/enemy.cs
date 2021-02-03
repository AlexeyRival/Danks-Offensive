using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class enemy : NetworkBehaviour
{
    public bool isBoss,isBigBoss;
    public GameObject shellPrefab,matterprefab;
    public Material deadMaterial;
    public Transform head, muzzle;
    public GameObject target,raytarget;
    public TextMesh hptext;



    [SyncVar]
    public int hp=300;
    public int timer=200;
    private float reloadtime = 0;
    public float maxspeed = 10f, maxreloadtime = 1f;
    private float speed = 0f;
    //ai
    private int aiphase=0;
    private bool alive=true;
    public GameObject walktarget;//сменить на private когда оттестирую
    private RaycastHit hit;
    private GameObject[] allMarkers = new GameObject[0];

    //эффекты
    public GameObject fireeffect, bluefireeffect, iceeffect;
    public GameObject[] shells;
    private GameObject effectobject;
    private byte effect;
    private short effecttimer;
    [SyncVar]
    private bool updateeffect;
    [SyncVar]
    private short resist=255;
    private bool updateeffectlocal;

    [Command]
    void CmdSetUpdateEffect(bool value)
    {
        updateeffect = value;
    }
    //кастомизация
    [SyncVar]
    private string bossname;
    [Command] //если они перестанут стрелять дело в этой строчке
    void CmdFire()
    {
        GameObject ob = Instantiate(shellPrefab, muzzle.transform.position, muzzle.transform.rotation);
        ob.transform.Rotate(0, 90, 0);
        ob.transform.Translate(-1.5f, 0, 0);
        ob.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 13, ForceMode.Impulse);
        Destroy(ob, 2.5f);
        NetworkServer.Spawn(ob);
    }
    private readonly string[] firstwords = { "Марципан", "Ириска", "Тыква", "Атлант", "Титан", "Огурчик", "Томат", "Фосфор", "Доминант", "Вектор", "Капелька", "Котлета", "Колбасня", "Стратостат", "Резистор", "Векториан","Орешек" };
    private readonly string[] zerowords = { "Электро", "Техно", "Гига", "Пента", "Окто", "Мега" };
    private readonly string[] bfirstwords = { "Мистер","Миссис","Господин","Госпожа","Товарищ","Сэр","Мсье","Мисс","Генерал","Сеньор","Сеньорита","Лорд","Брат","Сестра","Повелитель"};
    private readonly string[] bbfirstwords = { "Тиран", "Убийца", "Разрушитель", "Палач", "Жнец", "Аннигилятор" };
    private readonly string[] bsecondwords = { "Картошка", "Латук", "Бочка", "Сырник", "Дыня", "Тёрка", "Виолончель", "Крышка", "Таз", "Тромбон", "Киви", "Ночи", "Пара", "Наблюдатель", "Лётчик","Утка" };
    [Command]
    void CmdMakeBoss() {
        if (!isBigBoss)
        {
            bossname = Random.Range(0, 3) == 0 ? zerowords[Random.Range(0, zerowords.Length)] : firstwords[Random.Range(0, firstwords.Length)] + "-" + firstwords[Random.Range(0, firstwords.Length)];
        }
        else {
            bossname = (hp>4000? bbfirstwords[Random.Range(0, bbfirstwords.Length)] : bfirstwords[Random.Range(0, bfirstwords.Length)]) + " " + bsecondwords[Random.Range(0, bfirstwords.Length)];
            if(Random.Range(0,5)==2)resist = (byte)Random.Range(0,3);
        }
    }
    private GameObject[] GetNearMarkers() {
        List<GameObject> buffer = new List<GameObject>();
        for (int i = 0; i < allMarkers.Length; ++i) {
            if (Vector3.Distance(allMarkers[i].transform.position, transform.position) < 40f) {
                buffer.Add(allMarkers[i]);
            }
        }
        return buffer.ToArray();
    }
    private void Update()
    {
        //визуальные эффекты
        if (updateeffect != updateeffectlocal && updateeffect && !effectobject)
        {
            effectobject = Instantiate(effect == 0 ? fireeffect : (effect == 1 ? bluefireeffect : iceeffect), transform);
            updateeffectlocal = updateeffect;
        }
        if (updateeffectlocal != updateeffect && !updateeffect && effectobject)
        {
            Destroy(effectobject);
            updateeffectlocal = updateeffect;
        }


        if (allMarkers.Length == 0) { allMarkers = GameObject.FindGameObjectsWithTag("AIMarker"); }
        if (hp < 0)
        {
            if (alive)
            {
                Instantiate(matterprefab, transform.position, transform.rotation);
                if (isBigBoss) {
                    for (int i = 0; i < Random.Range(4, 8);++i)
                    {
                        Instantiate(matterprefab, transform.position, transform.rotation);
                    }
                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                    for (int i = 0; i < players.Length; ++i) {
                        players[i].GetComponent<player_tank>().AddMoney(5);
                    }
                }
                hptext.gameObject.SetActive(false);
                transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = deadMaterial;
                head.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = deadMaterial;
                muzzle.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = deadMaterial;
                GetComponent<AudioSource>().enabled=false;
                alive = false;
            }
        }
        hptext.text = (isBoss?(bossname+"\n"):"")+ hp.ToString();
        if (!isServer)
        {
            return;
        }

        if (effecttimer > 0)
        {
            effecttimer--;
            if (effecttimer % 100 == 0&&effect!=resist) switch (effect)
                {
                    case 0:
                        CmdDamage(15);
                        break;
                    case 1:
                        CmdDamage(25);
                        break;
                }
            if (effecttimer == 0||effect==resist)
            {
                effecttimer = 0;
                CmdSetUpdateEffect(false);
            }
        }

        if (hp < 0) {
            return; }
        if (isBoss && bossname==null) { 
            CmdMakeBoss();
            if(resist<3)shellPrefab = shells[resist];
        }
        if (target) {
            if (!(effecttimer > 0 && effect == 2))
            {
                head.transform.LookAt(new Vector3(target.transform.position.x, head.transform.position.y, target.transform.position.z));
                head.transform.Rotate(-90, 90, 90);
            }
            Debug.DrawRay(muzzle.transform.position, target.transform.position+new Vector3(0,1f,0)-muzzle.transform.position, Color.magenta,1f);
            if (reloadtime <= 0)
            {
                if (
     (Physics.Raycast(muzzle.transform.position, target.transform.position + new Vector3(0, 1f, 0) - muzzle.transform.position, out hit, Vector3.Distance(target.transform.position, muzzle.transform.position)) &&
     (hit.transform.gameObject == target.transform.gameObject || hit.transform.CompareTag("Player"))) ||
     !Physics.Raycast(muzzle.transform.position, target.transform.position + new Vector3(0, 1f, 0) - muzzle.transform.position, out hit, Vector3.Distance(target.transform.position, muzzle.transform.position) - 0.1f)
  )
                {
                    CmdFire();
                    reloadtime = maxreloadtime;
                }
                raytarget = Physics.Raycast(muzzle.transform.position, target.transform.position - muzzle.transform.position, out hit, Vector3.Distance(target.transform.position, muzzle.transform.position)) ? hit.transform.gameObject:raytarget;
            }
        }
        if (timer > 0) { --timer; }
        if (timer == 0) { target = null;timer--; }
        if (aiphase == 0&&target) {
            GameObject[] cmas= GetNearMarkers();
            float shortest=9999;
            int shortestid=-1;
            float bd;
            float td = Vector3.Distance(target.transform.position, transform.position);
            for (int i = 0; i < cmas.Length; ++i) {
                bd = Vector3.Distance(cmas[i].transform.position, transform.position);
                Vector3 bvec = new Vector3(transform.position.x,cmas[i].transform.position.y,transform.position.z);
                Debug.DrawRay(bvec, cmas[i].transform.position-bvec, Color.cyan); 
                //Physics.Raycast(bvec, cmas[i].transform.position-bvec, out hit, bd);//hit.transform==cmas[i].transform
                if (bd > (isBigBoss?4:3) && bd<shortest && !Physics.Raycast(bvec, cmas[i].transform.position - bvec, out hit, bd)&&Vector3.Distance(cmas[i].transform.position,target.transform.position)<td) {
                    shortest = bd;
                    shortestid = i;
                    Debug.DrawRay(bvec, cmas[i].transform.position - bvec, Color.blue,10f);
                }
            }
            if (shortestid != -1)
            {
                walktarget = cmas[shortestid];
                transform.LookAt(new Vector3(walktarget.transform.position.x, transform.position.y, walktarget.transform.position.z));
                transform.Rotate(0, 180, 0);
                aiphase = 1;
            }
        }
        if (aiphase == 1) {
            if (Vector3.Distance(walktarget.transform.position, transform.position) > (isBigBoss?3:2))
            {
                if (speed < maxspeed) { speed += 0.1f; }
                transform.LookAt(new Vector3(walktarget.transform.position.x, transform.position.y, walktarget.transform.position.z));
                transform.Rotate(0, 180, 0);
            }
            else { 
                aiphase = 0; 
            }
        }
        transform.Translate(0, 0, -speed * (effecttimer > 0 && effect == 2 ? 0.6f : 1f) * 0.02f);
        if (reloadtime > 0) { reloadtime -= 0.005f; }
        if (speed > 0) { speed -= 0.07f; }
        if (speed < 0) { speed += 0.07f; }
        if (Mathf.Abs(speed) < 0.01f) { speed = 0; }


    }
    [Command]
    void CmdDamage(int dmg)
    {
        hp -= dmg;
    }
    private void FindEnemy() {
        if (!target)
        {
            GameObject[] bufarr = GameObject.FindGameObjectsWithTag("Player");
            target = bufarr[0];
            float dist = Vector3.Distance(bufarr[0].transform.position, transform.position);
            for (int i = 1; i < bufarr.Length; ++i)
            {
                if (dist > Vector3.Distance(bufarr[i].transform.position, transform.position)) { dist = Vector3.Distance(bufarr[i].transform.position, transform.position); target = bufarr[i]; }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
            
        if (!isServer||Random.Range(0, 100) <= 0 + (isBigBoss ? 5 : 0)) { return; }
        if (collision.gameObject.name == "shell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) + (Random.Range(0, 10) == 0 ? 50 : 0));
            FindEnemy();
        }
        if (collision.gameObject.name == "heavyshell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) * 2 + (Random.Range(0, 10) == 0 ? 50 : 0) * 2);
            FindEnemy();
        }
        else
        if (collision.gameObject.name == "supershell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) * 4 + (Random.Range(0, 10) == 0 ? 50 : 0) * 4);
            FindEnemy();
        }
        else
        if (collision.gameObject.name == "exploshell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) * 4 + (Random.Range(0, 10) == 0 ? 50 : 0) * 4);
            FindEnemy();
        }
        else
        if (collision.gameObject.name == "fireshell(Clone)")
        {
            CmdDamage(Random.Range(40, 70) + (Random.Range(0, 10) == 0 ? 75 : 0));
            CmdSetUpdateEffect(true);
            effect = 0;
            effecttimer = 500;
            FindEnemy();
        }
        else
        if (collision.gameObject.name == "bluefireshell(Clone)")
        {
            CmdDamage(Random.Range(40, 80) + (Random.Range(0, 10) == 0 ? 90 : 0));
            CmdSetUpdateEffect(true);
            effect = 1;
            effecttimer = 700;
            FindEnemy();
        }
        else
        if (collision.gameObject.name == "iceshell(Clone)")
        {
            CmdDamage(Random.Range(40, 70) + (Random.Range(0, 10) == 0 ? 75 : 0));
            CmdSetUpdateEffect(true);
            effect = 2;
            effecttimer = 500;
            FindEnemy();
        }
        if (collision.gameObject.CompareTag("Player")) {
            CmdDamage((int)(collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude*10));
        }
    }
    public void Detect(GameObject obj) {
        if (!isServer) { return; }
        if (obj.CompareTag("Player")) {
            timer = 2000;
            if (!target) {
                target = obj;
            }
        }
    }
}
