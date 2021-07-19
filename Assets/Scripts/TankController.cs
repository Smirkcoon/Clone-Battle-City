using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
public class TankController : MonoBehaviour
{
    public enum TankType
    {
        Hunter,
        Destroyer,
        Stormtrooper,
        Assistant
    };

    public TankType tankType;
    public int Health,
        MoveSpeed,
        Accuracy,
        Damage,
        ShotsCount;
    public float  ShotDistance;
    private List<Mod> Mods = new List<Mod>();
    public bool IsPlayerTeam;
    private int HunterScanBlockDist = 50;
    public TankController TankAttaker;
    public bool IsPlayer;

    private void Start()
    {
        if (IsPlayer)
        {
            StartCoroutine(CheckIfReadyForShot(true, true, false));
        }
    }
    /// <summary>
    /// Configure tank relative levels modules from List<Mod>
    /// </summary>
    /// <param name="_Mods"></param>
    public void SetupTankParametersByMods(List<Mod> _Mods)
    {
        Mods = _Mods;
        foreach (Mod mod in _Mods)
        {
            SetupTankParametersByMod(mod);
        }
    }
    /// <summary>
    /// Configure tank relative levels modules
    /// </summary>
    /// <param name="mod"></param>
    private void SetupTankParametersByMod(Mod mod)
    {
        switch (mod.modType)
        {
            case ModsController.ModType.Base:
                MoveSpeed = 5 - mod.level;
                Health = 2 * mod.level;
                break;
            case ModsController.ModType.Tower:
                Accuracy = 6 + mod.level;
                break;
            case ModsController.ModType.FastCannon:
                Damage = mod.level;
                ShotsCount = 2;
                ShotDistance = 2.5f + mod.level;
                break;
            case ModsController.ModType.PoweredCannon:
                Damage = 2 * mod.level;
                ShotsCount = 1;
                ShotDistance = 2.5f + mod.level * 2;
                break;
        }
    }
    /// <summary>
    /// returns a module by type from the list of modules of this tank
    /// </summary>
    /// <param name="modType"></param>
    /// <returns></returns>
    private Mod GetModByType(ModsController.ModType modType)
    {
        foreach (Mod _mod in Mods)
        {
            if (_mod.modType == modType)
                return _mod;
        }
        return null;
    }
    /// <summary>
    /// Call when Tank Create, Setup Random TankType from all TankTypes
    /// </summary>
    public void SetRandomTankType()
    {
        System.Array values = TankType.GetValues(typeof(TankType));
        tankType = (TankType)values.GetValue(Random.Range(0, values.Length));
        SetTargetByTankType(tankType);
    }
    /// <summary>
    /// Customize movement and target selection for the character of the tank.
    /// </summary>
    /// <param name="tankType"></param>
    private void SetTargetByTankType(TankType tankType)
    {
        PathfindingMovement pathfindingMovement = GetComponent<PathfindingMovement>();
        switch (tankType)
        {
            case TankType.Hunter:
                StartCoroutine(Hunter(pathfindingMovement));
                StartCoroutine(CheckIfReadyForShot(true,false,false));
                break;
            case TankType.Destroyer:
                pathfindingMovement.FollowTarget = IsPlayerTeam == true ? MapController.GetInstance.BaseEnemy.transform : MapController.GetInstance.BasePlayer.transform;
                StartCoroutine(CheckIfReadyForShot(false, true, false));
                break;
            case TankType.Stormtrooper:
                StartCoroutine(StormtrooperAndAssistant(pathfindingMovement));
                StartCoroutine(CheckIfReadyForShot(true, false, false));
                break;
            case TankType.Assistant:
                StartCoroutine(StormtrooperAndAssistant(pathfindingMovement));
                StartCoroutine(CheckIfReadyForShot(false, false, true));
                break;
        }
    }
    /// <summary>
    /// Movement Character of Hunter
    /// </summary>
    /// <param name="pathfindingMovement"></param>
    /// <returns></returns>
    private IEnumerator Hunter(PathfindingMovement pathfindingMovement)
    {
        while (true)
        {
            if (MapController.GetInstance.ListBlocks.Count != 0)
            {
                List<Transform> TClosest = Util.GetScanDistListTransform(MapController.GetInstance.ListBlocks, HunterScanBlockDist, 5, transform);
                if (TClosest.Count == 0)
                {
                    HunterScanBlockDist += HunterScanBlockDist;//if there is not enough distance to the BlockCell, the distance is doubled
                    yield return new WaitForSeconds(1);
                }
                else
                    pathfindingMovement.FollowTarget = TClosest[Random.Range(0, TClosest.Count)];
            }
            yield return new WaitForSeconds(3);
        }
    }
    /// <summary>
    /// Movement Character of Stormtrooper And Assistant
    /// </summary>
    /// <param name="pathfindingMovement"></param>
    /// <returns></returns>
    private IEnumerator StormtrooperAndAssistant(PathfindingMovement pathfindingMovement)
    {
        while (true)
        {
            List<GameObject> GOlist = new List<GameObject>();
            foreach (TankController TP in (IsPlayerTeam == true && tankType != TankType.Assistant || IsPlayerTeam == false && tankType == TankType.Assistant) ? MapController.GetInstance.ListEnemyTanks : MapController.GetInstance.ListPlayerTeamTanks)
            {
                if(TP != this)
                GOlist.Add(TP.gameObject);
            }

            pathfindingMovement.FollowTarget = Util.GetClosestTransform(GOlist, transform);
            yield return new WaitForSeconds(1);
        }
    }
    /// <summary>
    /// Creating a list of targets to shoot
    /// </summary>
    /// <param name="Tanks"></param>
    /// <param name="Bases"></param>
    /// <param name="AssistTank"></param>
    /// <returns></returns>
    private IEnumerator CheckIfReadyForShot(bool Tanks, bool Bases, bool AssistTank)
    {
        while (true)
        {
            if (Util.CheckListIsNullOrEmpty(MapController.GetInstance.ListEnemyTanks) || Util.CheckListIsNullOrEmpty(MapController.GetInstance.ListPlayerTeamTanks))
                yield return new WaitForSeconds(1);

            if (MapController.GetInstance.BaseEnemy == null || MapController.GetInstance.BasePlayer == null)
                yield return new WaitForSeconds(1);

            List<Transform> Targets = new List<Transform>();
            if (Tanks) 
            {
                foreach (TankController TPTanks in (IsPlayerTeam == true ? MapController.GetInstance.ListEnemyTanks : MapController.GetInstance.ListPlayerTeamTanks))
                {
                    Targets.Add(TPTanks.gameObject.transform);
                }
            }

            if (Bases)
            {
                Targets.Add(IsPlayerTeam == true ? MapController.GetInstance.BaseEnemy.transform : MapController.GetInstance.BasePlayer.transform);
            }

            if (AssistTank)
            {
                foreach (TankController TPTanks in (IsPlayerTeam == false ? MapController.GetInstance.ListEnemyTanks : MapController.GetInstance.ListPlayerTeamTanks))
                {
                    if (TPTanks != this && TPTanks.TankAttaker != null)
                    {
                        Targets.Add(TPTanks.TankAttaker.gameObject.transform);
                    }
                }
            }

            if (!IsPlayer)
            {
                if (!ModsController.GetInstance.CheckModListIsNullOrEmpty(MapController.GetInstance.DropedMods))
                {
                    foreach (Mod DropedMod in MapController.GetInstance.DropedMods)
                    {
                        if (GetModByType(DropedMod.modType) != null && GetModByType(DropedMod.modType).level > DropedMod.level)
                        {
                            if(DropedMod.GODropedMod != null)
                            Targets.Add(DropedMod.GODropedMod.transform);
                        }
                    }
                }
            }

            for (int i = 0; i < Targets.Count; i++)
            {
                if (Vector2.Distance(transform.position, Targets[i].transform.position) < ShotDistance)
                {
                    if (GetModByType(ModsController.ModType.FastCannon) != null)
                    {
                        Shot(Targets[i], true);
                    }
                    else
                    {
                        Shot(Targets[i]);
                    }

                    break;
                }
            }
            yield return new WaitForSeconds(2);
        }
    }

    /// <summary>
    /// Create Missiles GameObjects when Ready For Shot
    /// </summary>
    /// <param name="target"></param>
    /// <param name="doubleShot"></param>
    private void Shot(Transform target, bool doubleShot = false)
    {
        GameObject Missile = IsPlayerTeam == true ? Resources.Load<GameObject>("MissilePlayerTeam01") : Resources.Load<GameObject>("MissileEnemyTeam02");
        Missile missile = Missile.GetComponent<Missile>();
        missile.Target = target;
        missile.firingUnit = this;
        if (doubleShot) 
        {
            Instantiate(Missile, transform.position - new Vector3(0, 0.3f, 0), transform.rotation);
            Instantiate(Missile, transform.position + new Vector3(0, 0.3f, 0), transform.rotation);
        }
        else
        {
            Instantiate(Missile, transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// When take damage change Health
    /// </summary>
    /// <param name="Damage"></param>
    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Health <= 0)
        {
            if (IsPlayerTeam)
            {
                MapController.GetInstance.ListPlayerTeamTanks.Remove(this);
            }
            else 
            {
                MapController.GetInstance.ListEnemyTanks.Remove(this);
            }

            DropMod();
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Create Random Droped Mod when Tank die
    /// </summary>
    private void DropMod()
    {
        int i = Random.Range(1, Mods.Count);
        if (Mods[i].GODropedMod != null)
        {
            Mods[i].GODropedMod.transform.SetParent(null);
            BoxCollider2D BoxCol2D = Mods[i].GODropedMod.AddComponent<BoxCollider2D>();
            BoxCol2D.size = new Vector2(0.5f, 0.5f);
            BoxCol2D.isTrigger = true;
            MapController.GetInstance.DropedMods.Add(Mods[i]);
        }
    }
    /// <summary>
    /// finds the corresponding module to update and compares
    /// updates only when there is a similar type of module below or the same level
    /// </summary>
    /// <param name="mod"></param>
    public void UpdateMod(Mod mod)
    {
        Mod _mod = GetModByType(mod.modType);
        if (_mod != null)
        {
            if (mod.level >= _mod.level || IsPlayer)
            {
                if (_mod.GODropedMod != null)
                {
                    if (IsPlayerTeam != mod.IsPlayerTeamMod)
                    {
                        Sprite sprite = mod.MainSprite;
                        mod.MainSprite = mod.SecondSprite;
                        mod.SecondSprite = sprite;
                        mod.IsPlayerTeamMod = IsPlayerTeam;
                        mod.GODropedMod.GetComponent<SpriteRenderer>().sprite = mod.MainSprite;
                    }

                    int index = _mod.GODropedMod.transform.GetSiblingIndex();
                    Transform parent = _mod.GODropedMod.transform.parent;
                    if (MapController.GetInstance.DropedMods.Contains(mod))
                        MapController.GetInstance.DropedMods.Remove(mod);

                    BoxCollider2D BoxCol2D = mod.GODropedMod.GetComponent<BoxCollider2D>();
                    if (BoxCol2D != null)
                        Destroy(BoxCol2D);

                    Mods[Mods.FindIndex((x => mod.modType == _mod.modType))] = mod;
                    Destroy(_mod.GODropedMod);
                    mod.GODropedMod.transform.SetParent(parent);
                    mod.GODropedMod.transform.SetSiblingIndex(index);
                    mod.GODropedMod.transform.localPosition = Vector3.zero;
                    mod.GODropedMod.transform.localRotation = Quaternion.identity;
                    SetupTankParametersByMod(mod);
                }
            }
        }
    }
}
