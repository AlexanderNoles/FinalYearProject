using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EntityAndDataDescriptor;
using System;

public class MainInfoUIControl : PostTickUpdate
{
    public static MainInfoUIControl instance;

    private const float sideBarMax = 0.4f;

    public ClockControl clockEffect;

    public Image engineBarImage;
    public GameObject engineArcaneFlame;
    private float engineBarTarget = 0.0f;

    public Image salvoBarImage;
    private float salvoBarTarget = 0.0f;

    [Header("Health")]
    public Image healthBarImage;
    public TextMeshProUGUI currentHealthLabel;
    public TextMeshProUGUI maxHealthLabel;

    [Header("Currency")]
    public TextMeshProUGUI currencyLabel;

	[Header("Population")]
	public TextMeshProUGUI populationLabel;

	[Header("Military")]
	public TextMeshProUGUI militaryLabel;

	[Header("Emblem")]
    public EmblemRenderer emblemRenderer;
    private bool emblemDrawn = false;

    private void Awake()
    {
        instance = this;
    }

    protected override void Update()
    {
        base.Update();

        if (engineBarImage.fillAmount != engineBarTarget)
        {
            engineBarImage.fillAmount = SmartExpMove(engineBarImage.fillAmount, engineBarTarget);
        }

        if (salvoBarImage.fillAmount != salvoBarTarget)
        {
            salvoBarImage.fillAmount = SmartExpMove(salvoBarImage.fillAmount, salvoBarTarget);
        }
    }

    protected override void PostTick()
    {
        Draw();
    }

    public static void ForceRedraw()
    {
        if (instance == null)
        {
            return;
        }

        instance.Draw();
    }

	public static void ForceCurrencyRedraw()
	{
		if (instance == null)
		{
			return;
		}

		instance.RedrawCurrencyLabel();
	}

	public static void ForceHealthBarRedraw()
	{
		if (instance == null)
		{
			return;
		}

		instance.DrawHealthAuto();
	}

    private void Draw()
    {
		clockEffect.UpdateClockPosition();
        DrawHealthAuto();

        if (PlayerManagement.PlayerEntityExists())
        {
			RedrawCurrencyLabel();
			RedrawPopulationLabel();
			RedrawMilitaryLabel();

			if (!emblemDrawn)
            {
                //Only need to draw emblem once, not every post tick
                PlayerManagement.GetTarget().GetData(DataTags.Emblem, out EmblemData emblemData);

                emblemDrawn = emblemRenderer.Draw(emblemData);
            }
        }
    }

	private void RedrawCurrencyLabel()
	{
		double value = Math.Round(PlayerManagement.GetInventory().mainCurrency, 1);
		string text = value.ToString();

		//No remainder
		if (value % 1.0f == 0.0f)
		{
			text += ".0";
		}

		currencyLabel.text = text;
	}

	private void RedrawPopulationLabel()
	{
		populationLabel.text = Mathf.RoundToInt(PlayerManagement.GetPopulation().currentPopulationCount).ToString();
	}

	private void RedrawMilitaryLabel()
	{
		MilitaryData mil = PlayerManagement.GetMilitary();
		militaryLabel.text = $"(     {mil.FullyRepairedFleetsCount(mil.reserveFleets)}/{Mathf.FloorToInt(mil.maxMilitaryCapacity)})";
	}

	private void DrawHealthAuto()
    {
		if (!PlayerManagement.PlayerEntityExists())
		{
			return;
		}

        //Get current and max player health
        PlayerStats playerStats = PlayerManagement.GetStats();

        DrawHealthDirect(PlayerSimObjBehaviour.GetCurrentHealth(), playerStats.GetStat(Stats.maxHealth.ToString()));
    }

    private void DrawHealthDirect(float currentHealth, float maxHealth)
    {
        float percentage = Mathf.Clamp01(currentHealth / maxHealth);

        healthBarImage.fillAmount = percentage;
		currentHealthLabel.text = UIHelper.ConvertToNiceNumberString(Mathf.Max(0.0f, Mathf.Round(currentHealth)), 0);
        maxHealthLabel.text = UIHelper.ConvertToNiceNumberString(maxHealth, 0);
    }

    private float SmartExpMove(float start, float target, float moveSpeedModifier = 10.0f)
    {
        float difference = Mathf.Abs(start - target);
        float moveThisFrame = Time.deltaTime * Mathf.Max(difference * moveSpeedModifier, 0.01f);
        return Mathf.MoveTowards(start, target, moveThisFrame);
    }

    public static void UpdateEngineBarInensity(float percentage)
    {
        percentage = Mathf.Clamp01(percentage) * sideBarMax;

        instance.engineBarTarget = percentage;
    }

    public static void UpdateSalvoBarInensity(float percentage)
    {
        percentage = Mathf.Clamp01(percentage) * sideBarMax;

        instance.salvoBarTarget = percentage;
    }

    public static void UpdateFuelLabel(float newValue)
    {
        //Not implemented
        //Not needed as fuel has been disabled, if fuel is re-enabled and ui is needed, implement this function
        throw new System.NotImplementedException();
    }

    public static void SetEngineArcaneFlameActive(bool _bool)
    {
        instance.engineArcaneFlame.SetActive(_bool);
    }
}
