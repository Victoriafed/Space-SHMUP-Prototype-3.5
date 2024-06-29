using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy_1 расширяет класс Enemy
public class Enemy_1 : Enemy
{
    [Header("Set in Inspecto: Enemy_1")]
    public float        waveFrequency = 2; // Число секунд полного цикла синусоиды
    // Ширина синусоиды в метрах
    public float        waveWidth = 4;
    public float        waveRotY = 45;

    private float       x0; // Начальное значение координаты Х
    private float       birthTime;

    // Метод Start хорошо подходит для наших целей, потому что не используется суперклассом Enemy
    void Start() {
        // Установить начальную координату Х объекта Enemy_1
        x0 = pos.x;
        birthTime = Time.time;
    }

    // переопределить функцию Move суперкласса Enemy
    public override void Move()
    {
        // Так как pos - это свойство, нельзя напрямую изменить pos.x
        // поэтому получим pos в виде вектор Vector3, доступного для изменения
        Vector3 tempPos = pos;
        // значение theta изменяется с течением времени
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        // повернуть немного относительно оси Y
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);
        
        // base.Move() обрабатывает движение вниз, вдоль оси Y
        base.Move();

        // print(bndCheck.isOnScreen)
    }
}
