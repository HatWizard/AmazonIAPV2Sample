using System;
using System.Collections;
using System.Collections.Generic;
using AmazonIAP;
using UnityEngine;

namespace AmazonIAP
{
    public class BaseProductStateSaveService : IProductStateSaveService
    {
        [System.Serializable]
        private class ProductsData
        {
            public ProductState[] States;

            public ProductsData(ProductState[] States)
            {
                this.States = States;
            }
        }

        private const string PRODUCT_STATES_PREF = "ProductStates";
        
        public void SaveProductStates(ProductState[] state)
        {
            PlayerPrefs.SetString(PRODUCT_STATES_PREF, JsonUtility.ToJson(new ProductsData(state)));
        }

        public bool HasProductStates()
        {
            return PlayerPrefs.HasKey(PRODUCT_STATES_PREF);
        }

        public ProductState[] GetProductStates()
        {
            var serialized = PlayerPrefs.GetString(PRODUCT_STATES_PREF);
            if (serialized != String.Empty)
            {
                var productsData = JsonUtility.FromJson<ProductsData>(serialized);
                return productsData.States;
            }

            return null;
        }
    }
}
