using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public class AmazonStoreController : IAmazonStoreController
    {
        /// <summary>
            /// Receipt can be null
        /// </summary>
        public event Action<IProductReceipt, ProductState> OnPurchaseStateUpdated;
        public event Action OnProductsInfoUpdated;
        public event Action<IProductReceipt> OnUnknownReceiptReceived;
        public event Action OnInitialized;
        public bool IsInitialized { get; private set; }

        public bool IsServiceAvailable
        {
            get => HasInternet();
        }

        private bool _productInfosUpdated;
        private bool _productReceiptsUpdated;
        private IProductStateSaveService _productStateSaveService;
        private IResponseParsingService _responseParsingService;
        private IReceiptValidationService _receiptValidationService;
        private IProductInfoUpdateService _productInfoUpdateService;
        private IAmazonIapV2 _amazonService;
        private List<ProductState> _productStates;
        private ResetInput _resetInput;
        private int _currentProductsUpdatesPagination;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveService">Products state data saving service</param>
        /// <param name="responseParsingService">Response parsing service</param>
        /// <param name="receiptValidationService">Receipt validation parsing service</param>
        /// <param name="resetInput">IMPORTANT: Request all products receipts (including already received previously)</param>
        public AmazonStoreController(IProductStateSaveService saveService, IResponseParsingService responseParsingService, 
            IReceiptValidationService receiptValidationService, IProductInfoUpdateService productInfoUpdateService, bool resetInput = true)
        {
            _amazonService = AmazonIapV2Impl.Instance;
            _productStateSaveService = saveService;
            _responseParsingService = responseParsingService;
            _receiptValidationService = receiptValidationService;
            _productInfoUpdateService = productInfoUpdateService;
            _resetInput = new ResetInput();
            _resetInput.Reset = resetInput;
            _productStates = new List<ProductState>();

            receiptValidationService.OnFulfill += NotifyReceiptFulfillment;
            receiptValidationService.OnStateUpdate += (receipt, state) =>
            {
                OnPurchaseStateUpdated?.Invoke(receipt, state);
            };
            receiptValidationService.OnUnknownReceiptReceived += (receipt) =>
            {
                OnUnknownReceiptReceived?.Invoke(receipt);
            };
            ReceiveProductStates();
        }

        public void Init()
        {
            SaveProductStates();
            AddListeners();
            StartInitialization();
        }

        public void AddNewProduct(ProductState productState)
        {
            if (_productStates.All(s => s.SKU != productState.SKU))
            {
                _productStates.Add(productState);
            }
            else
            {
                LogMessage("[AmazonIAP] Product already contains in store -> " + productState.SKU);
            }
        }

        public void Purchase(string sku)
        {
            if (!IsInitialized)
            {
                LogMessage("Store wasn't initialized!");
            }

            var state = _productStates.FirstOrDefault(s => s.SKU == sku);
            if (state == null)
            {
                LogMessage("Product for purchase not found: " + sku);
                return;
            }

            var skuInput = new SkuInput();
            skuInput.Sku = sku;
            _amazonService.Purchase(skuInput);

#if UNITY_EDITOR
            SimulatePurchaseResponse(sku);
#endif
        }

        public bool HasProducts()
        {
            return _productStateSaveService.HasProductStates();
        }

        public ProductState GetProduct(string sku)
        {
            var product = _productStates.FirstOrDefault(p => p.SKU == sku);
            return product;
        }

        private void StartInitialization()
        {
            if (HasInternet())
            {
                SendUpdateProductsRequest();
                SendProductDataRequest();
            }
            else
            {
                NotifyInitialized();
            }
        }

        private void SendUpdateProductsRequest()
        {
            _amazonService.GetPurchaseUpdates(_resetInput);
            
#if UNITY_EDITOR
            SimulateProductsUpdates();
#endif
        }

        private void SendProductDataRequest()
        {
            var skusInput = new SkusInput();
            skusInput.Skus = _productStates.Select(s => s.SKU).ToList();
            _amazonService.GetProductData(skusInput);
#if UNITY_EDITOR
            SimulateProductsInfoUpdate();
#endif
        }

        private void NotifyInitializationStep()
        {
            if (_productInfosUpdated && _productReceiptsUpdated)
            {
                NotifyInitialized();
            }
        }

        private void NotifyInitialized()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                OnInitialized?.Invoke();
                LogMessage("Store initialized!");
                SaveProductStates();
            }
        }

        private void SaveProductStates()
        {
            if (_productStates.Count > 0)
            {
                _productStateSaveService.SaveProductStates(_productStates.ToArray());
            }
            else
            {
                LogMessage("No products to save!");
            }
        }

        private void ReceiveProductStates()
        {
            if (HasProducts())
            {
                foreach (var savedProduct in _productStateSaveService.GetProductStates())
                {
                    _productStates.Add(savedProduct);
                }
            }
        }

        private void AddListeners()
        {
            _amazonService.AddGetPurchaseUpdatesResponseListener(ProductsUpdateResponseHandler);
            _amazonService.AddPurchaseResponseListener(PurchaseResponseHandler);
            _amazonService.AddGetProductDataResponseListener(GetProductsDataHandler);
        }

        private void ProductsUpdateResponseHandler(GetPurchaseUpdatesResponse response)
        {
            int receiptsCount = response.Receipts != null ? response.Receipts.Count : 0;
            LogMessage($"Update products response received with status: {response.Status} and receipts: {receiptsCount}");

            if (response.Status == IAPDefines.SUCCESS_RESPONSE_STATUS)
            {
                var parsedReceipts = ParseUpdatedReceipts(response.Receipts);
                _receiptValidationService.ValidateUpdateReceipts(parsedReceipts, _productStates.ToArray());
            }
            
            if (response.HasMore && response.Status != IAPDefines.FAILED_RESPONSE_STATUS)
            {
                _currentProductsUpdatesPagination++;
                LogMessage($"Get next products update recursively. Pagination: " + _currentProductsUpdatesPagination);
                _amazonService.GetPurchaseUpdates(_resetInput);
            }
            else
            {
                _productReceiptsUpdated = true;
                NotifyInitializationStep();
            }
        }

        private IProductReceipt[] ParseUpdatedReceipts(List<PurchaseReceipt> receipts)
        {
            List<IProductReceipt> productReceipts = new List<IProductReceipt>();

            if (receipts != null)
            {
                foreach (var receipt in receipts)
                {
                    productReceipts.Add(_responseParsingService.ParseReceipt(receipt));
                }
            }

            return productReceipts.ToArray();
        }

        private void PurchaseResponseHandler(PurchaseResponse purchaseResponse)
        {
            LogMessage("Purchase response received with status: " + purchaseResponse.Status);

            if (purchaseResponse.Status == IAPDefines.SUCCESS_RESPONSE_STATUS && purchaseResponse.PurchaseReceipt != null)
            {
                var parsedReceipt = _responseParsingService.ParseReceipt(purchaseResponse.PurchaseReceipt);
                _receiptValidationService.ValidatePurchaseReceipt(parsedReceipt, _productStates.ToArray());
            }
            SaveProductStates();
        }

        private void GetProductsDataHandler(GetProductDataResponse productDataResponse)
        {
            LogMessage("Products data response received with status: " + productDataResponse.Status);
            if (productDataResponse.Status == IAPDefines.SUCCESS_RESPONSE_STATUS)
            {
                _productInfoUpdateService.UpdateProductsInfo(productDataResponse, _productStates.ToArray());
                OnProductsInfoUpdated?.Invoke();
            }
            _productInfosUpdated = true;
            NotifyInitializationStep();
        }

        private void NotifyReceiptFulfillment(IProductReceipt productReceipt, ProductState productState)
        {
            var notifyFulfillmentInput = new NotifyFulfillmentInput();
            notifyFulfillmentInput.FulfillmentResult = IAPDefines.PRODUCT_FULFILLED;
            notifyFulfillmentInput.ReceiptId = productReceipt.ReceiptId;
            _amazonService.NotifyFulfillment(notifyFulfillmentInput);
            LogMessage("Notified fulfillment: " + productReceipt.ProductSKU);
        }

        private bool HasInternet()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void LogMessage(string message)
        {
            IAPLogger.Log(message);
        }

        //todo separated simulation of store controller for Unity Editor will be better
        #region SIMULATION

        private void SimulatePurchaseResponse(string sku)
        {
            var state = GetProduct(sku);
            var purchaseResponse = new PurchaseResponse();
            purchaseResponse.Status = IAPDefines.SUCCESS_RESPONSE_STATUS;
            purchaseResponse.RequestId = "TEST";
            var receipt = new PurchaseReceipt();
            receipt.Sku = sku;
            if (state.ProductType == ProductType.Subscription)
            {
                receipt.CancelDate = 0;
                receipt.PurchaseDate = (long)((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
                receipt.ProductType = IAPDefines.SUBSCRIPTION_PRODUCT_TYPE;
            }
            else if(state.ProductType == ProductType.Consumable)
            {
                receipt.ProductType = IAPDefines.CONSUMABLE_PRODUCT_TYPE;
            }
            else
            {
                receipt.ProductType = IAPDefines.ENTITLED_PRODUCT_TYPE;
            }

            purchaseResponse.PurchaseReceipt = receipt;
            
            PurchaseResponseHandler(purchaseResponse);
        }

        private void SimulateProductsUpdates()
        {
            var productsUpdateResponse = new GetPurchaseUpdatesResponse();
            productsUpdateResponse.Status = IAPDefines.SUCCESS_RESPONSE_STATUS;
            productsUpdateResponse.HasMore = false;
            productsUpdateResponse.Receipts = null;
            ProductsUpdateResponseHandler(productsUpdateResponse);
        }

        private void SimulateProductsInfoUpdate()
        {
            GetProductsDataHandler(new GetProductDataResponse());
        }

        #endregion
        
    }
}
