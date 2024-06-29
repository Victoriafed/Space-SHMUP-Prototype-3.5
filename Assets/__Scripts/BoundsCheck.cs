using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// Предотвращает выход ирового объекиа за границы экрана.
/// Важно: работает ТОЛЬКО с ортографической камерой Main Camera [0, 0, 0].
/// </summary>

public class BoundsCheck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float        radius = 1f;
    public bool         keepScreen = true;

    [Header("Set Dynamically")]
    public bool        isOnScreen = true;
    public float        camWidth;
    public float        camHeight;
    
    [HideInInspector]
    public bool         offRight, offLeft, offUp, offDown;

    void Awake() {
        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
    }

    void LateUpdate() {
        Vector3 pos = transform.position;
        isOnScreen = true;
        offRight = offLeft = offUp = offDown = false;

        if (pos.x > camWidth - radius) {
            pos.x = camWidth - radius;
            offRight = true;
        }
        if (pos.x < -camWidth + radius) {
            pos.x = -camWidth + radius;
            offLeft = true;
        }
        if (pos.y > camHeight - radius) {
            pos.y = camHeight - radius;
            offUp = true;
        }
        if (pos.y < -camHeight + radius) {
            pos.y = -camWidth + radius;
            offDown = true;
        }
        
        isOnScreen = !(offRight || offLeft || offUp || offDown);
        if (keepScreen && !isOnScreen) {
            transform.position = pos;
            isOnScreen = true;
            offRight = offLeft = offUp = offDown = false;
        }
    }

    // Рисует границы в панели Scene(Сцена) с помощью OnDrawGizmos()
    void OnDrawGizmos() {
        if (!Application.isPlaying) return;
        Vector3 boundSize = new Vector3(camWidth * 2, camHeight * 2, 0.1f);
        Gizmos.DrawWireCube(Vector3.zero, boundSize);
    }
    
}
