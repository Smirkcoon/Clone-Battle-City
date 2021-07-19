using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModsController : MonoBehaviour
{   
    public enum ModType
    {
        Truck,
        Base,
        Tower,
        FastCannon,
        PoweredCannon     
    };

    [Header("Sprites of Mods, input level from 1 to larger")]
    public Sprite SpriteTruck;
    [SerializeField]
    private Sprite[] SpBasesPlayer,
        SpTowersPlayer,
        SpFastCannonsPlayer,
        SpPoweredCannonsPlayer,
        SpBasesEnemy,
        SpTowersEnemy,
        SpFastCannonsEnemy,
        SpPoweredCannonsEnemy;

    private List<Mod> MBasesPlayer,
        MTowersPlayer,
        MFastCannonsPlayer,
        MPoweredCannonsPlayer,
        MBasesEnemy,
        MTowersEnemy,
        MFastCannonsEnemy,
        MPoweredCannonsEnemy;

    public static ModsController GetInstance { get; private set; }
    
    private void Awake()
    {
        GetInstance = this;
    }

    /// <summary>
    /// Pull and remove Random Mod From Mods List
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private Mod PullRandomFromList(List<Mod> list)
    {
        if (list != null && list.Count > 0)
        {
            int i = Random.Range(0, list.Count);
            Mod result = list[i];
            list.RemoveAt(i);
            return result;
        }
        else
            return null;

    }

    /// <summary>
    /// Check Mod List - return true if list IsNullOrEmpty
    /// </summary>
    /// <param name="modList"></param>
    /// <returns></returns>
    public bool CheckModListIsNullOrEmpty(List<Mod> modList)
    {
        return (modList == null || modList.Count == 0);
    }

    /// <summary>
    /// return all mods in list
    /// </summary>
    /// <param name="Playerteam"></param>
    /// <returns></returns>
    public List<Mod> TankRandomComplitation(bool Playerteam)
    {
        List<Mod> allMods = new List<Mod>();
        Mod _Mod = new Mod();
        if (SpriteTruck != null)
            _Mod.MainSprite = SpriteTruck;
        allMods.Add(_Mod);
        int i = Random.Range(0, 2);//FastCannon or PoweredCannon

        if (Playerteam)
        {
            if (CheckModListIsNullOrEmpty(MBasesPlayer))
                MBasesPlayer = NewListMods(SpBasesPlayer, ModType.Base, SpBasesEnemy);
            allMods.Add(PullRandomFromList(MBasesPlayer));

            if (CheckModListIsNullOrEmpty(MTowersPlayer))
                MTowersPlayer= NewListMods(SpTowersPlayer, ModType.Tower, SpTowersEnemy);
            allMods.Add(PullRandomFromList(MTowersPlayer));

            if (i == 0)
            {
                if (CheckModListIsNullOrEmpty(MFastCannonsPlayer))
                    MFastCannonsPlayer = NewListMods(SpFastCannonsPlayer, ModType.FastCannon, SpFastCannonsEnemy);
                allMods.Add(PullRandomFromList(MFastCannonsPlayer));
            }
            else
            {
                if (CheckModListIsNullOrEmpty(MPoweredCannonsPlayer))
                    MPoweredCannonsPlayer = NewListMods(SpPoweredCannonsPlayer, ModType.PoweredCannon, SpPoweredCannonsEnemy);
                allMods.Add(PullRandomFromList(MPoweredCannonsPlayer));
            }
        }
        else
        {
            if (CheckModListIsNullOrEmpty(MBasesEnemy))
                MBasesEnemy = NewListMods(SpBasesEnemy, ModType.Base, SpBasesPlayer);
            allMods.Add(PullRandomFromList(MBasesEnemy));

            if (CheckModListIsNullOrEmpty(MTowersEnemy))
                MTowersEnemy = NewListMods(SpTowersEnemy, ModType.Tower, SpTowersPlayer);
            allMods.Add(PullRandomFromList(MTowersEnemy));

            if (i == 0)
            {
                if (CheckModListIsNullOrEmpty(MFastCannonsEnemy))
                    MFastCannonsEnemy = NewListMods(SpFastCannonsEnemy, ModType.FastCannon, SpFastCannonsPlayer);
                allMods.Add(PullRandomFromList(MFastCannonsEnemy));
            }
            else
            {
                if (CheckModListIsNullOrEmpty(MPoweredCannonsEnemy))
                    MPoweredCannonsEnemy = NewListMods(SpPoweredCannonsEnemy, ModType.PoweredCannon, SpPoweredCannonsPlayer);
                allMods.Add(PullRandomFromList(MPoweredCannonsEnemy));
            }
        }

        foreach (Mod mod in allMods)
            mod.IsPlayerTeamMod = Playerteam;      

        return allMods;
    }
    /// <summary>
    /// refill Mods list if CheckModListIsNullOrEmpty is true
    /// </summary>
    /// <param name="sprites"></param>
    /// <param name="Mods"></param>
    /// <param name="modType"></param>
    private List<Mod> NewListMods(Sprite[] sprites, ModType modType, Sprite[] AnotherTeamSprites)
    {
        List<Mod> Mods = new List<Mod>();
        for (int i = 0; i < sprites.Length; i++)
        {
            Mod _Mod = new Mod();

            if (sprites[i] != null)
                _Mod.MainSprite = sprites[i];
            if (AnotherTeamSprites[i] != null)
                _Mod.SecondSprite = AnotherTeamSprites[i];
            _Mod.level = (i + 1);
            _Mod.modType = modType;
            Mods.Add(_Mod);
        }
        return Mods;
    }
}

