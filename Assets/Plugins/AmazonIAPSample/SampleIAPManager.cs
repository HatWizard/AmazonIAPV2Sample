using System.Collections;
using System.Collections.Generic;
using AmazonIAP;
using UnityEngine;

namespace AmazonIAP
{
    public class SampleIAPManager : MonoBehaviour
    {
        public event System.Action<bool> OnSubscriptionStateUpdated;

        private IAmazonStoreController _storeController;
        private string _subscriptionSKU = "com.little.tanks.combat.war.battle_subscribe";

        private void Start() 
        {
            CreateStore();
            AddProducts();
            InitializeStore();
        }

        public bool IsSubscriptionAvailable()
        {
            return _storeController.GetProduct(_subscriptionSKU).IsAvailable && _storeController.IsServiceAvailable;
        }

        public bool IsSubscriptionActive()
        {
            return _storeController.GetProduct(_subscriptionSKU).IsSubscriptionActive;
        }

        public void PurhcaseSubscription()
        {
            if(IsSubscriptionActive() || !_storeController.IsInitialized) return;

            _storeController.Purchase(_subscriptionSKU);
        }

        public string GetProductPrice()
        {
            return _storeController.GetProduct(_subscriptionSKU).Price;
        }

        private void CreateStore()
        {
            //Service for parsing received plugin receipts
            IResponseParsingService purchaseParser = new SubscriptionParsingService();
            
            //Service for saving product states between sessions
            IProductStateSaveService saveService = new BaseProductStateSaveService();

            //Service for updating product secondary informate such as price, description and so on
            IProductInfoUpdateService productInfoUpdater = new BaseProductInfoUpdateService();

            //Service for final receipt validation
            IReceiptValidationService receiptValidator = new BaseReceiptValidationService(true);

            _storeController = new AmazonStoreController(saveService, purchaseParser, receiptValidator, productInfoUpdater, true);
        }

        private void AddProducts()
        {
            //Add all game products states to the store if needed
            //All products will be saved during inialization
            if(!_storeController.HasProducts())
            {
                _storeController.AddNewProduct(new ProductState(_subscriptionSKU, ProductType.Subscription));
            }
        }

        private void InitializeStore()
        {
            _storeController.OnUnknownReceiptReceived += HandleUnknownReceipt;
            _storeController.OnInitialized += UpdateProductsOnInitialized;
            
            _storeController.Init();
        }

        private void UpdateProductsOnInitialized()
        {
            _storeController.OnPurchaseStateUpdated += UpdateProduct;
            //update subscription and entitled products state

            OnSubscriptionStateUpdated?.Invoke(IsSubscriptionActive());

        }

        private void UpdateProduct(IProductReceipt receipt, ProductState productState)
        {
            //Present consumable products or update subscriptions and entitled products state

            OnSubscriptionStateUpdated?.Invoke(IsSubscriptionActive());
        }

        private void HandleUnknownReceipt(IProductReceipt productReceipt)
        {
            //Receipt and initialial SKU's of subscription can be different in some cases (not confirmed)
        }
    }

}


