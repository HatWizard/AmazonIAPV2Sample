using System.Collections;
using System.Collections.Generic;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public interface IProductInfoUpdateService
    {
        void UpdateProductsInfo(GetProductDataResponse productDataResponse, ProductState[] productStates);
    }
}
