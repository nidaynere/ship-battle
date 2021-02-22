using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public int MapSizeX = 8;
    public int MapSizeY = 16;
    public string PlayablesFolderOnResources;
    public int SpawnCount = 3;
    public int BlocksCount = 2;
    public float WorldScale = 2f;
    public float PlayerMoveSpeed = 3f;
    public bool[] UserAI = new bool[2];
    public float[] AIWaitTime = new float[] { 1, 3 };

    public float CameraDistanceView = 3;
    public float CameraDistanceZDropper = 2;

    [Range(0, 100)]
    public int AIRandomness = 20;
}