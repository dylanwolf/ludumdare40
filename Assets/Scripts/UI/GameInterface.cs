using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour {

    public Text[] MinerCosts;
    public Text[] GeomancerCosts;
    public Text[] CrafterCosts;
    public Text[] SalesCosts;
    public GameObject StorageButton;
    public Text StorageCost;

    public Text Money;
    public Text RawMats;
    public Text Processed;
    public Text Crafted;

    public Image RawMatMeter;
    public Image ProcessedMeter;
    public Image CraftedMeter;
    public Color[] MeterColors;

	// Use this for initialization
	void Start () {
        InitializeFixedText();
	}

    LargeNumber num = new LargeNumber();
    private void InitializeFixedText()
    {
        

        for (var i = 0; i < MinerCosts.Length; i++)
        {
            num.SetValue(GameEngine.Current.AutoClickerCosts[i]);
            MinerCosts[i].text = num.ToShortString();
            GeomancerCosts[i].text = num.ToShortString();
            CrafterCosts[i].text = num.ToShortString();
            SalesCosts[i].text = num.ToShortString();
        }
    }

    // Update is called once per frame
    void Update () {

        Money.text = GameEngine.Current.Points.ToShortString();
        RawMats.text = GameEngine.Current.RawMaterials.ToShortString();
        Processed.text = GameEngine.Current.ProcessedMaterials.ToShortString();
        Crafted.text = GameEngine.Current.CraftedProducts.ToShortString();
        UpdateStorage();
        UpdateStorageGauge(RawMatMeter, GameEngine.Current.RawMatStorageGauge);
        UpdateStorageGauge(ProcessedMeter, GameEngine.Current.ProcessedStorageGauge);
        UpdateStorageGauge(CraftedMeter, GameEngine.Current.CraftedStorageGauge);
    }

    void UpdateStorage()
    {
        if (GameEngine.Current.StorageIndex < GameEngine.Current.StorageUpgradeCosts.Length)
        {
            if (!StorageButton.activeSelf)
                StorageButton.gameObject.SetActive(true);

            num.SetValue(GameEngine.Current.StorageUpgradeCosts[GameEngine.Current.StorageIndex]);
            StorageCost.text = num.ToShortString();
        }
        else
        {
            StorageButton.SetActive(false);
        }
    }

    void UpdateStorageGauge(Image fillBar, float pct)
    {
        fillBar.color = MeterColors[Mathf.Clamp(Mathf.FloorToInt(pct), 0, 2)];
        fillBar.fillAmount = Mathf.Clamp(pct / (GameEngine.Current.StoragePenalties.Length - 1), 0, 1);
    }

    public void ClickMine()
    {
        GameEngine.Current.Queue(GameActionType.Mine);
    }

    public void ClickProcess()
    {
        GameEngine.Current.Queue(GameActionType.Process);
    }

    public void ClickCrafting()
    {
        GameEngine.Current.Queue(GameActionType.Craft);
    }

    public void ClickSell()
    {
        GameEngine.Current.Queue(GameActionType.Sell);
    }

    public void BuyMiner(int level)
    {
        GameEngine.Current.BuyAutoClicker(GameActionType.Mine, level);
    }

    public void BuyProcesser(int level)
    {
        GameEngine.Current.BuyAutoClicker(GameActionType.Process, level);
    }

    public void BuyCrafter(int level)
    {
        GameEngine.Current.BuyAutoClicker(GameActionType.Craft, level);
    }

    public void BuySalesman(int level)
    {
        GameEngine.Current.BuyAutoClicker(GameActionType.Sell, level);
    }
}
