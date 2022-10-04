using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonIAP
{
    public interface IReceiptValidationService
    {
        event Action<IProductReceipt> OnUnknownReceiptReceived;
        event Action<IProductReceipt, ProductState> OnStateUpdate; 
        event Action<IProductReceipt, ProductState> OnFulfill;
        void ValidatePurchaseReceipt(IProductReceipt receipt, ProductState[] productStates);
        void ValidateUpdateReceipts(IProductReceipt[] receipts, ProductState[] productStates);
    }
}
