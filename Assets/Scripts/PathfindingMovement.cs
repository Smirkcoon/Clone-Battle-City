using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingMovement : MonoBehaviour
{
    private int currentPathIndex;
    private List<Vector3> pathVectorList;

    public Transform FollowTarget;
    private PathCell CurrentCell;
    private TankController tankController;
    private new Rigidbody2D rigidbody2D;
    private void Awake()
    {
        rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 0;
        rigidbody2D.freezeRotation = true;
    }
    private void Start()
    {
        CurrentCell = Pathfinding.GetInstance.GetCell((int)transform.position.x, (int)transform.position.y);
        CurrentCell.SetIsWalkable(false);
        StartCoroutine(MoveToTarget());
        tankController = GetComponent<TankController>();
    }
    private void Update()
    {
        HandleMovement();
    }
    /// <summary>
    /// Route movement
    /// </summary>
    private void HandleMovement()
    {
        if (pathVectorList != null && pathVectorList.Count > 0)
        {
            Vector3 pos = pathVectorList[currentPathIndex];
            Vector3 targetPosition = new Vector2(pos.x, pos.y);

            if (Vector2.Distance(transform.position, targetPosition) > 0.01f)
            {
                Vector3 moveDir = (targetPosition - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, tankController.MoveSpeed * Time.deltaTime);
                var angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            }
            else
            {
                CurrentCell.SetIsWalkable(true);
                CurrentCell = Pathfinding.GetInstance.GetCell((int)transform.position.x, (int)transform.position.y);
                CurrentCell.SetIsWalkable(false);
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count)
                {
                    pathVectorList = null;
                }
            }
        }
    }
    /// <summary>
    /// control of restarting the calculation of the target position
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveToTarget()
    {
        while (this.gameObject != null)
        {
            if (FollowTarget != null)
            {
                SetTargetPosition(FollowTarget.position);
            }

            yield return new WaitForSeconds(1f);
        }
    }
    /// <summary>
    /// Recalculation of target position
    /// </summary>
    /// <param name="targetPosition"></param>
    private void SetTargetPosition(Vector3 targetPosition)
    {
        currentPathIndex = 0;
        pathVectorList = Pathfinding.GetInstance.FindPath(transform.position, targetPosition);

        if (pathVectorList != null && pathVectorList.Count > 1)
        {
            pathVectorList.RemoveAt(0);
            int i = pathVectorList.Count;
            pathVectorList.RemoveAt(i - 1);
        }
    }
    private void OnDestroy()
    {
        if (CurrentCell != null)
            CurrentCell.SetIsWalkable(true);
    }

    /// <summary>
    /// when stepping on a module
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!ModsController.GetInstance.CheckModListIsNullOrEmpty(MapController.GetInstance.DropedMods))
        {
            foreach (Mod mod in MapController.GetInstance.DropedMods)
            {
                if (mod.GODropedMod == col.gameObject)
                {
                    tankController.UpdateMod(mod);
                    break;
                }
            }
        }
    }
}