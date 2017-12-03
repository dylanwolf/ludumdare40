using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour {

    public static GameEngine Current;
    private void Awake()
    {
        Current = this;
        InitializeSprites();
    }

    private void Start()
    {
        ResetGame();
    }

    public float TimePerTick = 1.0f;
    float timer = 0;

    public LargeNumber RawMaterials = new LargeNumber(0);
    public LargeNumber ProcessedMaterials = new LargeNumber(0);
    public LargeNumber CraftedProducts = new LargeNumber(0);
    public LargeNumber Points = new LargeNumber(0);

    public long[] MineAutoClickers = new long[] { 0, 0, 0, 0 };
    public long[] ProcessAutoClickers = new long[] { 0, 0, 0, 0 };
    public long[] CraftAutoClickers = new long[] { 0, 0, 0, 0 };
    public long[] SellAutoClickers = new long[] { 0, 0, 0, 0 };

    public Queue<GameActionType> Clicks = new Queue<GameActionType>();

    public long[] AutoClickerCosts = new long[] { 10, 100, 1000, 100000 };
    public long[] MineAutoClickerOutput = new long[] { 1, 100, 10000, 5000000 };
    public long[] ProcessAutoClickerOutput = new long[] { 1, 50, 1000, 200000 };
    public long[] CraftAutoClickerOutput = new long[] { 1, 25, 500, 25000 };
    public long[] SellAutoClickerOutput = new long[] { 1, 10, 100, 1000 };

    public long[] StorageUpgradeCosts = new long[] { 10, 100, 1000, 100000 };
    public long[] StorageAmounts = new long[] { 25, 250, 2500, 10000, 1000000 };
    public float[] StoragePenalties = new float[] { 1, 0.75f, 0.5f, 0.25f };
    public int StorageIndex = 0;
    public float ActionMultiplier = 1.0f;

    public float RawMatStorageGauge = 0;
    public float ProcessedStorageGauge = 0;
    public float CraftedStorageGauge = 0;

    public Rect MinerSpawnPoint;
    public Rect GeomancerSpawnPoint;
    public Rect ArtificerSpawnPoint;
    public Rect SalesmanSpawnPoint;

    Dictionary<string, List<Animator>> sprites = new Dictionary<string, List<Animator>>();
    Dictionary<GameActionType, string> pools = new Dictionary<GameActionType, string>()
    {
        { GameActionType.Mine, MINER },
        { GameActionType.Process, PROCESSOR },
        { GameActionType.Craft, CRAFTER },
        { GameActionType.Sell, SELLER },
    };
    const string MINER = "WizardPickaxe";
    const string PROCESSOR = "Geomancer";
    const string CRAFTER = "Artificer";
    const string SELLER = "Salesman";

    void InitializeSprites()
    {
        foreach (var val in pools.Values)
            sprites[val] = new List<Animator>();

    }

    Animator tmpAnim;
    void ClearSprites()
    {
        foreach (var key in sprites.Keys)
        {
            while (sprites[key].Count > 0)
            {
                ObjectPools.Despawn(sprites[key][0].gameObject);
                sprites[key].RemoveAt(0);
            }
        }
    }

    Vector3 GenerateRandomPoint(Rect boundaries)
    {
        return Camera.main.ViewportToWorldPoint(new Vector3(
                Random.Range(boundaries.min.x, boundaries.max.x),
                Random.Range(boundaries.min.y, boundaries.max.y),
                10
            ));
    }

    public int MAX_SPRITES = 100;

    void SpawnSprite(GameActionType actionType, int level, Vector3 position)
    {
        if (pools.ContainsKey(actionType) && sprites[pools[actionType]].Count < MAX_SPRITES)
        {
            tmpAnim = ObjectPools.Spawn<Animator>(pools[actionType], position, null);
            tmpAnim.speed = 1 + (level * 0.5f);
            tmpAnim.transform.localScale = new Vector3(2 * (Random.value > 0.5f ? 1 : -1), 2, 1);
            sprites[pools[actionType]].Add(tmpAnim);
        }
    }

    public void ResetGame()
    {
        RawMaterials.SetValue(0);
        ProcessedMaterials.SetValue(0);
        CraftedProducts.SetValue(0);
        Points.SetValue(0);

        Clicks.Clear();
        timer = 0;

        MineAutoClickers = new long[] { 0, 0, 0, 0 };
        ProcessAutoClickers = new long[] { 0, 0, 0, 0 };
        CraftAutoClickers = new long[] { 0, 0, 0, 0 };
        SellAutoClickers = new long[] { 0, 0, 0, 0 };

        StorageIndex = 0;
        ActionMultiplier = 1.0f;
        ClearSprites();
    }

    public void Queue(GameActionType action)
    {
        Clicks.Enqueue(action);
    }

    public void CalculateMultiplier()
    {
        RawMatStorageGauge = RawMaterials.GetValue() / (float)StorageAmounts[StorageIndex];
        ProcessedStorageGauge = ProcessedMaterials.GetValue() / (float)StorageAmounts[StorageIndex];
        CraftedStorageGauge = CraftedProducts.GetValue() / (float)StorageAmounts[StorageIndex];

        ActionMultiplier =
            StoragePenalties[Mathf.Clamp((int)Mathf.Floor(RawMatStorageGauge), 0, StoragePenalties.Length - 1)] *
            StoragePenalties[Mathf.Clamp((int)Mathf.Floor(ProcessedStorageGauge), 0, StoragePenalties.Length - 1)] *
            StoragePenalties[Mathf.Clamp((int)Mathf.Floor(CraftedStorageGauge), 0, StoragePenalties.Length - 1)];
    }

    public long GetMultipliedValue(long value)
    {
        return value;
        //var x = (long)(Mathf.Round(value * ActionMultiplier));
        //if (value > 0 && x < 1) x = 1;
        //return x;
    }

    public void MineItem(long amount, bool applyPenalty)
    {
        // TODO: Animation
        RawMaterials.Add(applyPenalty ? GetMultipliedValue(amount) : amount);
    }

    public void ProcessItem(long amount, bool applyPenalty)
    {
        // TODO: Animation
        var total = applyPenalty ? GetMultipliedValue(amount) : amount;
        if (RawMaterials < total)
            total = RawMaterials.GetValue();
        RawMaterials.Add(-total);
        ProcessedMaterials.Add(total);
    }

    public void CraftItem(long amount, bool applyPenalty)
    {
        // TODO: Animation
        var total = applyPenalty ? GetMultipliedValue(amount) : amount;
        if (ProcessedMaterials < total)
            total = ProcessedMaterials.GetValue();
        ProcessedMaterials.Add(-total);
        CraftedProducts.Add(total);
    }

    public void SellItem(long amount, bool applyPenalty)
    {
        // TODO: Animation
        var total = applyPenalty ? GetMultipliedValue(amount) : amount;
        if (CraftedProducts < total)
            total = CraftedProducts.GetValue();
        CraftedProducts.Add(-total);
        Points.Add(total);
    }

    public void BuyAutoClicker(GameActionType actionType, int level)
    {
        if (Points.GetValue() >= AutoClickerCosts[level])
        {
            Points.Add(-AutoClickerCosts[level]);
            switch (actionType)
            {
                case GameActionType.Mine:
                    MineAutoClickers[level]++;
                    SpawnSprite(GameActionType.Mine, level, GenerateRandomPoint(MinerSpawnPoint));
                    break;
                case GameActionType.Process:
                    ProcessAutoClickers[level]++;
                    SpawnSprite(GameActionType.Process, level, GenerateRandomPoint(GeomancerSpawnPoint));
                    break;
                case GameActionType.Craft:
                    CraftAutoClickers[level]++;
                    SpawnSprite(GameActionType.Craft, level, GenerateRandomPoint(ArtificerSpawnPoint));
                    break;
                case GameActionType.Sell:
                    SellAutoClickers[level]++;
                    SpawnSprite(GameActionType.Sell, level, GenerateRandomPoint(SalesmanSpawnPoint));
                    break;
            }
        }
    }

    public void UpgradeStorage()
    {
        if (StorageIndex < StorageUpgradeCosts.Length &&
            Points.GetValue() >= StorageUpgradeCosts[StorageIndex])
        {
            Points.Add(-StorageUpgradeCosts[StorageIndex]);
            StorageIndex++;
        }
    }

    public void HandleAction(GameActionType actionType, long amount, bool applyPenalty)
    {
        switch (actionType)
        {
            case GameActionType.Mine:
                MineItem(amount, applyPenalty);
                break;

            case GameActionType.Process:
                ProcessItem(amount, applyPenalty);
                break;

            case GameActionType.Craft:
                CraftItem(amount, applyPenalty);
                break;

            case GameActionType.Sell:
                SellItem(amount, applyPenalty);
                break;
        }
    }

    void ProcessAutoClicker(GameActionType actionType, long[] clickerCounts, long[] clickerOutputs, long minimumOutput = 0)
    {
        long totalOutput = 0;

        for (long i = 0; i < clickerCounts.Length; i++)
        {
            totalOutput += (clickerOutputs[i] * clickerCounts[i]);
        }

        if (totalOutput < minimumOutput)
            totalOutput = minimumOutput;

        HandleAction(actionType, totalOutput, true);
    }

    void Tick()
    {
        // Process autoclickers
        ProcessTick(GameActionType.Sell);
        ProcessTick(GameActionType.Craft);
        ProcessTick(GameActionType.Process);
        ProcessTick(GameActionType.Mine);
    }

    void ProcessTick(GameActionType actionType, int minimumOutput = 0)
    {
        switch (actionType)
        {
            case GameActionType.Sell:
                ProcessAutoClicker(GameActionType.Sell, SellAutoClickers, SellAutoClickerOutput, minimumOutput);
                break;

            case GameActionType.Craft:
                ProcessAutoClicker(GameActionType.Craft, CraftAutoClickers, CraftAutoClickerOutput, minimumOutput);
                break;

            case GameActionType.Process:
                ProcessAutoClicker(GameActionType.Process, ProcessAutoClickers, ProcessAutoClickerOutput, minimumOutput);
                break;

            case GameActionType.Mine:
                ProcessAutoClicker(GameActionType.Mine, MineAutoClickers, MineAutoClickerOutput, minimumOutput);
                break;
        }
    }


    // Update is called once per frame
    void Update () {
        CalculateMultiplier();

        // Process all clicks
        while (Clicks.Count > 0)
        {
            ProcessTick(Clicks.Dequeue(), 1);
            //HandleAction(Clicks.Dequeue(), 1, false);
        }

        timer += (Time.deltaTime * ActionMultiplier);
        while (timer > TimePerTick)
        {
            Tick();
            timer -= TimePerTick;
        }
	}
}
