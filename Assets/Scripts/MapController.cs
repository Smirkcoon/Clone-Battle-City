using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class MapController : MonoBehaviour
{
    public static MapController GetInstance { get; private set; }
    [SerializeField]
    private int CountOfBlocks = 100;
    [SerializeField]
    private int MapRows = 40;
    [SerializeField]
    private int MapColumns = 40;
    [SerializeField]
    private int EnemyCount = 30;
    [SerializeField]
    private int PlayerTeamCount = 30;

    private GameObject PrefabBasePlayer,
        PrefabBaseEnemy,
        PrefabGroundCell,
        PrefabBlock;

    [HideInInspector]
    public GameObject BasePlayer,
        BaseEnemy;

    [HideInInspector]
    private Pathfinding pathfinding;
    [HideInInspector]
    public List<TankController> ListEnemyTanks = new List<TankController>();
    [HideInInspector]
    public List<TankController> ListPlayerTeamTanks = new List<TankController>();
    public List<Mod> DropedMods = new List<Mod>();
    [HideInInspector]
    public List<Transform> ListBlocks = new List<Transform>();

    private void Awake()
    {
        GetInstance = this;
        pathfinding = new Pathfinding(MapRows, MapColumns); //initialization of cells for laying routes
        PrefabBasePlayer = Resources.Load<GameObject>("BasePlayer");
        PrefabBaseEnemy = Resources.Load<GameObject>("BaseEnemy");
        PrefabGroundCell = Resources.Load<GameObject>("BGCell");
        PrefabBlock = Resources.Load<GameObject>("Block");
    }

    private void Start()
    {
        SetupNewMap();
    }
    /// <summary>
    /// creating all initial objects on the map
    /// </summary>
    private void SetupNewMap()
    {
        NewGroundCells();
        NewBases();
        NewTank(true);
        StartCoroutine(TankCountController());
        NewTank(false);
        NewBlocks();
    }
    /// <summary>
    /// initialization background cells
    /// </summary>
    private void NewGroundCells()
    {
        GameObject BGCells = new GameObject("BGCells"); //creating a parent so as not to interfere in the hierarchy
        for (int x = 0; x < MapRows; x++)
        {
            for (int y = 0; y < MapColumns; y++)
            {
                Instantiate(PrefabGroundCell, new Vector2(x, y), Quaternion.Euler(0, 0, Random.Range(0, 4) * 90), BGCells.transform);
            }
        }
    }
    /// <summary>
    /// initialization bases
    /// </summary>
    private void NewBases()
    {
        //creation and placement of the player's team base
        int collum = Random.Range(1, 39);
        BasePlayer = Instantiate(PrefabBasePlayer, new Vector2((float)collum, 0), Quaternion.identity);
        BasePlayer.AddComponent<Base>();
        BasePlayer.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        pathfinding.GetCell(collum, 0).SetIsWalkable(!pathfinding.GetCell(collum, 0).isWalkable);
        //creation and placement of the enemy team base
        BaseEnemy = Instantiate(PrefabBaseEnemy, new Vector2((39.0f - collum), 39.0f), Quaternion.identity);
        BaseEnemy.AddComponent<Base>();
        BaseEnemy.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        pathfinding.GetCell(39 - collum, 39).SetIsWalkable(!pathfinding.GetCell(39 - collum, 39).isWalkable);
    }
    /// <summary>
    /// initialization impassable blocks on the map
    /// </summary>
    private void NewBlocks()
    {
        GameObject Blocks = new GameObject("Blocks");//creating a parent so as not to interfere in the hierarchy
        for (int i = 0; i < CountOfBlocks; i++)
        {
            int x = Random.Range(0, MapRows);
            int y = Random.Range(0, MapColumns);
            if (pathfinding.GetCell(x, y).isWalkable)
            {
                ListBlocks.Add(Instantiate(PrefabBlock, new Vector2((float)x, (float)y), Quaternion.Euler(0, 0, Random.Range(0, 4) * 90), Blocks.transform).transform);
                pathfinding.GetCell(x, y).SetIsWalkable(false);
            }
            else
            {
                i--;
                continue;
            }
        }
    }
    /// <summary>
    /// controls the number of tanks on the map
    /// </summary>
    /// <returns></returns>
    private IEnumerator TankCountController()
    {
        while (true)
        {
            if (ListPlayerTeamTanks.Count < 2 && PlayerTeamCount > 0)
                NewTank(true);
            if (ListEnemyTanks.Count < 2 && EnemyCount > 0)
                NewTank(false);
            yield return new WaitForSeconds(1);
        }
    }

    /// <summary>
    /// initialization of tank
    /// </summary>
    /// <param name="IsPlayerTeam"></param>
    private void NewTank(bool IsPlayerTeam)
    {
        PathCell pathCell = IsPlayerTeam == true ? Util.GetFreeCellNearPos(BasePlayer.transform.position, pathfinding) : Util.GetFreeCellNearPos(BaseEnemy.transform.position, pathfinding);
        if (pathCell == null)
            return;
        pathCell.SetIsWalkable(false);

        List<Mod> Mods = ModsController.GetInstance.TankRandomComplitation(IsPlayerTeam);

        GameObject Tank = new GameObject(IsPlayerTeam == true ? "PlayerTeamTank" : "EnemyTank");
        Tank.transform.position = new Vector2(pathCell.x, pathCell.y);
        TankController tankParameters = Tank.AddComponent<TankController>();
        tankParameters.SetupTankParametersByMods(Mods);
        tankParameters.IsPlayerTeam = IsPlayerTeam;
        if (ListPlayerTeamTanks.Count != 0)
        {
            Tank.AddComponent<PathfindingMovement>();
            tankParameters.SetRandomTankType();
            tankParameters.IsPlayer = false;
            Tank.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
        }
        else
        {
            Tank.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
            tankParameters.IsPlayer = true;
            Tank.AddComponent<PlayerMove>();
            Tank.name = "Player";
        }
        foreach (Mod mod in Mods)
        {
            GameObject GOmod = new GameObject();
            SpriteRenderer SpR = GOmod.AddComponent<SpriteRenderer>();
            SpR.sprite = mod.MainSprite;
            GOmod.name = mod.modType.ToString();
            GOmod.transform.SetParent(Tank.transform);
            GOmod.transform.localPosition = Vector3.zero;
            mod.GODropedMod = GOmod;
            SpR.sortingOrder = GOmod.transform.GetSiblingIndex() + 1;
        }

        if (IsPlayerTeam)
        {
            ListPlayerTeamTanks.Add(tankParameters);
            PlayerTeamCount--;
        }
        else
        {
            ListEnemyTanks.Add(tankParameters);
            EnemyCount--;
        }
    }
    /// <summary>
    /// Reload Scene when Player die or one of bases destroy
    /// </summary>
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
