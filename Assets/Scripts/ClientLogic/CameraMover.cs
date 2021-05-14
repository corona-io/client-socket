using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    private const float CameraSpeed = 0.1f;
    
    private void LateUpdate()
    {
        if (gameObject.GetComponent<Player>().isMine == false) return;
        Vector2 pos = Camera.main.transform.position;
        pos = Vector2.LerpUnclamped(transform.position, pos, 5*Time.deltaTime);
        Camera.main.transform.position = new Vector3(pos.x, pos.y, Camera.main.transform.position.z);
    }
}
