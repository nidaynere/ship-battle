using EpPathFinding.cs;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TurnBasedFW
{
    public class Map
    {
        private int sizeX, sizeY;
        private StaticGrid grid;
        private JumpPointParam jParam;

        public Map(int _sizeX, int _sizeY, int _randomBlocksCount)
        {
            sizeX = _sizeX;
            sizeY = _sizeY;

            bool[][] movableMatrix = new bool[sizeX][];
            for (int widthTrav = 0; widthTrav < sizeX; widthTrav++)
            {
                movableMatrix[widthTrav] = new bool[sizeY];
                for (int heightTrav = 0; heightTrav < sizeY; heightTrav++)
                {
                    movableMatrix[widthTrav][heightTrav] = true;
                }
            }

            grid = new StaticGrid(sizeX, sizeY, movableMatrix);
            jParam = new JumpPointParam(grid, true, DiagonalMovement.OnlyWhenNoObstacles, HeuristicMode.MANHATTAN);

            // create random blocks.
            while (_randomBlocksCount > 0)
            {
                _randomBlocksCount--;

                // create random block.
                for (int i = 0; i < 2; i++)
                {
                    int[] pos = GetRandomPosition(i);
                    SetWalkable(pos[0], pos[1], false);
                }
            }
        }

        private int[] FixPosition(ref Vector3 position)
        {
            return new int[2] { Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z) };
        }

        public bool IsWalkable(int x, int y)
        {
            return grid.IsWalkableAt(x, y);
        }

        /// <summary>
        /// Get center of the map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector3 GetCenter()
        {
            return new Vector3(sizeX / 2, 0, sizeY / 2);
        }

        /// <summary>
        /// Set walkable
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void SetWalkable(int x, int y, bool value)
        {
            grid.SetWalkableAt(x, y, value);
        }

        public Vector3[] GetPath(Vector3 start, Vector3 end)
        {
            var fixedStart = FixPosition(ref start);
            var fixedEnd = FixPosition(ref end);

            GridPos startPos = new GridPos(fixedStart[0], fixedStart[1]);
            GridPos endPos = new GridPos(fixedEnd[0], fixedEnd[1]);

            jParam.Reset(startPos, endPos);

            List<Vector3> path;
            JumpPointFinder.FindPath(jParam, out path);

            return path.ToArray();
        }

        public bool CanSee(Vector3 start, Vector3 end)
        {
            var path = GetPath(start, end);
            if (path.Length <= 2)
                return true;

            var dir = (end - start).normalized;

            float distance = Vector3.Distance(end, start);
            for (int i = 1; i < distance-1; i++)
            {
                Vector3 point = start + dir * i;

                if (
                    !IsWalkable(Mathf.CeilToInt (point.x), Mathf.CeilToInt(point.z)) ||
                    !IsWalkable(Mathf.FloorToInt (point.x), Mathf.FloorToInt(point.z))
                    )
                {
                    return false;
                }
            }

            return true;
        }

        public int[] GetRandomPosition(int user)
        {
            int[] result = new int[2];

            int i = 100; // crash shield
            while (i > 0)
            {
                int randX = Random.Range(0, sizeX);
                int randY = user == 0 ?
                    Random.Range(0, sizeY / 2) :
                    Random.Range(sizeY / 2 + 1, sizeY);

                if (IsWalkable(randX, randY))
                {
                    result[0] = randX;
                    result[1] = randY;
                    break;
                }

                i--;
            }

            if (i == 0)
                Debug.LogError("[MAP] GetRandomPosition () => Saved from crash.");

            return result;
        }
    }
}
