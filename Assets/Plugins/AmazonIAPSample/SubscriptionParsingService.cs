using System;
using System.Collections;
using System.Collections.Generic;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public class SubscriptionParsingService : IResponseParsingService
    {
        public const string SUBSCRIPTION_PRODUCT_TYPE = "SUBSCRIPTION";
        public const string ENTITLED_PRODUCT_TYPE = "ENTITLED";
        public const string CONSUMABLE_PRODUCT_TYPE = "CONSUMABLE";
        
        
        private static readonly DateTime _unixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        

        public IProductReceipt ParseReceipt(PurchaseReceipt receipt)
        {
            LogMessage("Parse receipt for SKU -> " + receipt.Sku);
            var purchaseDate = ConvertToDateTime(receipt.PurchaseDate);
            var cancelDate = ConvertToDateTime(receipt.CancelDate);
            ProductType productType = ConvertProductType(receipt.ProductType);
            bool isCanceled = receipt.CancelDate == 0;
            double totalPurchaseTime = (DateTime.Now - purchaseDate).TotalHours;
            
            LogMessage("Purchase date -> " + purchaseDate);
            LogMessage("Cancel date -> " + cancelDate);
            LogMessage("Total purchase time -> " + totalPurchaseTime);
            bool isSubscriptionActive = isCanceled && 
                                        productType == ProductType.Subscription;
            
            return new ProductReceipt(receipt.Sku, receipt.ReceiptId, productType, purchaseDate, cancelDate,
                isSubscriptionActive, isCanceled);
        }

        private DateTime ConvertToDateTime(long milliseconds)
        {
            return _unixEpoch.AddMilliseconds(milliseconds);
        }

        private ProductType ConvertProductType(string productType)
        {
            switch (productType.ToUpper())
            {
                case ENTITLED_PRODUCT_TYPE:
                    return ProductType.Entitled;
                case CONSUMABLE_PRODUCT_TYPE:
                    return ProductType.Consumable;
                case SUBSCRIPTION_PRODUCT_TYPE:
                    return ProductType.Subscription;
            }
            return ProductType.Consumable;
        }

        private void LogMessage(string message)
        {
            IAPLogger.Log(message);
        }
    }
}


