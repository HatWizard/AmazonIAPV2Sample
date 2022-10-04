using System;
using System.Collections;
using System.Collections.Generic;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public interface IAmazonStoreController
    {
        event Action OnInitialized;
        event Action OnProductsInfoUpdated;
        event Action<IProductReceipt, ProductState> OnPurchaseStateUpdated;
        event Action<IProductReceipt> OnUnknownReceiptReceived; 
        public bool IsInitialized { get; }
        public bool IsServiceAvailable { get; }
        public void Purchase(string sku);
        public void AddNewProduct(ProductState products);

        public void Init();
        public bool HasProducts();
        public ProductState GetProduct(string sku);
    }
}
