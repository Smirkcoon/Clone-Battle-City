using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Util
    {
        /// <summary>
        /// Get closest Transform from list
        /// </summary>
        /// <param name="GOlist"></param>
        /// <param name="_transform"></param>
        /// <returns></returns>
        public static Transform GetClosestTransform(List<GameObject> GOlist, Transform _transform)
        {
            if (GOlist == null || GOlist.Count == 0)
                return null;

            Transform TClosest = GOlist[0].transform;
            foreach (GameObject go in GOlist)
            {
                if (Vector2.Distance(_transform.position, TClosest.position) > Vector2.Distance(_transform.position, go.transform.position))
                {
                    TClosest = go.transform;
                }
            }
            return TClosest;
        }
        /// <summary>
        /// Get closest List Transforms from List with ScanDistance
        /// </summary>
        /// <param name="Tlist"></param>
        /// <param name="ReturnCount"></param>
        /// <param name="ScanDist"></param>
        /// <param name="_transform"></param>
        /// <returns></returns>
        public static List<Transform> GetScanDistListTransform(List<Transform> Tlist, int ReturnCount, int ScanDist, Transform _transform)
        {
            if (Tlist == null || Tlist.Count == 0)
                return null;

            List<Transform> TClosest = new List<Transform>();
            for (int i = 0; i < ReturnCount && i < Tlist.Count; i++)
            {
                if (Vector2.Distance(_transform.position, Tlist[i].position) < ScanDist)
                {
                    TClosest.Add(Tlist[i]);
                }
            }

            return TClosest;
        }
        /// <summary>
        /// check List TankControllers , if Null Or Empty return true
        /// </summary>
        /// <param name="TPList"></param>
        /// <returns></returns>
        public static bool CheckListIsNullOrEmpty(List<TankController> TPList)
        {
            return (TPList == null || TPList.Count == 0);
        }
        /// <summary>
        /// Check and return isWalkable Cells Left and Right of position 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pathfinding"></param>
        /// <returns></returns>
        public static PathCell GetFreeCellNearPos(Vector2 pos, Pathfinding pathfinding)
        {
            PathCell Right = pathfinding.GetCell((int)pos.x + 1, (int)pos.y);
            if (Right != null && Right.isWalkable)
            {
                return Right;
            }
            else
            {
                PathCell Left = pathfinding.GetCell((int)pos.x - 1, (int)pos.y);
                if (Left != null && Left.isWalkable)
                {
                    return Left;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
