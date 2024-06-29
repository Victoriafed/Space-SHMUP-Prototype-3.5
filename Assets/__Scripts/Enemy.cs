using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Set in Inspector: Enemy")]
    public float        speed = 10f; // Скорость в м/с
    public float        fireRate = 0.3f; // Секунд между выстрелами (не используется)
    public float        health = 10;
    public int          score = 100; // Очки за уничтожение корабля
    public float        showDamageDuration = 0.1f; // Длительность эффекта попадания в секундах
    public float        powerUpDropChance = 1f; // Вероятность сбросить бонус 
    protected BoundsCheck bndCheck;

    [Header("Set Dynamically: Enemy")]
    public Color[]      originalColors;
    public Material[]   materials; // Все материалы игрового объекта и его потомков 
    public bool         showingDamage = false;
    public float        damageDoneTime; // Время прекращения отображения эффекта 
    public bool         notifiedOfDestruction; 


    void Awake() {
        bndCheck = GetComponent<BoundsCheck>();
        // Получить материалы и цвет этого игрового объекта и его потомков
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i=0; i < materials.Length; i++) {
            originalColors[i] = materials[i].color;
        }
    }

    // Это свойство: метод, действующий как поле
    public Vector3 pos {
        get { return(this.transform.position);}
        set { this.transform.position = value;}
    }

    void Update() {
        Move();

        if (showingDamage && Time.time > damageDoneTime) {
            UnShowDamage();
        }

        if ( bndCheck != null && bndCheck.offDown) {
            // Корабль за нижней границей, поэтому его нужно уничтожить
            Destroy(gameObject);
        }
    }

    public virtual void Move() {
        Vector3  tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
 
    }

    void OnCollisionEnter (Collision coll) {
        GameObject otherGo = coll.gameObject;
        switch (otherGo.tag) {
            case "ProjectileHero":
                Projectile p = otherGo.GetComponent<Projectile>();
                // Если вражеский корабль за границами экранами,не наносить ему повреждений.
                if (!bndCheck.isOnScreen) {
                    Destroy(otherGo);
                    break;
                }
                ShowDamage();
                // Поразиь вражеский корабль. Получить разрущающую силу из WEAP_DICT в классе Main.
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                if (health <= 0) {
                    // Сообщить ообъекту-одиночке Main о уничтожении
                    if (!notifiedOfDestruction) {
                        Main.S.ShipDestroyed(this);
                    }
                    notifiedOfDestruction = true;
                    // Уничтожить этот вражеский корабль
                    Destroy(this.gameObject);
                }
                Destroy(otherGo);
                break;
            default:
                print("Enemy hit by non-ProjectileHero: " + otherGo.name);
                break;
        }
    }

    void ShowDamage() {
        foreach (Material m in materials) {
            m.color = Color.red;
        }
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage() {
        for (int i=0; i<materials.Length; i++) {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }
}
