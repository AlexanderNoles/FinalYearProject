using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EntityAndDataDescriptor;

public class MainInfoUIControl : PostTickUpdate
{
    public static MainInfoUIControl instance;

    private const float sideBarMax = 0.4f;

    public TextMeshProUGUI dateLabel;

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
        dateLabel.text = SimulationManagement.GetDateString();
        DrawHealthAuto();

        if (PlayerManagement.PlayerEntityExists())
        {
			RedrawCurrencyLabel();

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
		currencyLabel.text = PlayerManagement.GetInventory().mainCurrency.ToString();
	}

    private void DrawHealthAuto()
    {
		if (!PlayerManagement.PlayerEntityExists())
		{
			return;
		}

        //Get current and max player health
        PlayerStats playerStats = PlayerManagement.GetStats();

        DrawHealthDirect(PlayerBattleBehaviour.GetCurrentHealth(), playerStats.GetStat(Stats.maxHealth.ToString()));
    }

    private void DrawHealthDirect(float currentHealth, float maxHealth)
    {
        float percentage = Mathf.Clamp01(currentHealth / maxHealth);

        healthBarImage.fillAmount = percentage;
        currentHealthLabel.text = Mathf.Max(0.0f, Mathf.Round(currentHealth)).ToString();
        maxHealthLabel.text = maxHealth.ToString();
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
