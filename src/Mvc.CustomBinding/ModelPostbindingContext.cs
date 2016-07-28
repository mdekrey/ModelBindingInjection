using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Mvc.CustomBinding
{
    /// <summary>
    /// Context for post-binding a model
    /// </summary>
    public class ModelPostbindingContext : DefaultModelBindingContext
    {
        private Stack<Tuple<ModelMetadata, object>> containers = new Stack<Tuple<ModelMetadata, object>>();
        
        /// <summary>
        /// Sets up the post-binding context
        /// </summary>
        /// <param name="originalModelMetadata">The metadata for the original (root) model</param>
        /// <param name="originalModel">The root model. Kept in order to have relative binding, such as using an Id field.</param>
        public ModelPostbindingContext(ModelMetadata originalModelMetadata, object originalModel)
        {
            OriginalModelMetadata = originalModelMetadata;
            OriginalModel = originalModel;
        }
        /// <summary>
        /// The root model metadata
        /// </summary>
        public ModelMetadata OriginalModelMetadata { get; }
        /// <summary>
        /// The root model
        /// </summary>
        public object OriginalModel { get; }

        /// <summary>
        /// The current target's direct container's metadata
        /// </summary>
        public ModelMetadata ContainerModelMetadata { get; set; }
        /// <summary>
        /// The instance representing the current target's container
        /// </summary>
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

        /// <summary>
        /// Creates a new <see cref="ModelPostbindingContext"/> for top-level model binding operation.
        /// </summary>
        /// <param name="actionContext">
        /// The <see cref="ActionContext"/> associated with the binding operation.
        /// </param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/> to use for binding.</param>
        /// <param name="metadata"><see cref="ModelMetadata"/> associated with the model.</param>
        /// <param name="bindingInfo"><see cref="BindingInfo"/> associated with the model.</param>
        /// <param name="modelName">The name of the property or parameter being bound.</param>
        /// <param name="originalModel">The root model being rebound</param>
        /// <returns>A new instance of <see cref="ModelPostbindingContext"/>.</returns>
        public static ModelPostbindingContext CreateBindingContext(
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

            return new ModelPostbindingContext(metadata, originalModel)
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
