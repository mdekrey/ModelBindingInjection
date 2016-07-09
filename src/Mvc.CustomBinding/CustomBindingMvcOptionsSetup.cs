﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mvc.CustomBinding
{
    internal class CustomBindingMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            options.ValueProviderFactories.Add(new DeepDataValueProviderFactory());
            //options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
        }
    }
}