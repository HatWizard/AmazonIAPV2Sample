using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonIAP
{
    public interface IProductReceipt
    {
        public string ProductSKU { get; }
        public string ReceiptId { get; }
        public ProductType ProductType { get; }
        public DateTime PurchaseDate { get; }
        public DateTime CancelDate { get; }
        public bool IsCanceled { get; }
        public bool IsSubscriptionActive { get; }
        
    }
}
