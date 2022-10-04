using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public class BaseProductInfoUpdateService : IProductInfoUpdateService
    {
        public void UpdateProductsInfo(GetProductDataResponse productDataResponse, ProductState[] productStates)
        {
            LogMessage("Updating products info by response...");
            foreach (var keySku in productDataResponse.ProductDataMap.Keys)
            {
                
                var state = GetProductBySKU(keySku, productStates);
                if(state == null) continue;

                var productData = productDataResponse.ProductDataMap[keySku];

                state.Title = productData.Title;
                state.Description = productData.Description;
                state.Price = productData.Price;
                state.IsAvailable = true;
                LogMessage(keySku + ": available product info updated -> Product SKU: " + state.SKU);
            }

            MarkUnavailable(productDataResponse.UnavailableSkus, productStates);
        }

        private ProductState GetProductBySKU(string sku, ProductState[] productStates)
        {
            return productStates.FirstOrDefault(p => p.SKU.Contains(sku));
        }

        private void MarkUnavailable(List<string> unavailableSKU, ProductState[] productStates)
        {
            if(unavailableSKU == null) return;

            foreach (var sku in unavailableSKU)
            {
                var state = GetProductBySKU(sku, productStates);
                if(state == null) continue;
                LogMessage(sku + ": is not available!");
                state.IsAvailable = false;
            }
        }

        private void LogMessage(string message)
        {
            IAPLogger.Log(message);
        }
    }
}
