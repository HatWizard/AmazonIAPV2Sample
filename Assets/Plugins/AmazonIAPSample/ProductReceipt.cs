using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AmazonIAP
{
    [System.Serializable]
    public class ProductReceipt : IProductReceipt
    {
        public string ProductSKU { get; private set; }
        public string ReceiptId { get; private set; }
        public ProductType ProductType { get; private set; }
        public DateTime PurchaseDate { get; private set; }
        public DateTime CancelDate { get; private set; }
        public bool IsCanceled { get; private set; }
        public bool IsSubscriptionActive { get; private set; }

        public ProductReceipt(string productSKU, string receiptId, ProductType productType, DateTime purchaseDate, 
            DateTime cancelDate, bool isSubscriptionActive, bool isCanceled)
        {
            ProductSKU = productSKU;
            ReceiptId = receiptId;
            PurchaseDate = purchaseDate;
            CancelDate = cancelDate;
            IsSubscriptionActive = isSubscriptionActive;
            IsCanceled = isCanceled;
            ProductType = productType;
        }
    }
}




