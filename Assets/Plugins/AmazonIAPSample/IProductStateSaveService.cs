using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonIAP
{
    public interface IProductStateSaveService
    {
        void SaveProductStates(ProductState[] state);
        bool HasProductStates();
        ProductState[] GetProductStates();
    }
}
