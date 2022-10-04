using System;
using System.Collections;
using System.Collections.Generic;
using AmazonIAP;
using UnityEngine;
using UnityEngine.UI;

public class SubscriptionPurchaseViewController : MonoBehaviour
{
    [field: SerializeField] private SampleIAPManager IAPManager { get; set; }
    [field: SerializeField] private TMPro.TextMeshProUGUI LogTxt { get; set; }
    [field: SerializeField] private TMPro.TextMeshProUGUI PurchaseBtnTxt { get; set; }
    [field: SerializeField] private Button PurchaseBtn { get; set; }
    [field: SerializeField] private Button ClearLogBtn { get; set; }
    [field: SerializeField] private GameObject SubscriptionActiveMark { get; set; }

    private string _logMessages = string.Empty;

    private void Awake() 
    {
        IAPLogger.OnMessageReceived += AddToLog;
        IAPManager.OnSubscriptionStateUpdated += UpdateSubscriptionView;

        PurchaseBtn.onClick.AddListener(PurchaseSubscription);
        ClearLogBtn.onClick.AddListener(ClearLog);
    }

    private void UpdateSubscriptionView(bool hasSubscription)
    {
        PurchaseBtn.gameObject.SetActive(!hasSubscription);
        SubscriptionActiveMark.SetActive(hasSubscription);

        PurchaseBtnTxt.text = "Purchase for " + IAPManager.GetProductPrice();
    }

    private void AddToLog(string message)
    {
        message += Environment.NewLine;
        _logMessages += message;
        UpdateLog();
    }

    private void UpdateLog()
    {
        LogTxt.text = _logMessages;
    }

    private void ClearLog()
    {
        _logMessages = string.Empty;
        UpdateLog();
    }

    private void PurchaseSubscription()
    {
        IAPManager.PurhcaseSubscription();
    }
}
