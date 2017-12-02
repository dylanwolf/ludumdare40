using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingOutput : MonoBehaviour {

    public Text RawValue;
    public Text ProcValue;
    public Text CraftValue;
    public Text PointsValue;

    public Dropdown BuyType;
    public Dropdown BuyLevel;

    // Update is called once per frame
    void Update () {
        RawValue.text = GameEngine.Current.RawMaterials.ToShortString();
        ProcValue.text = GameEngine.Current.ProcessedMaterials.ToShortString();
        CraftValue.text = GameEngine.Current.CraftedProducts.ToShortString();
        PointsValue.text = GameEngine.Current.Points.ToShortString();
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

    public void BuyAutoClicker()
    {
        GameEngine.Current.BuyAutoClicker((GameActionType)BuyType.value, BuyLevel.value);
    }
}
