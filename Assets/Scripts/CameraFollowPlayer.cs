using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    private Transform player;
    public Vector3 offset;

    void Update()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z); // Camera follows the player with specified offset position
        }
        else
        {
            if(PlayerMove.GetInstance != null)
            player = PlayerMove.GetInstance.transform;
        }
    }
}
