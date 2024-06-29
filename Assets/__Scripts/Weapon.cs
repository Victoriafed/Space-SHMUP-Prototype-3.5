using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Это перечисление всех возможных типов оружия.
/// Также включает тип "shield", чтобы дать возможность совершенствовать защиту.
/// Аббревиатурой [HP] ниже отмечены элементы, не реализованные в этой книге.
/// </summary>

public enum WeaponType {
    none, // По умолчанию / нет оружия
    blaster, // Простой бластер
    spread, // Веерная пушка, стреляющая несколькими снарядами
    phaser, // [HP] Волновой фазер
    missile, // [HP] Самонаводящиеся ракеты
    laser, // [HP] Наносит поврееждения при долговременном воздействии
    shield // Увеличивает shieldLevel
}

/// <summary>
/// Класс WeaponDefinition позволяет настраивать свойства
/// конкретного вида оружия в инспекторе. Для этого класса Main
/// будет хранить массив элементов типа WeaponDefinition.
/// </summary>
[System.Serializable]
public class WeaponDefinition {
    public WeaponType   type = WeaponType.none;
    public string       letter; // Буква на кубике, изображающем бонус
    public Color        color = Color.white; // Цвет ствола оружия и кубика бонуса
    public GameObject   projectilePrefab; // Шаблон снарядов
    public Color        projectileColor = Color.white; 
    public float        damageOnHit = 0; // Разрушительная мощность 
    public float        continuosDamage = 0; // Степень разрушения в секунду (для Laser)
    public float        delayBetweenShots = 0;
    public float        velocity = 20; // Скорость полета снарядов
}
public class Weapon : MonoBehaviour
{
    static public Transform     PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]  [SerializeField]
    private WeaponType          _type = WeaponType.none;
    public WeaponDefinition     def;
    public GameObject           collar;
    public float                lastShotTime; // Время последнего выстрела
    private Renderer              collarRend;

    void Start() {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();

        // Вызвать SetType(), чтобы заменить тип оружия по умолчанию Weapon.none
        SetType(_type);
        // Динамическт создать точку привязки для всех снарядов 
        if (PROJECTILE_ANCHOR == null) {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        // Найти fireDelegate в корневом ировом объекте
        GameObject rootGo = transform.root.gameObject;
        if (rootGo.GetComponent<Hero>() != null) {
            rootGo.GetComponent<Hero>().fireDelegate += Fire;
        }
    }

    public WeaponType type {
        get { return(_type); }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt) {
        _type = wt;
        if (type == WeaponType.none) {
            this.gameObject.SetActive(false);
            return;
        } else {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0; // Сразу после остановки _type можно выстрелить 
    }

    public void Fire() {
        // Если this.gameObject неактивен, выйти 
        if (!gameObject.activeInHierarchy) return;
        // Если между выстрелами прошло недостаточно много времени, выйти 
        if (Time.time - lastShotTime < def.delayBetweenShots) {
            return;
        }
        Projectile p;
        Vector4 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0) vel.y = - vel.y;
        switch (type) {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.velocity = vel;
                break;
            case WeaponType.spread:
                p = MakeProjectile(); // Снаряд, летящий прямо
                p.rigid.velocity = vel;
                p = MakeProjectile(); // Снаряд, летящий вправо
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); // Снаряд, летящий влево
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                break;
        }  
    }

    public Projectile MakeProjectile() {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.parent.gameObject.tag == "Hero") {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        } else {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShotTime = Time.time;
        return(p);
    }

}
