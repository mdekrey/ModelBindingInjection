using Microsoft.AspNetCore.Mvc.ModelBinding;
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

namespace Mvc.CustomBinding
{
    delegate IModelPostbinder RebinderFactory(ModelMetadata metadata);

    /// <summary>
    /// Default implementation for adding postprocessing to the model binding phase
    /// </summary>
    public class PostprocessingBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory original;
        private readonly IPostprocessBinderFactory postprocessBinderFactory;
        private readonly IModelRebinderFactory modelRebinderFactory;

        /// <summary>
        /// Constructs the factory
        /// </summary>
        /// <param name="postprocessBinderFactory">The postprocess binder factory</param>
        /// <param name="modelRebinderFactory">The rebinder factory to handle the postprocess binder</param>
        /// <param name="original">The original model binder factory for initial binding</param>
        public PostprocessingBinderFactory(IPostprocessBinderFactory postprocessBinderFactory, IModelRebinderFactory modelRebinderFactory, IModelBinderFactory original)
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
                return modelRebinderFactory.CreateRebinder(result, new ModelPostbinder(rebinder));
            }
            else
            {
                return result;
            }
        }

        private IModelPostbinder BuildRebinder(ModelMetadata metadata)
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
                return postprocessBinderFactory.GetModelBinder(metadata);
            }
            return null;
        }

        class ComplexTypeModelRebinder : IModelPostbinder
        {
            private readonly Dictionary<ModelMetadata, IModelPostbinder> propertyBinders;

            public ComplexTypeModelRebinder(ModelMetadata metadata, RebinderFactory createBinder)
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelPostbinder>();
                foreach (var property in metadata.Properties)
                {
                    propertyBinders.Add(property, createBinder(property));
                }
                this.propertyBinders = propertyBinders;
            }

            public async Task BindModelAsync(ModelPostbindingContext bindingContext)
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


        class CollectionTypeModelRebinder : IModelPostbinder
        {
            private readonly IModelPostbinder elementBinder;
            private readonly ModelMetadata elementMetadata;

            public CollectionTypeModelRebinder(ModelMetadata metadata, RebinderFactory createBinder)
            {
                elementMetadata = metadata.ElementMetadata;
                elementBinder = createBinder(elementMetadata);
            }

            public async Task BindModelAsync(ModelPostbindingContext bindingContext)
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
