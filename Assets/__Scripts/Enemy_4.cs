using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Part - - еще один сериализируемый класс подобно WeaponDefinition, предназначенный для хранения данных
/// </summary>
[System.Serializable]
public class Part {
    // Значение этих трех полей должны определяться в инспекторе
    public string       name; // имя этй части
    public float        health; // Степень стойкости этой части
    public string[]     protectedBy; // Другие части, защищающие эту

    // Эти два поля инициализируются автоатически в Start(). Кэшиование как здесь ускоряет получение необходимых данныхю
    [HideInInspector] // Не позволяет следующему полю появиться в инспекторе
    public GameObject   go; // Игровой объект этой части
    [HideInInspector]
    public Material     mat; // Материал для отображения повреждений
}

/// <summary>
/// Enemy_4 создается за верхней границей, выбирает случайную точку на экране
/// и перемещается к ней. Добравшись до места, выбирает другую случайную точку 
/// и продолжает двигаться, пока игрок не уничтожить.
/// </summary>

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[]       parts; // Массив частей, составляющих корабль
    private Vector3     p0, p1; // Две точки для интерполяции
    private float       timeStart; // Время создания этого корабля
    private float       duration=4; // Продолжительность перемещения

    void Start() {
        // Начальная позиция уже выбрана в Main.SpawnEnemy(), поэтому запишем ее как начальные значения в p0 и p1
        p0 = p1 = pos;
        InitMovement();

        // Записатьв кэш игровой объект и материал каждой части в parts 
        Transform t;
        foreach (Part prt in parts) {
            t = transform.Find(prt.name);
            if (t != null) {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement() {
        p0 = p1; // Переписать p1 в p0
        // Выбрать новую точку p1 на экране 
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Сбросить время
        timeStart = Time.time;
    }

    public override void Move()
    {
        // Этот метод переопределяет Enemy.Move и реализует линейную интерполяцию
        float u = (Time.time - timeStart) / duration;

        if (u >= 1) {
            InitMovement();
            u=0;
        }

        u = 1 -Mathf.Pow(1-u, 2);
        pos = (1 - u) * p0 + u * p1;
        
    }

    // Эти две функции выполняют поиск части в массиве parts n по имени или ссылке на игровой объект  
    Part FindPart(string n) {
        foreach (Part prt in parts)
        {
            if (prt.name == n) {
                return(prt);
            }
        }
        return(null);
    }

    Part FindPart(GameObject go) {
        foreach (Part prt in parts)
        {
            if (prt.go == go) {
                return(prt);
            }
        }
        return(null);
    }

    // Эти функции возвращают true если данная часть уничтожена 
    bool Destroyed(GameObject go) {
        return(Destroyed(FindPart(go)));
    }

    bool Destroyed(string n) {
        return(Destroyed(FindPart(n)));
    }

    bool Destroyed(Part prt) {
        if (prt == null ) { // Если ссылка на часть не была передана
            return(true); // Вернуть true (то есть: да, была уничтожена)
        }
        // Вернуть результат сравнения: prt.health <= 0
        // Если prt.health <= 0 вернуть true (да, была уничтожкна)
        return(prt.health <= 0);
    }

    // Окрашивает в красный только одну часть а не весь корабль 
    void ShowLocalizedDamage(Material m) {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    // Переопределяет метод OnCollisionEnter из сценария Enemy.cs
    void OnCollisionEnter(Collision coll) {
        GameObject other = coll.gameObject;
        switch (other.tag) {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                // Если корабль за границами экранами не повреждать его
                if (!bndCheck.isOnScreen) {
                    Destroy(other);
                    break;
                }
                
                // Поразить вражеский корабль 
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null) {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                } 
                // Проверить защищена ли еще эта часть корабля 
                if (prtHit.protectedBy != null) {
                    foreach (string s in prtHit.protectedBy) {
                        // Если хотя бы одна из защищающих частей не разрушена 
                        if (!Destroyed(s)) {
                            // не наносить вореждений этой части
                            Destroy(other); // Уничтожить снаряд ProjectileHero
                            return; // выйти не повреждая Enemy_4
                        }
                    }
                }

                // Эта часть не защищена нанести ей повреждение
                // Получить разрущающую силу из Projectile.type и Main.WeapDict
                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                // Показать эффект попадения в часть
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0) {
                    // Вместо разрушения всего корабля деактивировать уничтженную часть
                    prtHit.go.SetActive(false);
                }
                //  Проверить был ли корабль разрушен полностью 
                bool allDestroyed = true; // Предположить что разрушен
                foreach (Part prt in parts) {
                    if (!Destroyed(prt)) { // Если какая то часть существует 
                        allDestroyed = false; // записать false в allDestroyed
                        break; // и прервать цикл foreach
                    }
                }
                if (allDestroyed) { // Если корабль разрушен полностью
                    // ..уведомить объект-одиночку Main что этот корабль разрушен
                    Main.S.ShipDestroyed(this);
                    // Уничтожить этот объект
                    Destroy(this.gameObject);
                }
                Destroy(other); // Уничтожить снаряд Projectilehero
                break;
                
        }
    }


}
