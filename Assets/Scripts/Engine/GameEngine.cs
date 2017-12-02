using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour {

    public static GameEngine Current;
    private void Awake()
    {
        Current = this;
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
    public long[] StorageAmounts = new long[] { 25, 250, 2500, 10000 };
    public float[] StorageLevels = new float[] { 1, 0.75f, 0.5f, 0.25f };
    public int StorageIndex = 0;
    public float ActionMultiplier = 1.0f;

    private void ResetGame()
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
    }

    public void Queue(GameActionType action)
    {
        Clicks.Enqueue(action);
    }

    public void CalculateMultiplier()
    {
        ActionMultiplier =
            StorageLevels[Mathf.Clamp((int)Mathf.Floor(RawMaterials.GetValue() / (float)StorageAmounts[StorageIndex]), 0, StorageLevels.Length - 1)] *
            StorageLevels[Mathf.Clamp((int)Mathf.Floor(ProcessedMaterials.GetValue() / (float)StorageAmounts[StorageIndex]), 0, StorageLevels.Length - 1)] *
            StorageLevels[Mathf.Clamp((int)Mathf.Floor(CraftedProducts.GetValue() / (float)StorageAmounts[StorageIndex]), 0, StorageLevels.Length - 1)];
    }

    public long GetMultipliedValue(long value)
    {
        var x = (long)(Mathf.Round(value * ActionMultiplier));
        if (x < 1) x = 1;
        return x;
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
        if (ProcessedMaterials.GetValue() >= AutoClickerCosts[level])
        {
            ProcessedMaterials.Add(-AutoClickerCosts[level]);
            switch (actionType)
            {
                case GameActionType.Mine:
                    MineAutoClickers[level]++;
                    break;
                case GameActionType.Process:
                    ProcessAutoClickers[level]++;
                    break;
                case GameActionType.Craft:
                    CraftAutoClickers[level]++;
                    break;
                case GameActionType.Sell:
                    SellAutoClickers[level]++;
                    break;
            }
        }
    }

    public void UpgradeStorage()
    {
        if (StorageIndex < StorageLevels.Length - 1 &&
            ProcessedMaterials.GetValue() >= StorageUpgradeCosts[StorageIndex])
        {
            ProcessedMaterials.Add(-StorageUpgradeCosts[StorageIndex]);
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

    void ProcessAutoClicker(GameActionType actionType, long[] clickerCounts, long[] clickerOutputs)
    {
        for (long i = 0; i < clickerCounts.Length; i++)
        {
            for (long j = 0; j < clickerCounts[i]; j++)
            {
                HandleAction(actionType, clickerOutputs[i], true);
            }
        }
    }

    void Tick()
    {
        CalculateMultiplier();

        // Process all clicks
        while (Clicks.Count > 0)
        {
            HandleAction(Clicks.Dequeue(), 1, false);
        }

        // Process autoclickers
        ProcessAutoClicker(GameActionType.Mine, MineAutoClickers, MineAutoClickerOutput);
        ProcessAutoClicker(GameActionType.Process, ProcessAutoClickers, ProcessAutoClickerOutput);
        ProcessAutoClicker(GameActionType.Craft, CraftAutoClickers, CraftAutoClickerOutput);
        ProcessAutoClicker(GameActionType.Sell, SellAutoClickers, SellAutoClickerOutput);

        // Enable/disabled click controls based on required mats
    }


    // Update is called once per frame
    void Update () {
        timer += Time.deltaTime;
        while (timer > TimePerTick)
        {
            Tick();
            timer -= TimePerTick;
        }
	}
}
