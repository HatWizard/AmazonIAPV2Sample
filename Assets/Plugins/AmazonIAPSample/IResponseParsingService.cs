using System.Collections;
using System.Collections.Generic;
using com.amazon.device.iap.cpt;
using UnityEngine;

namespace AmazonIAP
{
    public interface IResponseParsingService
    {
        IProductReceipt ParseReceipt(PurchaseReceipt receipt);
    }    
}

