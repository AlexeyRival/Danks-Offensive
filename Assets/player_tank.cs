
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class player_tank : NetworkBehaviour
{
    public customNetworkHUD manager;


    public Camera cam0, cam1;
    public GameObject muzzle, shellPrefab, heavyShellPrefab, matterPrefab, shellExplosionPrefab, comradeprefab, healprefab, explosionshellprefab, defenderprefab;
    public TextMesh hptext,respawntext;
    public float maxspeed=10f,maxreloadtime=1f,magmaxreloadtime=0f;
    public byte maxammo = 3;
    public player_camera player_Camera;
    public AudioSource audio;
    public Texture2D damageTexture;
    public Texture2D[] effectTexture;
    private float reloadtime=0;
    private float magreloadtime=0;
    private bool cam = false;
    private float speed = 0f, acceleration = 1f, rotationspeed=1f;
    [SyncVar]
    public bool isElite;
    [SyncVar]
    public string nick;
    [SyncVar]
    public int hp=1000;
    private int maxhp, prevhp;
    private string baserespawntext;
    private player_tank[] players;
    //уууУУуууу
    [SyncVar]
    public int matter=0;
    public int maxmatter = 170, localmatter;
    public float respawntime;
    [SyncVar]
    public bool disconnected=false;
    //прокачка
    [SyncVar]
    public int upgradematter,upgradematterneed=150;
    [SyncVar]
    public int level = 1;
    public bool upgradenow=false;
    private readonly string[,] upgrades ={{"СКОРОСТЬ","ХП" },{ "БАШНЯ БРОНЯ","БАШНЯ ВРРРР"},{ "ПУШКА БАБАХ","ПУШКА ЧИК-ЧИК"},{ "ДВЕ ПУШКИ ОФИГЕТЬ","ПУШКА ЧИК-ЧИК"},{ "ПУШКА БАХ","ПУШКА ПУ-ПИУ-ПИУ"},{"ОГНИЩЕ","перезаряяядка" },{"ЛЁД","перезарядка?" },{ "ГОЛУБОЙ ОГНИЩЕ", "Скорость или +патрон" },{"БрОнЯ", "Скорость или +патрон" } };
    [SyncVar]
    private byte weapontype=0;
    //визуализация оной
    public GameObject[] towers, muzzles, bodys;
    public GameObject v_tower, v_muzzle, v_body;
    public GameObject s_tower, s_muzzle, s_body;
    [SyncVar]
    private byte SomethingChanged=0;
    private byte LocalChanged=0;
    [SyncVar]
    private int towernum, muzzlenum, bodynum;

    //синхронизация хп с интерфейсом
    private byte hpsync,hpsynclocal;
    //иконки прокачки
    public Sprite[] UpgradePicsA,UpgradePicsB;
    //особенности пушек
    private byte ammo = 3;
    [SyncVar]
    private bool IsDouble = false, isCassete=false;

    //эффекты и всё что с ними связано
    public GameObject fireshell, bluefireshell, iceshell;
    public GameObject fireeffect, bluefireeffect, iceeffect;
    private GameObject effectobject;
    private byte effect;
    private short effecttimer;
    [SyncVar]
    private bool updateeffect;
    private bool updateeffectlocal;

    //возвращение в исходное положение
    private short resetTimer = 0;
    //интерфейс
    private GameObject hpbar,upgradebar,matterbar,reloadbar,magreloadbar;
    private Text hpbartext, upgradebartext, matterbartext;
    private bool updateui=true,updatefireui=false,updateenemyui=false;
    private GameObject upgradeparent, leftup, rightup;
    private GameObject comradebase;

    //список задач
    private GameObject enemyicon, minibossicon, bossicon, crystallicon;
    private byte uitimer;
    private GameObject[] allenemys,shortarr;
    private byte allcrystals;

    //деньги
    private int money=0;
    [SyncVar]
    private int bossmoney=0;

    //предметы
    private int[] items= new int[6];
    private readonly int[] itemMaxTimers = {1200,1200,600,1200,100,8000};//нитро, овердрайв, хилка, комхилка, разрывные колья, баррикады
    private int[] itemtimers = new int[6];
    private GameObject[] itemcolors=new GameObject[6], itempowers = new GameObject[6];
    private Text[] itemcounts = new Text[6];

    //квикчат
    public GameObject QC_Attack, QC_Defend, QC_SOS, QC_Attention;
    private GameObject QCUIL, QCUIR;
    private bool LeftQC, RightQC;
    private short qcTimer;

    //выход
    private int exittimer = 300;
    private bool isTheyAllDie = false;
    
    void Start()
    {
        maxhp = hp;
        prevhp = hp;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        baserespawntext = respawntext.text;
        respawntext.gameObject.SetActive(false);
        for (int i = 0; i < 10; ++i)
        {
            GameObject.Find("bullet_" + i).GetComponent<Image>().enabled = false;
        }
    }
    int contrlevel;
    private void LeaveMatch(byte exitid) {
        if (isServer) {
            CmdDisconnect();
        }
        {
            StreamWriter file = new StreamWriter(Application.dataPath + "/logs.dxo");
            file.WriteLine(contrlevel);
            file.WriteLine(money + bossmoney);
            file.WriteLine(exitid);
            for (int i = 0; i < items.Length; ++i)
            {
                file.WriteLine(items[i]);
            }
            file.Close();
        }
        GameObject.Find("networkManager").GetComponent<customNetworkHUD>().Disconnect();
        if (isServer) { 
        GameObject.Find("networkManager").GetComponent<customNetworkHUD>().StopServer();
        }
        Application.LoadLevel(1);
    }
    [Command]
    void CmdFire()
    {
        GameObject ob;
        if (weapontype < 4) { 
            ob = Instantiate((weapontype != 1 && weapontype != 3) || (shellPrefab.name != "shell") ? shellPrefab : heavyShellPrefab, muzzle.transform.position, muzzle.transform.rotation); 
        } else {
            ob = Instantiate(weapontype==4 ? shellPrefab : weapontype==5?iceshell:weapontype==6?bluefireshell:weapontype==7?heavyShellPrefab: weapontype == 8 ? fireshell :explosionshellprefab, muzzle.transform.position, muzzle.transform.rotation);
        }
        ob.transform.Rotate(0, 90, 0);
        ob.transform.Translate(-1.5f, 0, 0);
        ob.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 13, ForceMode.Impulse);
        Destroy(ob,2.5f);
        NetworkServer.Spawn(ob);
        if (IsDouble)
        {
            ob = Instantiate(weapontype == 8 ? fireshell : weapontype == 5 ? iceshell : weapontype == 6 ? bluefireshell : weapontype == 7||weapontype==3 ? heavyShellPrefab : explosionshellprefab, muzzle.transform.position, muzzle.transform.rotation);
            ob.transform.Rotate(0, 90, 0);
            ob.transform.Translate(-1.5f, 0, 0);
            ob.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 13, ForceMode.Impulse);
            Destroy(ob, 2.5f);
        NetworkServer.Spawn(ob);
        }
    }
    [Command]
    void CmdQuickChat(byte id) {
        GameObject ob=gameObject;
        switch (id) {
            case 0: ob = Instantiate(QC_Attack,transform); break;
            case 1: ob = Instantiate(QC_Defend, transform); break;
            case 2: ob = Instantiate(QC_SOS, transform); break;
            case 3: ob = Instantiate(QC_Attention, transform); break;
        }
        NetworkServer.Spawn(ob);
        Destroy(ob, 5f);
    }

    [Command]
    void CmdDisconnect()
    {
        disconnected = true;
    }
    [Command]
    void CmdSpawnHeal()
    {
        GameObject ob = Instantiate(healprefab, transform.position + new Vector3(0, 1f, 0), Quaternion.identity); 
        NetworkServer.Spawn(ob); 
        Destroy(ob, 20f);
    }
    [Command]
    void CmdSpawnDefender()
    {
        GameObject ob = Instantiate(defenderprefab, transform.position + new Vector3(0, 0, 0), transform.rotation); 
        NetworkServer.Spawn(ob); 
        Destroy(ob, 133f);
    }
    void GetListOfPlayers() {
        players = FindObjectsOfType<player_tank>();
    }
    [Command]
    void CmdDestroy() {
        NetworkServer.UnSpawn(gameObject);
        manager.Players.Remove(this);
        NetworkServer.Destroy(gameObject);
    }
    [Command]
    void CmdDamage(int dmg) {
        hp -= dmg;
    }
    [Command]
    void CmdSetNick(string nick) {
        this.nick = nick;
    }
    [Command]
    public void CmdAddMatter(int amout) {
        matter += amout;
        if (matter < 0) { matter=0; }
        if (matter > maxmatter) { matter = maxmatter; }
    }
    [Command]
    void CmdRespawn(int maxhp)
    {
        hp = maxhp;
    }
    [Command]
    void CmdChangeVisual(int towernum, int bodynum, int muzzlenum) {
        if (towernum != 0) this.towernum = towernum;
        if (bodynum != 0) this.bodynum = bodynum;
        if (muzzlenum != 0) this.muzzlenum = muzzlenum;
        SomethingChanged++;
    }
    [Command]
    void CmdSetWeaponType(byte weapontype) {
        this.weapontype = weapontype;
        if (weapontype == 3) {
            IsDouble = true;
        }
        if (weapontype == 4 || weapontype == 7) {
            isCassete = true;
        }
    }
    [Command]
    void CmdAddUpgradeMatter(int amout) {
        upgradematter += amout;
    }
    [Command]
    void CmdLvlUp() {
        upgradematterneed += 50;
    }
    [Command]
    void CmdSetUpdateEffect(bool value) {
        updateeffect = value;
    }
    [Command]
    void CmdSetElite() {
        isElite = true;
        maxmatter = 250;
        maxhp = 2000;
        hp = 2000;
        prevhp = 2000;
        level = 10;
        SomethingChanged = 1;
    }
    public int GetMatter() {
        if (upgradenow) { return 0; }
        if (matter <= 0) { return 0; }
        int bufint = matter;
        CmdAddMatter(-matter);
        CmdAddUpgradeMatter(bufint);
        return bufint;
    }

    private bool isMove;
    private Vector3 lastpoint=new Vector3(0,0,0);


    //компенсация лагов
    private byte lagtimer = 0;
    void Update()
    {
        //визуальные эффекты
        if (updateeffect!=updateeffectlocal&&updateeffect&&!effectobject) {
            effectobject = Instantiate(effect==0?fireeffect:(effect==1?bluefireeffect:iceeffect),transform);
            effectobject.transform.Translate(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            updateeffectlocal=updateeffect;
        }
        if (updateeffectlocal!=updateeffect&&!updateeffect&& effectobject) {
            Destroy(effectobject);
            updateeffectlocal=updateeffect;
        }

        //визуализация изменений
        if (SomethingChanged != LocalChanged) {
            LocalChanged = SomethingChanged;
            GameObject vb,vm,vt;
            if (!isElite)
            {
                vb = Instantiate(bodys[bodynum], v_body.transform.position, v_body.transform.rotation, v_body.transform.parent);
                vm = Instantiate(muzzles[muzzlenum], v_muzzle.transform.position, v_muzzle.transform.rotation, v_muzzle.transform.parent);
                vt = Instantiate(towers[towernum], v_tower.transform.position, v_tower.transform.rotation, v_tower.transform.parent);
            }
            else
            {
                vb = Instantiate(s_body, v_body.transform.position, v_body.transform.rotation, v_body.transform.parent);
                vm = Instantiate(s_muzzle, v_muzzle.transform.position, v_muzzle.transform.rotation, v_muzzle.transform.parent);
                vt = Instantiate(s_tower, v_tower.transform.position, v_tower.transform.rotation, v_tower.transform.parent);
            }
            Destroy(v_body);
            Destroy(v_muzzle);
            Destroy(v_tower);
            v_body = vb;
            v_muzzle = vm;
            v_tower = vt;
        }

        if (hp < 0 && respawntime <= 0) {
            respawntime = 9f; if (isLocalPlayer) { 
                respawntext.gameObject.SetActive(true);
            }
        }
        hptext.text = nick + "\n" + hp.ToString();

        //скорость танка и его звук

        audio.pitch = 0.85f + 0.01f * Vector3.Distance(transform.position,lastpoint);
        if (!isLocalPlayer)
        {
            //transform.position = (lastpoint*.25f + transform.position*.75f);
            if (lagtimer == 3) { 
                transform.Translate((transform.position - lastpoint) * 0.5f); lagtimer = 0; 
            } else { 
                ++lagtimer; 
            }
        }
        lastpoint = transform.position;


        if (!isLocalPlayer) { cam0.gameObject.SetActive(false); cam1.gameObject.SetActive(false); player_Camera.actualtargetplane.SetActive(false); player_Camera.enabled = false; return; }
        if (transform.rotation.x > 0.3f || transform.rotation.x < -0.3f || transform.rotation.z > 0.7f || transform.rotation.z < -0.7f) { resetTimer--; } else { resetTimer = 250; }
        if (resetTimer <= -1) {
            resetTimer = 250;
            transform.rotation = Quaternion.identity;
        }
        if (localmatter != matter) {
            localmatter = matter;
            updateui = true;
        }
        //красивость
        cam0.fieldOfView = 60 + Mathf.Abs(speed * (1 + 0.001f * itemtimers[0]));
        isMove = false;
        hptext.gameObject.SetActive(false);
        GetListOfPlayers();
        if (respawntime <= 0)
        {
            if (Input.GetKey(KeyCode.A)) { transform.Rotate(0, Mathf.Sign(speed) * rotationspeed*(effecttimer>0&&effect==2?0.6f:1f) * -0.67f, 0); player_Camera.Rot(-Mathf.Sign(speed) * rotationspeed * -0.67f); player_camera.range += 0.1f; }
            if (Input.GetKey(KeyCode.D)) { transform.Rotate(0, Mathf.Sign(speed) * rotationspeed * (effecttimer > 0 && effect == 2 ? 0.6f : 1f) * 0.67f, 0); player_Camera.Rot(-Mathf.Sign(speed) * rotationspeed * 0.67f); player_camera.range += 0.1f; }
            if (Input.GetKey(KeyCode.S)) { if (speed > -maxspeed) { speed -= acceleration * (effecttimer > 0 && effect == 2 ? 0.6f : 1f) * 6.5f * Time.deltaTime; isMove = true; } player_camera.range += 0.15f; player_camera.range += 0.15f; }
            if (Input.GetKey(KeyCode.W)) { if (speed < maxspeed) { speed += acceleration * (effecttimer > 0 && effect == 2 ? 0.6f : 1f) * 6.5f * Time.deltaTime; isMove = true; } player_camera.range += 0.15f; }
            if (Input.GetKey(KeyCode.R) && isCassete) { ammo = 0; magreloadtime = magmaxreloadtime; updatefireui = true; }
            if (Input.GetKey(KeyCode.Escape)) { exittimer--; if (exittimer <= 0) { LeaveMatch(0); } } else { exittimer = 300; }
            if (Input.GetKey(KeyCode.Space)) { speed *= 0.9f; }
            if (Input.GetKey(KeyCode.KeypadMinus)) { CmdDestroy(); }
            if (Input.GetKey(KeyCode.KeypadPlus)) { CmdAddMatter(upgradematterneed); GetMatter(); }
            if (!Input.GetKey(KeyCode.LeftControl)) {
                if (Input.GetKey(KeyCode.Alpha1) && itemtimers[0] == 0 && items[0] > 0) { itemtimers[0] = itemMaxTimers[0]; --items[0]; itemcounts[0].text = items[0].ToString(); }
                if (Input.GetKey(KeyCode.Alpha2) && itemtimers[1] == 0 && items[1] > 0) { itemtimers[1] = itemMaxTimers[1]; --items[1]; itemcounts[1].text = items[1].ToString(); }
                if (Input.GetKey(KeyCode.Alpha3) && itemtimers[2] == 0 && items[2] > 0) { itemtimers[2] = itemMaxTimers[2]; --items[2]; itemcounts[2].text = items[2].ToString();CmdDamage(-600); }
                if (Input.GetKey(KeyCode.Alpha4) && itemtimers[3] == 0 && items[3] > 0) { itemtimers[3] = itemMaxTimers[3]; --items[3]; itemcounts[3].text = items[3].ToString(); CmdSpawnHeal(); }
                if (Input.GetKey(KeyCode.Alpha5) && itemtimers[4] == 0 && items[4] > 0) { itemtimers[4] = itemMaxTimers[4]; --items[4]; itemcounts[4].text = items[4].ToString();CmdSetWeaponType(9); }
                if (Input.GetKey(KeyCode.Alpha6) && itemtimers[5] == 0 && items[5] > 0) { itemtimers[5] = itemMaxTimers[5]; --items[5]; itemcounts[5].text = items[5].ToString(); CmdSpawnDefender(); }
            }
            if (Input.GetKeyDown(KeyCode.Q)) {
                if (LeftQC)
                {
                    CmdQuickChat(0);
                    qcTimer = 300;
                    LeftQC = false;
                    QCUIL.SetActive(false);
                }
                else if (RightQC)
                {
                    CmdQuickChat(2);
                    qcTimer = 300;
                    RightQC = false;
                    QCUIR.SetActive(false);
                }
                else
                { LeftQC = true;QCUIL.SetActive(true); }
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                if (LeftQC)
                {
                    CmdQuickChat(1);
                    qcTimer = 300;
                    LeftQC = false; 
                    QCUIL.SetActive(false);
                }
                else if (RightQC)
                {
                    CmdQuickChat(3);
                    qcTimer = 300;
                    RightQC = false;
                    QCUIR.SetActive(false);
                }
                else
                { RightQC = true; QCUIR.SetActive(true); }
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                cam = !cam;
                cam0.enabled = !cam;
                cam1.enabled = cam;
                player_camera.whatcam = cam;
                player_Camera.actualtargetplane.SetActive(!cam);
            }
            if (Input.GetKey(KeyCode.Mouse0) && reloadtime <= 0 && magreloadtime <= 0 && ammo > 0)
            {
                CmdFire();
                reloadtime = maxreloadtime;
                if (isCassete) {
                    ammo--;
                    if (ammo == 0) magreloadtime = magmaxreloadtime;
                }
                GameObject ob = Instantiate(shellExplosionPrefab, muzzle.transform.position, muzzle.transform.rotation);
                ob.transform.Translate(0, 0, 1.5f);
                Destroy(ob, 3f);
                updatefireui = true;
                updateenemyui = true;
            }
            transform.Translate(-speed * 0.015f*(1+0.001f*itemtimers[0]), 0, 0);
            if (reloadtime > 0)
            {
                reloadtime -= 0.005f + (0.01f * itemtimers[1] / itemMaxTimers[1]);
                reloadbar.transform.rotation = new Quaternion(0, 0, 0.6f * reloadtime / maxreloadtime, reloadbar.transform.rotation.w);
            }
            if (magreloadtime > 0) {
                magreloadbar.transform.rotation = new Quaternion(0, 0, 0.3f * magreloadtime / magmaxreloadtime, magreloadbar.transform.rotation.w);
                magreloadtime -= 0.005f+(0.01f*itemtimers[1]/itemMaxTimers[1]);
                if (magreloadtime <= 0 && magreloadtime > -1f) { magreloadtime = -1f; ammo = maxammo; updatefireui = true; }
            }
            if (speed > 0) { speed -= 0.07f; }
            if (speed < 0) { speed += 0.07f; }
            if (Mathf.Abs(speed) < 0.04f && !isMove) { speed = 0; }

            //чек перед прокачкой
            if (upgradematter >= upgradematterneed && !upgradenow)
            {
                if (level < 6)
                {
                    if (level < 4) { ++level; }
                    else
                    {
                        level = 4 + weapontype;
                    }
                    upgradenow = true;
                    CmdAddUpgradeMatter(-upgradematterneed);
                    CmdLvlUp();
                }
                else {
                    if (level < 15) { level++; upgradenow = true; } else { money += 2; }
                    CmdAddUpgradeMatter(-upgradematterneed);
                    CmdLvlUp();
                }
                if (upgradenow)
                {
                    leftup.GetComponent<Image>().sprite = UpgradePicsA[level - 2];
                    rightup.GetComponent<Image>().sprite = UpgradePicsB[level - 2];
                    upgradeparent.SetActive(true);
                }
            }

            //прокачка
            if (upgradenow)
            {
                if (Input.GetKey(KeyCode.Alpha1)&& Input.GetKey(KeyCode.LeftControl))
                {
                    upgradenow = false;
                    if (level == 2)
                    {
                        maxspeed *= 1.25f;
                        acceleration *= 1.1f;
                        rotationspeed *= 1.1f;
                        CmdChangeVisual(0, 2, 0);
                    }
                    if (level == 3)
                    {
                        maxhp = (int)(maxhp * 1.15f);
                        hp = (int)(hp * 1.15f);
                        player_Camera.towerspeed *= 0.75f;
                        CmdChangeVisual(2, 0, 0);
                    }
                    if (level == 4)
                    {
                        CmdSetWeaponType(1);
                        CmdChangeVisual(0, 0, 1);
                    }
                    if (level == 5)
                    {
                        CmdSetWeaponType(3);
                        CmdChangeVisual(0, 0, 2);
                    }
                    if (level == 6)
                    {
                        CmdSetWeaponType(1);
                    }
                    if (level == 7)
                    {
                        CmdSetWeaponType(4);
                    }
                    if (level == 8)
                    {
                        CmdSetWeaponType(5);
                    }
                    if (level == 9)
                    {
                        CmdSetWeaponType(6);
                    }
                    if (level == 10)
                    {
                        maxhp = (int)(maxhp * 1.10f);
                        prevhp = (int)(prevhp * 1.1f);
                        CmdDamage(-(int)(hp*0.1f));
                    }
                    if (level > 10) {
                        switch (Random.Range(0, 6))
                        {
                            case 0:
                                maxhp = (int)(maxhp * 1.10f);
                                prevhp = (int)(prevhp * 1.1f);
                                CmdDamage(-(int)(hp * 0.1f));
                                break;
                            case 1:
                                maxspeed *= 1.25f;
                                acceleration *= 1.1f;
                                rotationspeed *= 1.1f;
                                break;
                            case 2:
                                maxhp = (int)(maxhp * 1.25f);
                                prevhp = (int)(prevhp * 1.25f);
                                CmdDamage(-(int)(hp * 0.25f));
                                break;
                            case 3:
                                maxreloadtime *= 0.85f;
                                break;
                            case 4:
                                maxreloadtime *= 0.75f;
                                magmaxreloadtime *= 0.75f;
                                break;
                            case 5:
                                if (magmaxreloadtime == 0)
                                {
                                    maxspeed *= 1.1f;
                                }
                                else
                                {
                                    maxammo++;
                                    magreloadtime = 0.1f;
                                }
                                break;
                        }
                    }
                }
                else
                if (Input.GetKey(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftControl))
                {
                    upgradenow = false;
                    if (level == 2)
                    {
                        maxhp = (int)(maxhp * 1.25f);
                        prevhp = (int)(prevhp * 1.25f);
                        CmdDamage(-(int)(hp * 0.25f));
                        maxspeed *= 0.75f;
                        acceleration *= 0.9f;
                        rotationspeed *= 0.8f;
                        CmdChangeVisual(0, 1, 0);
                    }
                    if (level == 3)
                    {
                        player_Camera.towerspeed *= 1.5f;
                        CmdChangeVisual(1, 0, 0);
                    }
                    if (level == 4)
                    {
                        CmdSetWeaponType(2);
                        CmdChangeVisual(0, 0, 3);
                    }
                    if (level == 5)
                    {
                        maxreloadtime *= 0.85f;
                    }
                    if (level == 6)
                    {
                        CmdSetWeaponType(4);
                        magmaxreloadtime = 1f;
                        maxreloadtime *= 0.15f;
                        updatefireui = true;
                    }
                    if (level == 7)
                    {
                        maxreloadtime *= 0.75f;
                        magmaxreloadtime *= 0.75f;
                    }
                    if (level == 8)
                    {
                        maxreloadtime *= 0.75f;
                        magmaxreloadtime *= 0.75f;
                    }
                    if (level == 9)
                    {
                        if (magmaxreloadtime == 0)
                        {
                            maxspeed *= 1.1f;
                        }
                        else
                        {
                            maxammo++;
                            magreloadtime = 0.1f;
                        }
                    }
                    if (level == 10)
                    {
                        if (magmaxreloadtime == 0)
                        {
                            maxspeed *= 1.1f;
                        }
                        else
                        {
                            maxammo++;
                            magreloadtime = 0.1f;
                        }
                    }
                    if (level > 10)
                    {
                        switch (Random.Range(0, 6))
                        {
                            case 0:
                                maxhp = (int)(maxhp * 1.10f);
                                prevhp = (int)(prevhp * 1.1f);
                                CmdDamage(-(int)(hp * 0.1f));
                                break;
                            case 1:
                                maxspeed *= 1.25f;
                                acceleration *= 1.1f;
                                rotationspeed *= 1.1f;
                                break;
                            case 2:
                                maxhp = (int)(maxhp * 1.25f);
                                prevhp = (int)(prevhp * 1.25f);
                                CmdDamage(-(int)(hp * 0.25f));
                                break;
                            case 3:
                                maxreloadtime *= 0.85f;
                                break;
                            case 4:
                                maxreloadtime *= 0.75f;
                                magmaxreloadtime *= 0.75f;
                                break;
                            case 5:
                                if (magmaxreloadtime == 0)
                                {
                                    maxspeed *= 1.1f;
                                }
                                else
                                {
                                    maxammo++;
                                    magreloadtime = 0.1f;
                                }
                                break;
                        }
                    }
                }
                if (!upgradenow) { 
                    upgradeparent.SetActive(false);
                }
            }
        }
        else {
            respawntime -= 0.05f;
            if (!isTheyAllDie)
            {
                isTheyAllDie = true;
                for (int i = 0; i < players.Length; ++i)
                {
                    if (players[i].hp > 0)
                    {
                        isTheyAllDie = false;
                    }
                }
            }
            else {
                baserespawntext = "Все машины выведены из строя!\nСекунд до эвакуации: ";
            }
            respawntext.text = baserespawntext + (Mathf.Floor(respawntime * 10) * 0.1f);
            if (respawntime <= 0)
            {
                if (!isTheyAllDie)
                {
                    CmdRespawn(maxhp);
                    for (int i = 0; i < matter / 20; ++i)
                    {
                        Instantiate(matterPrefab, transform.position, transform.rotation);
                    }
                    transform.position = GameObject.Find("networkManager").transform.position;
                    transform.rotation = Quaternion.identity;
                    respawntext.gameObject.SetActive(false);
                    respawntext.text = baserespawntext;
                    hp = maxhp;
                    prevhp = maxhp;
                    matter = 0;
                    effecttimer = 1;
                    hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
                    hpbartext.text = hp.ToString();
                }
                else {
                    LeaveMatch(1);
                }
            }
        }
        if (effecttimer > 0) {
            effecttimer--;
            if (effecttimer % 100 == 0) switch (effect)
                {
                    case 0:
                        CmdDamage(15);
                        hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
                        hpbartext.text = hp.ToString();
                        break;
                    case 1:
                        CmdDamage(25);
                        hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
                        hpbartext.text = hp.ToString();
                        break;
                }
            if (effecttimer == 0) {
                CmdSetUpdateEffect(false);
            }
        }
        if (uitimer > 0)
        {
            uitimer--;
        }
        else { uitimer = 200;updateenemyui = true;
            for (int i = 0; i < comradebase.transform.childCount; ++i) {
                GameObject ob = comradebase.transform.GetChild(i).gameObject;
                ob.transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                ob.transform.GetChild(1).GetComponent<Text>().text = players[i].hp.ToString();
                ob.transform.GetChild(3).GetComponent<Text>().text = players[i].matter.ToString();
                ob.transform.GetChild(5).GetComponent<Text>().text = players[i].level.ToString();
            }
        }
        if (updateui) {
            upgradebar.transform.localScale = new Vector3(1f + (1.9f * upgradematter / upgradematterneed), 1, 1);
            upgradebartext.text = upgradematter + "/" + upgradematterneed;
            matterbar.transform.localScale = new Vector3(1f + (1.9f * matter / maxmatter), 1, 1);
            matterbartext.text = matter + "/" + maxmatter;
            matterbar.transform.localScale = new Vector3(1f + (1.9f * matter / maxmatter), 1, 1);
            matterbartext.text = matter + "/" + maxmatter;
        }
        if (updatefireui) {
            if (magmaxreloadtime > 0)
            {
                for (int i = 0; i < 10; ++i)
                {
                    GameObject.Find("bullet_" + i).GetComponent<Image>().enabled = (i < ammo);
                }
            }
        }
        if (updateenemyui) {
            if (!enemyicon)
            {
                enemyicon = GameObject.Find("enemyicon");
                minibossicon = GameObject.Find("minibossicon");
                bossicon = GameObject.Find("bossicon");
                crystallicon = GameObject.Find("crystallicon");
                allenemys = new GameObject[0];
                allcrystals = 0;
            }
            else {
                shortarr = GameObject.FindGameObjectsWithTag("Enemy");
                {
                    allenemys = shortarr;
                    int enms = 0, mbsss = 0, bsss = 0; 
                    allcrystals = (byte)GameObject.FindGameObjectsWithTag("CorruptedCrystall").Length;
                    for (int i = 0; i < allenemys.Length; ++i)
                    {
                        if (allenemys[i].GetComponent<enemy>().hp >= 0)
                        {
                            if (allenemys[i].GetComponent<enemy>().isBoss)
                            {
                                if (allenemys[i].GetComponent<enemy>().isBigBoss)
                                {
                                    ++bsss;
                                }
                                else
                                {
                                    ++mbsss;
                                }
                            }
                            else { ++enms; }
                        }
                    }
                    enemyicon.transform.GetChild(0).GetComponent<Text>().text = enms.ToString();
                    if (mbsss != 0)
                    {
                        minibossicon.SetActive(true);
                        minibossicon.transform.GetChild(0).GetComponent<Text>().text = mbsss.ToString();
                    }
                    else
                    {
                        minibossicon.SetActive(false);
                    }
                    if (bsss != 0)
                    {
                        bossicon.SetActive(true);
                        bossicon.transform.GetChild(0).GetComponent<Text>().text = bsss.ToString();
                    }
                    else
                    {
                        bossicon.SetActive(false);
                    }
                    if (allcrystals != 0)
                    {
                        crystallicon.SetActive(true);
                        crystallicon.transform.GetChild(0).GetComponent<Text>().text = allcrystals.ToString();
                    }
                    else
                    {
                        crystallicon.SetActive(false);
                    }
                }
            }
        }
        if (comradebase.transform.childCount != players.Length) {
            for (int i = 0; i < comradebase.transform.childCount; ++i) {
                Destroy(comradebase.transform.GetChild(i).gameObject);
            }
            for(int i = 0; i < players.Length; ++i) {
                GameObject ob = Instantiate(comradeprefab,comradebase.transform.position,comradebase.transform.rotation, comradebase.transform);
                ob.transform.Translate(0,-20*i,0);
                ob.transform.GetChild(0).GetComponent<Text>().text = players[i].nick;
                ob.transform.GetChild(1).GetComponent<Text>().text = players[i].hp.ToString();
                ob.transform.GetChild(3).GetComponent<Text>().text = players[i].matter.ToString();
                ob.transform.GetChild(5).GetComponent<Text>().text = players[i].level.ToString();
            }
        }
        for (int i = 0; i < items.Length; ++i) {
            if (itemtimers[i] > 0) { 
                --itemtimers[i];
                itempowers[i].transform.localScale = new Vector3(1,1+(2f * itemtimers[i]/itemMaxTimers[i]),1);
            }
        }
        if (qcTimer > 0) {
            --qcTimer;
        }
        if (prevhp > hp)
        {
            --prevhp;
            if (hpbartext.text != hp.ToString()) {
                hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
                hpbartext.text = hp.ToString();
            }
        }
        contrlevel = GameObject.Find("collector").GetComponent<collector>().level;
        if (Application.targetFrameRate != 60) { Application.targetFrameRate = 60; }
    }
    private void OnDestroy()
    {
        if (isLocalPlayer) {
            LeaveMatch(0);
        }
    }
    public void AddMoney(int count) {
        CmdAddMoney(count);
    }
    [Command]
    public void CmdAddMoney(int count) {
        bossmoney += count;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer||Random.Range(0,100)==0) { return; }
        if (collision.gameObject.name == "shell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) + (Random.Range(0, 10) == 0 ? 50 : 0));
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
        }
        if (collision.gameObject.name == "heavyshell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) * 2 + (Random.Range(0, 10) == 0 ? 50 : 0) * 2);
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
        }
        else
        if (collision.gameObject.name == "supershell(Clone)")
        {
            CmdDamage(Random.Range(20, 50) * 4 + (Random.Range(0, 10) == 0 ? 50 : 0) * 4);
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
        }
        else
        if (collision.gameObject.name == "fireshell(Clone)")
        {
            CmdDamage(Random.Range(40, 70) + (Random.Range(0, 10) == 0 ? 75 : 0));
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
            CmdSetUpdateEffect(true);
            effect = 0;
            effecttimer = 500;
        }else
        if (collision.gameObject.name == "bluefireshell(Clone)")
        {
            CmdDamage(Random.Range(40, 80) + (Random.Range(0, 10) == 0 ? 90 : 0));
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
            CmdSetUpdateEffect(true);
            effect = 1;
            effecttimer = 700;
        }
        else
        if (collision.gameObject.name == "iceshell(Clone)")
        {
            CmdDamage(Random.Range(40, 70) + (Random.Range(0, 10) == 0 ? 75 : 0));
            hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
            hpbartext.text = hp.ToString();
            CmdSetUpdateEffect(true);
            effect = 2;
            effecttimer = 500;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer) { return; }
        if (uitimer == 0&&other.gameObject.name=="team_heal(Clone)") {
            if (hp < maxhp) { 
                CmdDamage(-15);
                hpbar.transform.localScale = new Vector3(1f + (2f * hp / maxhp), 1, 1);
                hpbartext.text = hp.ToString();
            }
        }
    }
    public override void OnStartLocalPlayer()
    {
        manager = GameObject.Find("networkManager").GetComponent<customNetworkHUD>();
        CmdSetNick(manager.nickname);
        items = manager.items;
        hpbar = GameObject.Find("hpbar");
        upgradebar = GameObject.Find("upgradebar");
        matterbar = GameObject.Find("matterbar");
        reloadbar = GameObject.Find("reloadbar");
        magreloadbar = GameObject.Find("magreloadbar");
        hpbartext = GameObject.Find("hpbartext").GetComponent<Text>();
        upgradebartext = GameObject.Find("upgradebartext").GetComponent<Text>();
        matterbartext = GameObject.Find("matterbartext").GetComponent<Text>();
        upgradeparent = GameObject.Find("UPGRADE");
        QCUIL = GameObject.Find("QCUIL");
        QCUIR = GameObject.Find("QCUIR");
        QCUIL.SetActive(false);
        QCUIR.SetActive(false);
        upgradeparent.SetActive(false);
        for (int i = 0; i < upgradeparent.transform.childCount; ++i)
        {
            if (upgradeparent.transform.GetChild(i).name == "leftpic")
            {
                leftup = upgradeparent.transform.GetChild(i).gameObject;
            }
            if (upgradeparent.transform.GetChild(i).name == "rightpic")
            {
                rightup = upgradeparent.transform.GetChild(i).gameObject;
            }
        }
        comradebase = GameObject.Find("ТОВАРИЩИ");
        updateenemyui = true;
        GameObject back;
        for (int i = 0; i < items.Length; ++i) {
            back = itemcolors[i] = GameObject.Find("back_"+i);
            itemcounts[i] = back.transform.GetChild(3).GetComponent<Text>();
            itemcounts[i].text = items[i].ToString();
            itempowers[i] = back.transform.GetChild(1).gameObject;
        }
        GetListOfPlayers();
        if (manager.isElite)
        {
            CmdSetElite();
            maxmatter = 250;
            maxhp = 2000;
            hp = 2000;
            prevhp = 2000;
            level = 10;
            baserespawntext = "Временные неполадки.\nСекунд до исправления:";
            maxammo = 3;
            maxspeed *= 1.4f;
            maxreloadtime *= 0.3f;
            magmaxreloadtime = 0.7f;
            CmdSetWeaponType(7);
        }
        base.OnStartLocalPlayer();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Matter")&&isLocalPlayer&&respawntime<=0&&hp>0&&matter<maxmatter) {
            CmdAddMatter(17);
            updateui = true;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Collector")) {
            if (isLocalPlayer)
            {
               // other.GetComponent<collector>().GiveMatter(GetMatter());
                updateui = true;
            }
            else
            {
               // other.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
            }
        }
    }
    private void OnGUI()
    {
        if (!isLocalPlayer) { return; }
      //  GUI.Box(new Rect(Screen.width - 120, Screen.height * 0.5f - 40, 120, 20), level + "");
        for (int p = 0; p < players.Length; ++p)
        {
         /*   GUI.Box(new Rect(0, p * 25, 100, 25), players[p].nick);
            GUI.Box(new Rect(100, p * 25, 100, 25), "hp: " + players[p].hp);
            GUI.Box(new Rect(200, p * 25, 70, 25), players[p].matter + "/" + players[p].maxmatter);
            GUI.Box(new Rect(270, p * 25, 30, 25), players[p].level.ToString());*/
        }
        if (upgradenow)
        {
       //     GUI.Box(new Rect(Screen.width * 0.5f - 50, 30, 100, 25), "ПРОКАЧКА");
       //     GUI.Box(new Rect(Screen.width * 0.5f - 230, 90, 200, 100), "[1] : "+upgrades[level-2,0]);
       //     GUI.Box(new Rect(Screen.width * 0.5f + 30, 90, 200, 100), "[2] : " + upgrades[level-2, 1]);
        }

        if (prevhp > hp)
        {
            GUI.DrawTexture(new Rect(Screen.width - (prevhp - hp)/5, 0, (prevhp - hp)/5, Screen.height), damageTexture);
            GUI.DrawTexture(new Rect(0, 0, (prevhp - hp)/5, Screen.height), damageTexture);
            GUI.DrawTexture(new Rect(0, Screen.height- (prevhp - hp)/5, Screen.width, (prevhp - hp)/5), damageTexture);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, (prevhp - hp)/5), damageTexture);
        }
        if (effecttimer > 0) {
            GUI.DrawTexture(new Rect(Screen.width - effecttimer / 5, 0, effecttimer / 5, Screen.height), effectTexture[effect]);
            GUI.DrawTexture(new Rect(0, 0, effecttimer / 5, Screen.height), effectTexture[effect]);
            GUI.DrawTexture(new Rect(0, Screen.height - effecttimer / 5, Screen.width, effecttimer / 5), effectTexture[effect]);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, effecttimer / 5), effectTexture[effect]);
        }
    }
}
