using TurnBasedFW;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;

    [SerializeField] private UISelection playablesSelection;

    [SerializeField] private Button playButton, startGamePlayButton;

    [SerializeField] private Transform gameHolder;

    [SerializeField] private Text gameStatusText;

    [SerializeField] private Text gameResultText;

    List<Playable> loadedPlayables;
    int currentPlayablesSelection;

    Dictionary<int, Dictionary<string, Player>> Users; 

    #region session variables
    Session session;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Load();
    }

    private void Load()
    {
        var reference = playablesSelection.transform.GetChild(0);

        // Load all playables.
        loadedPlayables = new List<Playable>();

        var playables = Resources.LoadAll<TextAsset>(gameSettings.PlayablesFolderOnResources);
        foreach (var playable in playables)
        {
            Playable p = JsonUtility.FromJson<Playable>(playable.text);
            var createdPlayable = Instantiate(reference, playablesSelection.transform);
            createdPlayable.GetComponentInChildren<Text>().text = p.Name;

            loadedPlayables.Add(p);
        }

        // remove prefab.
        reference.gameObject.SetActive(false);

        playablesSelection.Initialize(1);

        currentPlayablesSelection = 0;
        playablesSelection.OnValueChanged.AddListener((val) => { currentPlayablesSelection = val - 1; });

        playButton.onClick.AddListener(() => {
            CreateSession();
        });

        startGamePlayButton.onClick.AddListener(() => {
            if (session == null)
            {
                Debug.LogError("Create a session first.");
                return;
            }
            session.GameInput.StartGamePlay();
        });
    }

    private void PlayAsAI(int user)
    {
        StartCoroutine(AIPlayer(user));
    }

    IEnumerator AIPlayer(int user)
    {
        ///random wait time for AI;
        yield return new WaitForSeconds(Random.Range(gameSettings.AIWaitTime[0], gameSettings.AIWaitTime[1]));

        if (gameSettings.UserAI[user])
        {
            Debug.Log("Playing as AI " + user);
            // AI attack.

            // find closest. Or maybe a little bit randomness? im gonna make it %50-%50

            Player attacker = null, defender = null;

            if (Random.Range(0, 100) >= gameSettings.AIRandomness)
            { // Smart decision
                float lastDistance = 10000;
                Users[user].Values.ToList().ForEach(c =>
                {
                    Vector2 cPos = new Vector2(c.Playable.PosX, c.Playable.PosY);

                    Users[user == 1 ? 0 : 1].Values.ToList().ForEach(r =>
                    {
                        Vector2 rPos = new Vector2(r.Playable.PosX, r.Playable.PosY);

                        var dist = Vector2.Distance(cPos, rPos);
                        if (dist < lastDistance)
                        {
                            lastDistance = dist;
                            attacker = c;
                            defender = r;
                        }
                    });
                });
            }

            else
            {
                //Random play.
                var attackerPlayers = Users[user].Values.ToList();
                int length = attackerPlayers.Count;

                attacker = attackerPlayers[Random.Range(0, length)];

                var defenderPlayers = Users[user == 1 ? 0 : 1].Values.ToList();
                length = defenderPlayers.Count;

                defender = defenderPlayers[Random.Range(0, length)];
            }

            if (attacker != null && defender != null)
            {
                // attack, if it can see it.
                session.GameInput.PlayableAttack(user, attacker.Playable.ID, defender.Playable.ID);
            }
        }
    }

    private void CreateSession()
    {
        // for possible AI Wait.
        StopAllCoroutines();

        // Clear game result.
        gameResultText.text = "";

        // clear old session.
        int count = gameHolder.childCount;
        var gonnaClear = new Transform[count];
        for (int i = 0; i < count; i++)
            gonnaClear[i] = gameHolder.GetChild(i);
        for (int i = 0; i < count; i++)
            Destroy(gonnaClear[i].gameObject);
        //

        // Init game.
        session = new Session();

        // register game outputs.

        session.GameOutput.OnPlayablePathfind = (user, playable, path) => {
            Debug.Log("OnPlayablePathfind => path length: " + path.Length);

            if (path.Length != 0)
            {
                Users[user][playable.ID].ApplyPath(gameSettings.WorldScale, path, gameSettings.PlayerMoveSpeed, () => {
                    // next turn.
                    session.GameInput.NextTurn();
                });
            }
        };

        session.GameOutput.OnGameStatusUpdate = (status) => {
            gameStatusText.text = status.ToString();

            int user = -1;

            if (status == Game.Status.User1Turn)
            {
                user = 0;
            }

            else if (status == Game.Status.User2Turn)
            {
                user = 1;
            }

            Debug.Log("Target user => " + user);

            if (user != -1)
            {
                if (gameSettings.UserAI[user])
                {
                    PlayAsAI(user);
                }
                else
                {// User play, not implemented :(
                    throw new System.NotImplementedException();
                }
            }
        };

        session.GameOutput.OnMapInitialized = (Map map) =>
        {
            // adjust camera by map size.
            Camera.main.transform.position = Vector3.zero;

            var camPos = Camera.main.transform.position;
            var mapCenter = map.GetCenter() * gameSettings.WorldScale;
            camPos.x = mapCenter.x;
            camPos.z = mapCenter.z - gameSettings.CameraDistanceZDropper;
            Camera.main.transform.position = camPos;

            Vector3 cameraDirection = Camera.main.transform.forward;
            var hipo = mapCenter.magnitude * gameSettings.WorldScale;
            Camera.main.transform.position -= hipo * cameraDirection * gameSettings.CameraDistanceView;
            //

            // create map.
            var assets = Resources.LoadAll <Transform>("Blocks/");
            int assetsCount = assets.Length;

            var grid = Resources.Load<Transform>("Grid");

            for (int x = 0; x < gameSettings.MapSizeX; x++)
            {
                for (int y = 0; y < gameSettings.MapSizeY; y++)
                {
                    // create grid point.
                    var gridPoint = Instantiate(grid, gameHolder);
                    gridPoint.transform.localScale = Vector3.one * gameSettings.WorldScale / 0.9f;
                    gridPoint.transform.position = new Vector3(x, 0.01f, y) * gameSettings.WorldScale;

                    var isBlocked = !map.IsWalkable(x, y);

                    if (isBlocked)
                    {
                        var block = Instantiate(assets[Random.Range(0, assetsCount)], gameHolder);
                        block.position = new Vector3(x * gameSettings.WorldScale, 0, y * gameSettings.WorldScale);
                    }
                }
            }
        };

        session.GameOutput.OnGameFinished = (user) => {
            Debug.Log("Game is ended.");
            gameResultText.text = "game is end, winner is " + user;
        };

        session.GameOutput.OnHealthUpdate = (user, playable, health, maxHealth) => {
            Users[user][playable.ID].Filler.fillAmount = (float)health / maxHealth;
        };

        session.GameOutput.OnDamageEffect = (attacker, playable1, defender, playable2) =>
        {
            var attack = Resources.Load<Transform>("Effects/Attack");
            var damage = Resources.Load<Transform>("Effects/Damage");

            var effect1 = Instantiate (attack, Users[attacker][playable1.ID].transform.position, Quaternion.identity);
            var effect2 = Instantiate(damage, Users[defender][playable2.ID].transform.position, Quaternion.identity);

            Destroy(effect1.gameObject, 5);
            Destroy(effect2.gameObject, 5);
        };

        session.GameOutput.OnPlayableDestroyed = (user, playable) => {
            var killed = Users[user][playable.ID];
            Users[user].Remove(playable.ID);
            Destroy(killed.gameObject);
        };

        Users = new Dictionary<int, Dictionary<string, Player>>();

        session.GameOutput.OnPlayableSpawned = (user, spawned) => {
            // create new player object.
            var newPlayable = new GameObject(user + "_playable_ " + spawned.ID);
            newPlayable.transform.SetParent(gameHolder);

            // create rotator.
            var rotator = new GameObject("rotator");
            rotator.transform.SetParent(newPlayable.transform);
            rotator.transform.localPosition = Vector3.zero;

            // create health UI.
            var healthUI = Resources.Load<GameObject>("PlayerUI");
            healthUI = Instantiate(healthUI);
            healthUI.transform.SetParent(newPlayable.transform);
            healthUI.transform.localPosition = Vector3.zero;

            var player = newPlayable.AddComponent<Player>();
            player.Set ( spawned, rotator, healthUI );

            var asset = Resources.Load<GameObject>("Players/" + spawned.AssetId);
            asset = Instantiate(asset, rotator.transform);
            asset.transform.localPosition = Vector3.zero;
            
            newPlayable.transform.position = new Vector3(spawned.PosX*gameSettings.WorldScale, 0, spawned.PosY * gameSettings.WorldScale);

            rotator.transform.rotation = Quaternion.Euler(0, user == 0 ? 0 : 180, 0);

            if (!Users.ContainsKey(user))
            {
                Users.Add(user, new Dictionary<string, Player>());    
            }

            Users[user].Add(spawned.ID, player);
        };

        var user1Selection = loadedPlayables[currentPlayablesSelection];
        var others = loadedPlayables.FindAll(x => x != user1Selection);
        var user2Selection = others[Random.Range(0, others.Count)];
        // initialize.
        session.Initialize(gameSettings.MapSizeX,
            gameSettings.MapSizeY,
            new Playable[2] {
                    user1Selection,
                    user2Selection
            },
            gameSettings.SpawnCount,
            gameSettings.BlocksCount);
    }

#if UNITY_EDITOR
    [SerializeField] private bool MapDebugger = false;
    private void OnGUI()
    {
        if (!MapDebugger)
            return;

        if (session == null)
            return;

        // visualize session map.
        var map = session.Game.GetMap;
        var size = 30;
        for (int x = 0; x < gameSettings.MapSizeX; x++)
        {
            for (int y = 0; y < gameSettings.MapSizeY; y++)
            {
                var isBlocked = !map.IsWalkable(x, y);
                GUI.color = isBlocked ? Color.red : Color.green;
                GUI.Box (new Rect(x*size, (gameSettings.MapSizeY - y) * size, size, size), "O");
            }
        }
    }
#endif
}
