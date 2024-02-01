using System;
using System.Collections;
using Custard;
using UnityEngine;
using UnityEngine.UI;

public class LocalCustardListener : MonoBehaviour
{
    public WorldCells worldCells;
    public CustardState custardState;
    public int HeightTillCovered;
    private Player player;
    [SerializeField] private Image inCustardEffect;
    [SerializeField] private Image inCustardEffectRight;

    public bool InsideCustard;
    public bool CoveredByCustard;

    public float graceTimeForBeingInsideOrOutsideCustard = .6f;

    public float timeBeforePlayerTakesDamageFromDrawning;
    public float timeBetweenDamageTicks;
    public int drowningDamage;
    private float timePassedSinceCoveredByCustard = 0.0f;
    private float timePassedSinceInsideCustardChange = 0.0f;
    private float timePassedSinceLastDamage = 0.0f;
    private Coroutine inCustardEffectFader;
    [SerializeField] private float _fadeInDuration = 2.5f;
    [SerializeField] private float _fadeOutDuration = 1.5f;

    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
        HideInCustardEffectImmediately();
    }

    private void HideInCustardEffectImmediately()
    {
        var colorLeft = inCustardEffect.color;
        inCustardEffect.color = new Color(colorLeft.r, colorLeft.g, colorLeft.b, 0);

        var colorRight = inCustardEffectRight.color;
        inCustardEffectRight.color = new Color(colorRight.r, colorRight.g, colorRight.b, 0);
    }

    private void FixedUpdate()
    {
        var transformPosition = transform.position;
        Coords cellPosition = worldCells.GetCellPosition(transformPosition.x, transformPosition.z);
        var custardAmount = custardState.GetCurrentCustardLevelAt(cellPosition);

        var insideCustard = custardAmount > 0;
        var coveredByCustard = custardAmount >= HeightTillCovered;

        if (CoveredByCustard != coveredByCustard)
        {
            OnCoveredByCustard(coveredByCustard);
            if (!coveredByCustard)
            {
                CheckForDrownDamage(false);
            }
        }

        if (InsideCustard != insideCustard)
        {
            timePassedSinceInsideCustardChange = graceTimeForBeingInsideOrOutsideCustard;
        }

        if (timePassedSinceInsideCustardChange > 0)
        {
            timePassedSinceInsideCustardChange -= Time.deltaTime;
        }

        InsideCustard = insideCustard;

        if (timePassedSinceInsideCustardChange <= 0)
        {
            MusicManager.Instance.SetUnder(insideCustard);
            timePassedSinceInsideCustardChange = 0;
        }
        CoveredByCustard = coveredByCustard;

        if (CoveredByCustard)
        {
            CheckForDrownDamage(CoveredByCustard);
        }
    }

    private void OnCoveredByCustard(bool coveredByCustard)
    {
        timePassedSinceCoveredByCustard = 0.0f;
        timePassedSinceLastDamage = 0.0f;
        player.EnterSwimMode(coveredByCustard);
    }

    private void CheckForDrownDamage(bool coveredByCustard)
    {
        if (coveredByCustard)
        {
            if (timePassedSinceCoveredByCustard < timeBeforePlayerTakesDamageFromDrawning)
            {
                timePassedSinceCoveredByCustard += Time.deltaTime;

                var timeTillDamage = timeBeforePlayerTakesDamageFromDrawning - timePassedSinceCoveredByCustard;
                if (timeTillDamage <= .01f)
                {
                    FadeCustardEffect(100, _fadeInDuration);
                }
            }
            else
            {
                if (timePassedSinceLastDamage < timeBetweenDamageTicks)
                {
                    timePassedSinceLastDamage += Time.deltaTime;
                }
                else if (player)
                {
                    player.TakeDamage(drowningDamage, DamageImplication.RadianceThenHealth);
                    timePassedSinceLastDamage = 0.0f;
                }
            }
        }
        else
        {
            timePassedSinceCoveredByCustard = 0;
            FadeCustardEffect(0, _fadeOutDuration);
        }
    }

    private void FadeCustardEffect(int alpha, float duration)
    {
        CrossFadeAlpha(inCustardEffect, inCustardEffectRight, alpha, duration);
    }

    public void CrossFadeAlpha(Image img, Image img2, float alpha, float duration)
    {
        if (inCustardEffectFader != null)
        {
            StopCoroutine(inCustardEffectFader);
        }

        inCustardEffectFader = StartCoroutine(CrossFadeAlphaCoroutine(img, img2, alpha, duration));
    }

    public static IEnumerator CrossFadeAlphaCoroutine(Image img, Image img2, float alpha, float duration)
    {
        Color currentColor = img.color;

        float counter = 0;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            img.color = new Color(currentColor.r, currentColor.g, currentColor.b,
                Mathf.MoveTowards(currentColor.a, alpha, counter / duration));
            img2.color = img.color;
            yield return null;
        }
    }
}