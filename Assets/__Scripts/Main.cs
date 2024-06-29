using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    static public Main      S; // Объект-одиночка Main
    static Dictionary<WeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Set in Inspector")]
    public GameObject[]     prefabEnemies; // Массив шаблонов Enemy
    public float        enemySpawnSecond = 0.5f; // Вражеских кораблей в секунду
    public float        enemyDefaultPadding = 1.5f; // Отступ для позиционирования
    public WeaponDefinition[] weaponDefinitions;
    public GameObject         prefabPowerUp;
    public WeaponType[]           powerUpFrequency = new WeaponType[] {
                                    WeaponType.blaster, WeaponType.blaster,
                                    WeaponType.spread, WeaponType.shield};

    private BoundsCheck bndCheck;

    public void ShipDestroyed(Enemy e) {
        // Сгенерировать бонус с заданной вероятностью
        if (Random.value <= e.powerUpDropChance) {
            // Выбрать тип бонуса
            // Выбрать один из элементов в PowerUpFrequency
            int ndx = Random.Range(0, powerUpFrequency.Length);
            WeaponType puType = powerUpFrequency[ndx];

            // Создать экземпляр PowerUp
            GameObject go = Instantiate(prefabPowerUp) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            // Установиь соотвествующий тип WeaponType
            pu.SetType(puType);

            // Поместить в место, где находился разрушенный корабль
            pu.transform.position = e.transform.position;
        }
    }

    void Awake() {
        S = this;
        // Записать в bndCheck ссылку на компонент BoundsCheck этого игрвого объекта 
        bndCheck = GetComponent<BoundsCheck>();
        // Вызвать SpawnEnemy() один раз (в 2 секунды при значениях по умолчанию)
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnSecond);

        // Словарь с ключами типа WeaponType
        WEAP_DICT = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions) {
            WEAP_DICT[def.type] = def; 
        }
    }

    public void SpawnEnemy() {
        // Выбрать случайный шаблон Enemy для создания
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // Разместить вражеский корабль над экраном в случаной позиции
        float enemyPadding = enemyDefaultPadding;
        if (go.GetComponent<BoundsCheck>() != null) {
            enemyPadding = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Установить начальные координаты созданного вражеского корабля
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyPadding;
        float xMax = bndCheck.camWidth - enemyPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyPadding;
        go.transform.position = pos;

        // Снова вызвать SpawnEnemy()
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnSecond);
    }

    public void DelayedRestart(float delay) {
        // Вызвать метод (Restart) через delay секунд
        Invoke(nameof(Restart), delay);
    }

    public void Restart() {
        // Перезагрузить _Scene_0, чтобы перезапустить игру
        SceneManager.LoadScene("_Scene_0");
    }

    /// <summary>
    /// Статистическая функция, возвращающая WeaponDefinition из статистического защищенного поля WEAP_DICT класса Main.
    /// </summary>
    /// <return> Экзмепляр WeaponDefinition или, если нет такого определения 
    /// для указанноо WeaponType, возвращает новый экземпляр WeaponDefinition с типом none. </returns>
    /// <param name="wt"> Тип WeaponType, для которого требуется получить WeaponDefinition</param>
    static public WeaponDefinition GetWeaponDefinition(WeaponType wt) {
        // Проверить наличие указанного ключа в словаре 
        // Попытка извлечь значение по отсутсвующемц ключу вызовет ошибку, поэтому следующая инструкция играет важную роль.
        if (WEAP_DICT.ContainsKey(wt)) {
            return(WEAP_DICT[wt]);
        }
        // Следующая инструкция возвращает новый экземпляр WeaponDefinition 
        // с типом оружия WeaponType.none, что означает неудачную попытку
        // найти требуемое определение WeaponDefinition
        return(new WeaponDefinition());
    }
}
