using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public static PlayerMove GetInstance;
    private Rigidbody2D playerRigidbody2D;
    private Vector3 moveDir;
    private float rotateZ;
    private Mod _mod;
    private TankController tankController;
    private void Awake()
    {
        GetInstance = this;
        playerRigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        playerRigidbody2D.gravityScale = 0;
        playerRigidbody2D.freezeRotation = true;
        tankController = GetComponent<TankController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveY = +1f;
            rotateZ = -90f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
            rotateZ = 90f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
            rotateZ = 0f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = +1f;
            rotateZ = 180f;
        }

        if (Input.GetKey(KeyCode.Z) && _mod != null)
        {
            GetComponent<TankController>().UpdateMod(_mod);
        }
        if (Input.GetKey(KeyCode.M) && _mod != null)
        {
            if (MapController.GetInstance.DropedMods.Contains(_mod))
            {
                MapController.GetInstance.DropedMods.Remove(_mod);
            }
            Destroy(_mod.GODropedMod);
        }


        moveDir = new Vector3(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        bool isIdle = moveDir.x == 0 && moveDir.y == 0 && transform.eulerAngles.z == rotateZ;
        if (!isIdle)
        {
            transform.eulerAngles = new Vector3(0f, 0f, rotateZ);
            playerRigidbody2D.MovePosition(transform.position + moveDir * tankController.MoveSpeed * Time.fixedDeltaTime);
        }
    }
    private void OnDestroy()
    {
        MapController.GetInstance.ReloadScene();
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (!ModsController.GetInstance.CheckModListIsNullOrEmpty(MapController.GetInstance.DropedMods))
        {
            foreach (Mod mod in MapController.GetInstance.DropedMods)
            {
                if (mod.GODropedMod == col.gameObject)
                {
                    _mod = mod;
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (_mod != null && col.gameObject == _mod.GODropedMod)
        {
            _mod = null;
        }
    }
}
