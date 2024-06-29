using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero  S; // Одиночка

    [Header("Set in Inspector")]
    // Поля управляющие движением корабля
    public float        speed = 30;
    public float        rollMult = -45;
    public float        pitchMult = 30;
    public float        gameRestartDelay = 2f;
    public GameObject   prjectilePrefab;
    public float        projectileSpeed = 40;
    public Weapon[]     weapons; 

    [Header("Set Dynamically")]
    [SerializeField]
    private float        _shieldLevel = 1;

    // Эта переменная хранит ссылку на последний столкнувшийся игровой объекта
    private GameObject  lastTriggerGo = null;
    
    // Объявление нового делегата типа WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    // Создать поле типа WeaponFireDelegate c именем fireDelegate.
    public WeaponFireDelegate fireDelegate;


    void Start() {
        if (S == null) {
            S = this; // Сохранить ссылку на одиночку
            // Очистить массиа weapons и начать игру с 1 бластером
            ClearWeapons();
            weapons[0].SetType(WeaponType.blaster);
        } else {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
//        fireDelegate += TempFire;
    }

    void Update() {
        // Извлечь информацию из класса Input
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        // изменить transform.position, опираясь на информацию по осям
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        // Повернуть корабль, чтоы придать ощущения динамизма
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        // Позволить кораблю выстрелить 
        // if (Input.GetKeyDown(KeyCode.Space)) TempFire();

        // Произвести выстрел из всех видов оружия вызовом fireDelegate
        // Сначала проверить нажатие клавиши: Axis("Jump")
        // Затем убедиться, что значение fireDelegate не равно null, чтобы избежать ошибки.
        if (Input.GetAxis("Jump") == 1 && fireDelegate != null) fireDelegate();    
    }

    void TempFire() {
        GameObject projGo = Instantiate<GameObject>(prjectilePrefab);
        projGo.transform.position = transform.position;
        Rigidbody rigidB = projGo.GetComponent<Rigidbody>();
        // rigidB.velocity = Vector3.up * projectileSpeed;

        Projectile proj = projGo.GetComponent<Projectile>();
        proj.type = WeaponType.blaster;
        float tSpeed = Main.GetWeaponDefinition(proj.type).velocity;
        rigidB.velocity = Vector3.up * tSpeed;
    }

    void OnTriggerEnter(Collider other) {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        // print("Trigger: " + go.name);

        // Гарантировать невозможность повторного столкновения с тем же объекта
        if (go == lastTriggerGo) {
            return;
        }

        lastTriggerGo = go;
        
        if (go.tag == "Enemy") {
            shieldLevel--;
            Destroy(go);
        } else if (go.tag == "PowerUP") {
            // Если защитное поле столкнулось с бонусом
            AbsorbPowerUp(go);
        } else {
            print("Triggered by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(GameObject go) {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type) {
            case WeaponType.shield:
                shieldLevel++;
                break;

            default:
                if (pu.type == weapons[0].type) { // Если оружие того же типа
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null) {
                        // Установить в pu.type
                        w.SetType(pu.type);
                    } else { // Если оружие другого типа 
                        ClearWeapons();
                        weapons[0].SetType(pu.type);
                    }
                }
                break;
        }
        pu.AbsorbedBy(this.gameObject);
    }

    public float shieldLevel {
        get { return(_shieldLevel); }
        set { 
            _shieldLevel = Mathf.Min(value, 4);
            // Если уровень поля упал до нуля или ниже 
            if (value < 0) {
                Destroy(this.gameObject);
                Main.S.DelayedRestart(gameRestartDelay);
            }
        }
    }

    Weapon GetEmptyWeaponSlot() {
        for (int i=0; i<weapons.Length; i++) {
            if (weapons[i].type == WeaponType.none) {
                return(weapons[i]);
            }
        }
        return(null);
    }

    void ClearWeapons() {
        foreach (Weapon w in weapons) {
            w.SetType(WeaponType.none);
        }
    }
}
