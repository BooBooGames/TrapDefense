using AssetKits.ParticleImage;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIParticleEffectsManager : MonoBehaviour
{
    private static UIParticleEffectsManager uiParticleEffectsManager;
    public static UIParticleEffectsManager Instance => uiParticleEffectsManager;

    [SerializeField] public RectTransform center;

    [SerializeField] ParticleImage coinEffect, gearEffect, healthEffect, confettiBurst;
    public ParticleImage upgradeProgressBarEffect, fullScreenConfetti;

    [SerializeField] TextMeshProUGUI rewardCountText;
    [SerializeField] RectTransform rewardCountTextRT;
    float checkTimeOut = 3f;

    public ParticleImage coinSpendEffect;

    private void Awake()
    {
        if (uiParticleEffectsManager == null)
        {
            uiParticleEffectsManager = this;
        }
    }

    private void Update()
    {
        /*checkTimeOut -= Time.deltaTime;
        if(checkTimeOut <= 0)
        {
            checkTimeOut = 0.3f;
            if (!Input.GetMouseButton(0))
            {
                upgradeProgressBarEffect.Stop();
                cashBurstEffect.Stop();
            }
        }*/
    }

    public void PlayCoinEffect(Vector3 effectPosition)
    {
        PlayParticleEffect(coinEffect, effectPosition);
    }

    public void PlayGearEffect(Vector3 effectPosition)
    {
        // PlayParticleEffect(gearEffect, effectPosition);
        PlayParticleEffect(gearEffect, center.position);
    }

    public void PlayHealthEffect(Vector3 effectPosition)
    {
        // PlayParticleEffect(healthEffect, effectPosition);
        PlayParticleEffect(healthEffect, center.position);
    }

    // cash busrt effect on button press

    public void PlayConfettiBurstEffect(Transform effectPosTransform)
    {
        // confettiBurst.SetParent(effectPosTransform.GetComponentInParent<SafeArea>().transform);
        PlayParticleEffect(confettiBurst, effectPosTransform.position);
    }

    public void PlayCoinSpendEffect(Vector3 effectPosition)
    {
        PlayParticleEffect(coinSpendEffect, effectPosition);
    }

    private async void PlayParticleEffect(ParticleImage effect, Vector3 effectPosition)
    {
        Debug.Log("PlayParticleEffect at " + effect.name);

        SoundManager.PlayAudio(AudioType.Get_Reward);
        effect.transform.position = effectPosition;
        effect.Play();

        await Awaitable.WaitForSecondsAsync(1f);

        SoundManager.PlayAudio(AudioType.Reward_Animation);
    }

    public void ShowRewardCountText(int cashAmount, int gemsAmount, int ticketsAmount, Vector3 pos)
    {
        rewardCountText.DOKill();
        rewardCountTextRT.DOKill();
        rewardCountText.gameObject.SetActive(true);
        string rewardTextString = "";
        if (cashAmount > 0)
        {
            // rewardTextString += string.Format("<color=#1AE829>+${0}</color><br>", MoneyManager.instance.ToKMB(cashAmount));
        }

        if (gemsAmount > 0)
        {
            // rewardTextString += string.Format("<color=#F144D0>+{0}</color><br>", MoneyManager.instance.ToKMB(gemsAmount));
        }

        if (ticketsAmount > 0)
        {
            // rewardTextString += string.Format("<color=#FFFFFF>+{0}</color>", MoneyManager.instance.ToKMB(ticketsAmount));
        }
        rewardCountText.text = rewardTextString;
        rewardCountText.DOFade(1, 0.05f);
        rewardCountTextRT.transform.position = pos;
        rewardCountTextRT.transform.DOMoveY(pos.y + 600, 1.5f);
        rewardCountText.DOFade(0, 0.25f).SetDelay(1.25f).OnComplete(() =>
        {
            rewardCountText.gameObject.SetActive(false);
        });
    }

    public void PlayFullScreenConfettiEffect()
    {
        fullScreenConfetti.gameObject.SetActive(false);
        fullScreenConfetti.gameObject.SetActive(true);
    }

    /*     public void StopProgressBarAndCashEffect()
        {
            upgradeProgressBarEffect.transform.SetParent(coinEffect.transform.parent);
            cashBurstEffect.Stop();
            upgradeProgressBarEffect.Stop();
        } */

    /*  public void PlayProgressBarAndCashEffect()
     {
         if (cashBurstEffect.particles.Count < 30)
         {
             cashBurstEffect.Play();
         }
         if (upgradeProgressBarEffect.particles.Count < 70)
         {
             upgradeProgressBarEffect.Play();
         }
     } */
}
