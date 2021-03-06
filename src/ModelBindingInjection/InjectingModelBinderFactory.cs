﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ModelBindingInjection
{
    delegate IModelBindingInjector RebinderFactory(ModelMetadata metadata);

    /// <summary>
    /// Default implementation for adding postprocessing to the model binding phase
    /// </summary>
    public class InjectingModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory original;
        private readonly IModelBindingInjectorFactory postprocessBinderFactory;
        private readonly IModelRebinderFactory modelRebinderFactory;

        /// <summary>
        /// Constructs the factory
        /// </summary>
        /// <param name="postprocessBinderFactory">The postprocess binder factory</param>
        /// <param name="modelRebinderFactory">The rebinder factory to handle the postprocess binder</param>
        /// <param name="original">The original model binder factory for initial binding</param>
        public InjectingModelBinderFactory(IModelBindingInjectorFactory postprocessBinderFactory, IModelRebinderFactory modelRebinderFactory, IModelBinderFactory original)
        {
            this.postprocessBinderFactory = postprocessBinderFactory;
            this.modelRebinderFactory = modelRebinderFactory;
            this.original = original;
        }

        /// <inheritdoc />
        public IModelBinder CreateBinder(ModelBinderFactoryContext context)
        {
            var result = original.CreateBinder(context);
            
            var rebinder = BuildRebinder(context.Metadata);
            if (rebinder != null)
            {
                var binder = new ModelBindingInjector(rebinder);
                return modelRebinderFactory.CreateRebinder(result, binder);
            }
            else
            {
                return result;
            }
        }

        private IModelBindingInjector BuildRebinder(ModelMetadata metadata)
        {
            if (metadata.BindingSource == ModelBindingInjectorAttribute.Source)
            {
                return postprocessBinderFactory.GetModelBinder(metadata);
            }
            else if (metadata.IsCollectionType)
            {
                var result = new CollectionTypeModelRebinder(metadata, BuildRebinder);
                return result.HasBinding ? result : null;
            }
            else if (metadata.IsComplexType)
            {
                var attributes = (metadata as DefaultModelMetadata)?.Attributes?.Attributes.OfType<Attribute>() 
                    ?? metadata.ModelType.GetTypeInfo().GetCustomAttributes();
                if (attributes.OfType<RecurseModelBindingInjectionAttribute>().Any())
                {
                    var result = new ComplexTypeModelRebinder(metadata, BuildRebinder);
                    return result.HasBindings ? result : null;
                }
            }
            return null;
        }

        class ComplexTypeModelRebinder : IModelBindingInjector
        {
            private readonly Dictionary<ModelMetadata, IModelBindingInjector> propertyBinders;

            public ComplexTypeModelRebinder(ModelMetadata metadata, RebinderFactory createBinder)
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBindingInjector>();
                foreach (var property in metadata.Properties)
                {
                    propertyBinders.Add(property, createBinder(property));
                }
                this.propertyBinders = propertyBinders;
            }

            public bool HasBindings => propertyBinders.Values.Any(v => v != null);

            public async Task BindModelAsync(ModelBindingInjectorContext bindingContext)
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


        class CollectionTypeModelRebinder : IModelBindingInjector
        {
            private readonly IModelBindingInjector elementBinder;
            private readonly ModelMetadata elementMetadata;

            public CollectionTypeModelRebinder(ModelMetadata metadata, RebinderFactory createBinder)
            {
                elementMetadata = metadata.ElementMetadata;
                elementBinder = createBinder(elementMetadata);
            }

            public bool HasBinding => elementBinder != null;

            public async Task BindModelAsync(ModelBindingInjectorContext bindingContext)
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
