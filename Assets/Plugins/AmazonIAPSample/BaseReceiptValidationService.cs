using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmazonIAP;
using UnityEngine;

namespace AmazonIAP
{
    public class BaseReceiptValidationService : IReceiptValidationService
    {
        public BaseReceiptValidationService(bool clearSubscriptionWithoutReceipts)
        {
            _clearSubscriptionWithoutReceipts = clearSubscriptionWithoutReceipts;
        }

        public event Action<IProductReceipt> OnUnknownReceiptReceived;
        public event Action<IProductReceipt, ProductState> OnStateUpdate;
        public event Action<IProductReceipt, ProductState> OnFulfill;

        private bool _clearSubscriptionWithoutReceipts;
        
        public void ValidatePurchaseReceipt(IProductReceipt receipt, ProductState[] productStates)
        {
            var state = FindValidProductState(receipt, productStates);
            if (state != null)
            {
                ProcessReceipt(receipt, state);
            }
            else
            {
                LogMessage($"Receipt with unknown SKU was found - {receipt.ProductSKU} of type - {receipt.ProductType}");
                OnUnknownReceiptReceived?.Invoke(receipt);
            }
        }

        public void ValidateUpdateReceipts(IProductReceipt[] receipts, ProductState[] productStates)
        {
            foreach (var receipt in receipts)
            {
                var state = FindValidProductState(receipt, productStates);
                if (state != null)
                {
                    ProcessReceipt(receipt, state);
                }
                else
                {
                    LogMessage($"Receipt with unknown SKU was found - {receipt.ProductSKU} of type - {receipt.ProductType}");
                    OnUnknownReceiptReceived?.Invoke(receipt);
                }
            }

            if (receipts.Length == 0 && _clearSubscriptionWithoutReceipts)
            {
                ValidateNotUpdatedProduct(productStates);
            }
        }

        private void ValidateNotUpdatedProduct(ProductState[] productStates)
        {
            foreach (var state in productStates)
            {
                if (state.ProductType == ProductType.Subscription && state.IsSubscriptionActive)
                {
                    state.IsSubscriptionActive = false;
                    OnStateUpdate?.Invoke(null, state);
                }
            }
        }

        // private bool HasValidState(IProductReceipt productReceipt, ProductState[] productStates)
        // {
        //     var productState = productStates
        //         .FirstOrDefault(r => r.SKU.Contains(productReceipt.ProductSKU));
        //     if(productState != null)
        //     {
        //         if (productState.ProductType == productReceipt.ProductType)
        //             return true;
        //     }
        //
        //     return false;
        // }

        private ProductState FindValidProductState(IProductReceipt receipt, ProductState[] productStates)
        {
            var productState = productStates
                .FirstOrDefault(r => r.SKU.Contains(receipt.ProductSKU));
            if(productState != null)
            {
                if (productState.ProductType == receipt.ProductType)
                    return productState;
            }
            return null;
        }

        private void ProcessReceipt(IProductReceipt productReceipt, ProductState state)
        {
            switch (state.ProductType)
            {
                case ProductType.Consumable:
                    ProcessConsumableReceipt(productReceipt, state);
                    break;
                case ProductType.Subscription:
                    ProcessSubscriptionReceipt(productReceipt, state);
                    break;
                case ProductType.Entitled:
                    ProcessEntitledReceipt(productReceipt, state);
                    break;
            }
        }
        
        
        private void ProcessSubscriptionReceipt(IProductReceipt receipt, ProductState productState)
        {
            bool previouslyActive = productState.IsSubscriptionActive;

            LogMessage($"Subscription receipt - {receipt.ProductSKU} - active: {receipt.IsSubscriptionActive}");
            LogMessage($"Current product state - {productState.SKU} - active: {productState.IsSubscriptionActive}");

            productState.CancelDate = receipt.CancelDate;
            productState.IsSubscriptionActive = receipt.IsSubscriptionActive;

            if (productState.IsSubscriptionActive != previouslyActive)
            {
                LogMessage($"Subscription product state updated - {productState.SKU}");
                OnStateUpdate?.Invoke(receipt, productState);
                if(!previouslyActive) OnFulfill?.Invoke(receipt, productState);
            }
        }

        private void ProcessEntitledReceipt(IProductReceipt receipt, ProductState productState)
        {
            if(productState.IsEntitledPurchased) return;

            productState.IsEntitledPurchased = true;
            
            OnStateUpdate?.Invoke(receipt, productState);
            OnFulfill?.Invoke(receipt, productState);
        }

        private void ProcessConsumableReceipt(IProductReceipt receipt, ProductState productState)
        {
            OnStateUpdate?.Invoke(receipt, productState);
            OnFulfill?.Invoke(receipt, productState);
        }

        private void LogMessage(string message)
        {
            IAPLogger.Log(message);
        }

    }
}
