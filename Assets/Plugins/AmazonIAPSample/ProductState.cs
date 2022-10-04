using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonIAP
{
    [System.Serializable]
    public class ProductState
    {
        //todo we can't use properties with json utils
        public string SKU;
        public string Title;
        public string Description;
        public string Price;
        public ProductType ProductType;
        public bool IsAvailable = true;
        public bool IsSubscriptionActive;
        public bool IsEntitledPurchased;
        public DateTime CancelDate;

        public ProductState(string sku, ProductType productType)
        {
            SKU = sku;
            ProductType = productType;
            IsSubscriptionActive = false;
            IsEntitledPurchased = false;
            CancelDate = DateTime.UnixEpoch;
        }

        public ProductState(string sku, string title, string description, string price, ProductType productType, 
            bool isAvailable, bool isSubscriptionActive, bool isEntitledPurchased, DateTime cancelDate)
        {
            SKU = sku;
            Title = title;
            Description = description;
            Price = price;
            ProductType = productType;
            IsAvailable = isAvailable;
            IsSubscriptionActive = isSubscriptionActive;
            IsEntitledPurchased = isEntitledPurchased;
            CancelDate = cancelDate;
        }
    }
}
