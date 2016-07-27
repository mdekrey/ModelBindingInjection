﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.DotNet.InternalAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class PostprocessingBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory original;

        public PostprocessingBinderFactory(IModelBinderFactory original)
        {
            this.original = original;
        }

        public IModelBinder CreateBinder(ModelBinderFactoryContext context)
        {
            var result = original.CreateBinder(context);
            
            var rebinder = BuildRebinder(context.Metadata);
            if (rebinder != null)
            {
                return new ModelRebinder(result, rebinder);
            }
            else
            {
                return result;
            }
        }

        private IModelBinder BuildRebinder(ModelMetadata metadata)
        {
            if (metadata.IsCollectionType)
            {
                return new CollectionTypeModelRebinder(metadata, BuildRebinder);
            }
            else if (metadata.IsComplexType && metadata.ModelType.GetTypeInfo().GetCustomAttribute<RecursePostprocessBindingAttribute>() != null)
            {
                return new ComplexTypeModelRebinder(metadata, BuildRebinder);
            }
            else if (metadata.BindingSource == PostprocessBindingAttribute.Source)
            {
                return new CustomModelBinder();
            }
            return null;
        }

        class ComplexTypeModelRebinder : IModelBinder
        {
            private readonly Dictionary<ModelMetadata, IModelBinder> propertyBinders;

            public ComplexTypeModelRebinder(ModelMetadata metadata, Func<ModelMetadata, IModelBinder> createBinder)
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
                foreach (var property in metadata.Properties)
                {
                    propertyBinders.Add(property, createBinder(property));
                }
                this.propertyBinders = propertyBinders;
            }

            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext.Model == null)
                {
                    ModelBindingResult.Failed();
                }
                foreach (var property in bindingContext.ModelMetadata.Properties)
                {
                    // Pass complex (including collection) values down so that binding system does not unnecessarily
                    // recreate instances or overwrite inner properties that are not bound. No need for this with simple
                    // values because they will be overwritten if binding succeeds. Arrays are never reused because they
                    // cannot be resized.
                    object propertyModel = null;
                    if (property.PropertyGetter != null &&
                        property.IsComplexType)
                    {
                        propertyModel = property.PropertyGetter(bindingContext.Model);
                    }

                    var fieldName = property.BinderModelName ?? property.PropertyName;
                    var modelName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, fieldName);

                    ModelBindingResult result = ModelBindingResult.Failed();
                    using (bindingContext.EnterNestedScope(
                        modelMetadata: property,
                        fieldName: fieldName,
                        modelName: modelName,
                        model: propertyModel))
                    {
                        if (propertyBinders[property] != null)
                        {
                            await propertyBinders[property].BindModelAsync(bindingContext);
                            result = bindingContext.Result;
                        }
                    }

                    if (result.IsModelSet)
                    {
                        try
                        {
                            property.PropertySetter(bindingContext.Model, result.Model);
                        }
                        catch (Exception) // exception)
                        {
                            // TODO - log model errors
                            //AddModelError(exception, modelName, bindingContext, result);
                        }
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            }
        }


        class CollectionTypeModelRebinder : IModelBinder
        {
            private readonly IModelBinder elementBinder;
            private readonly ModelMetadata elementMetadata;

            public CollectionTypeModelRebinder(ModelMetadata metadata, Func<ModelMetadata, IModelBinder> createBinder)
            {
                elementMetadata = metadata.ElementMetadata;
                elementBinder = createBinder(elementMetadata);
            }

            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext.Model != null && elementBinder != null)
                {
                    foreach (var entry in (System.Collections.IEnumerable)bindingContext.Model)
                    {
                        using (bindingContext.EnterNestedScope(
                            elementMetadata,
                            fieldName: bindingContext.FieldName,
                            modelName: bindingContext.ModelName,
                            model: entry))
                        {
                            await elementBinder.BindModelAsync(bindingContext);
                        }
                    }
                }
            }
        }
    }
}
