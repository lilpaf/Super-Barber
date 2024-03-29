﻿using SuperBarber.Core.Models.Service;

namespace SuperBarber.Core.Extensions
{
    public class ModelStateCustomException : Exception
    {
        public string Key { get; }
        
        public List<ServiceListingViewModel> CartList { get; }

        public ModelStateCustomException(string key, string messege) : base(messege)
        {
            Key = key;
        }
        
        public ModelStateCustomException(string key, string messege, List<ServiceListingViewModel> cartList) : base(messege)
        {
            Key = key;
            CartList = cartList;
        }
    }
}
