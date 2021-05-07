using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    private const float CameraSpeed = 0.1f;
    
    private void LateUpdate()
    {
        Vector2 pos = Camera.main.transform.position;
        pos = Vector2.LerpUnclamped(transform.position, pos, 5*Time.deltaTime);
        Camera.main.transform.position = new Vector3(pos.x, pos.y, Camera.main.transform.position.z);
    }
}
