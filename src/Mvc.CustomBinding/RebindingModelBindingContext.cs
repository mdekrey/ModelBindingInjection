using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Mvc.CustomBinding
{
    public class RebindingModelBindingContext : DefaultModelBindingContext
    {
        private Stack<Tuple<ModelMetadata, object>> containers = new Stack<Tuple<ModelMetadata, object>>();
        
        public RebindingModelBindingContext(ModelMetadata originalModelMetadata, object originalModel)
        {
            OriginalModelMetadata = originalModelMetadata;
            OriginalModel = originalModel;
        }
        public ModelMetadata OriginalModelMetadata { get; }
        public object OriginalModel { get; }

        public ModelMetadata ContainerModelMetadata { get; set; }
        public object ContainerModel { get; set; }

        /// <inheritdoc />
        public override NestedScope EnterNestedScope()
        {
            containers.Push(Tuple.Create(ContainerModelMetadata, ContainerModel));
            ContainerModelMetadata = ModelMetadata;
            ContainerModel = Model;
            return base.EnterNestedScope();
        }

        /// <inheritdoc />
        protected override void ExitNestedScope()
        {
            var temp = containers.Pop();
            ContainerModelMetadata = temp.Item1;
            ContainerModel = temp.Item2;
            base.ExitNestedScope();
        }

        public static RebindingModelBindingContext CreateBindingContext(
            ActionContext actionContext,
            IValueProvider valueProvider,
            ModelMetadata metadata,
            BindingInfo bindingInfo,
            string modelName,
            object originalModel)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (modelName == null)
            {
                throw new ArgumentNullException(nameof(modelName));
            }

            var binderModelName = bindingInfo?.BinderModelName ?? metadata.BinderModelName;
            var propertyFilterProvider = bindingInfo?.PropertyFilterProvider ?? metadata.PropertyFilterProvider;

            var bindingSource = bindingInfo?.BindingSource ?? metadata.BindingSource;

            return new RebindingModelBindingContext(metadata, originalModel)
            {
                ActionContext = actionContext,
                BinderModelName = binderModelName,
                BindingSource = bindingSource,
                PropertyFilter = propertyFilterProvider?.PropertyFilter,

                // Because this is the top-level context, FieldName and ModelName should be the same.
                FieldName = binderModelName ?? modelName,
                ModelName = binderModelName ?? modelName,

                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelState = actionContext.ModelState,
                Model = originalModel,

                OriginalValueProvider = valueProvider,
                ValueProvider = FilterValueProvider(valueProvider, bindingSource),

                ValidationState = new ValidationStateDictionary(),
            };
        }

        private static IValueProvider FilterValueProvider(IValueProvider valueProvider, BindingSource bindingSource)
        {
            if (bindingSource == null || bindingSource.IsGreedy)
            {
                return valueProvider;
            }

            var bindingSourceValueProvider = valueProvider as IBindingSourceValueProvider;
            if (bindingSourceValueProvider == null)
            {
                return valueProvider;
            }

            return bindingSourceValueProvider.Filter(bindingSource) ?? new CompositeValueProvider();
        }
    }
}
