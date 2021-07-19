using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Transform Target;
    public TankController firingUnit;
    private TankController _firingUnit;
    private bool takeTarget = false;
    private void Start()
    {
        Destroy(gameObject, 5);
        _firingUnit = firingUnit;
    }
    /// <summary>
    /// calculation of hitting the target and the result
    /// </summary>
    private void Shot()
    {
        if (Target != null)
        {
            TankController TPtarget = Target.GetComponent<TankController>();
            if (TPtarget != null)
            {
                TPtarget.TankAttaker = firingUnit;
                if (_firingUnit.Accuracy >= Random.Range(1, 11))
                {
                    TPtarget.TakeDamage(_firingUnit.Damage);
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            if (!ModsController.GetInstance.CheckModListIsNullOrEmpty(MapController.GetInstance.DropedMods))
            {
                for (int i = 0; i < MapController.GetInstance.DropedMods.Count; i++)
                {
                    if (MapController.GetInstance.DropedMods[i].GODropedMod.transform == Target)
                    {
                        Destroy(MapController.GetInstance.DropedMods[i].GODropedMod);
                        MapController.GetInstance.DropedMods.Remove(MapController.GetInstance.DropedMods[i]);
                        Destroy(gameObject);
                    }
                }
            }

            Base BaseTarget = Target.GetComponent<Base>();
            if (BaseTarget != null)
            {
                BaseTarget.TakeDamage(_firingUnit.Damage);
                Destroy(gameObject);
            }
        }
    }
    /// <summary>
    /// Moving to target
    /// </summary>
    private void Update()
    {
        if (Target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.position, 6 * Time.deltaTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (MapController.GetInstance.ListBlocks.Contains(col.transform))
            Destroy(gameObject);

        if (col.transform == Target && !takeTarget)
        {
            takeTarget = true;
            Shot();
        }
    }
}
