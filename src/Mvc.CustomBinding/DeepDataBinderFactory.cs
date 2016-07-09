using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.DotNet.InternalAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mvc.CustomBinding
{
    public class DeepDataBinderFactory : IModelBinderFactory
    {
        private readonly IModelMetadataProvider metadataProvider;
        private readonly IModelBinderFactory original;

        public DeepDataBinderFactory(IModelMetadataProvider metadataProvider, IModelBinderFactory original)
        {
            this.metadataProvider = metadataProvider;
            this.original = original;
        }
        public IModelBinder CreateBinder(ModelBinderFactoryContext context)
        {
            var result = original.CreateBinder(context);

            if (result is CustomModelBinder)
            {
                return result;
            }
            else
            {
                var rebinder = CreateRebinder(context);
                if (rebinder == null)
                {
                    return result;
                }
                else
                {
                    return new CustomModelRebinder(result, rebinder);
                }
            }
        }

        private IModelBinder CreateRebinder(ModelBinderFactoryContext context)
        {
            if (context.Metadata.IsCollectionType)
            {
                return new CollectionModelBinderProvider().GetBinder(CreateModelBinderProviderContext(context));
            }
            else if (context.Metadata.IsComplexType)
            {
                return new ComplexTypeModelBinderProvider().GetBinder(CreateModelBinderProviderContext(context));
            }
            return null;
        }

        private ModelBinderProviderContext CreateModelBinderProviderContext(ModelBinderFactoryContext context)
        {
            return new DefaultModelBinderProviderContext(this, context);
        }

        private class DefaultModelBinderProviderContext : ModelBinderProviderContext
        {
            private readonly DeepDataBinderFactory factory;

            public DefaultModelBinderProviderContext(
                DeepDataBinderFactory factory,
                ModelBinderFactoryContext factoryContext)
            {
                this.factory = factory;
                Metadata = factoryContext.Metadata;
                BindingInfo = new BindingInfo
                {
                    BinderModelName = Metadata.BinderModelName,
                    BinderType = Metadata.BinderType,
                    BindingSource = Metadata.BindingSource,
                    PropertyFilterProvider = Metadata.PropertyFilterProvider,
                };
                MetadataProvider = factory.metadataProvider;
                Visited = new Dictionary<Key, IModelBinder>();
            }

            public override BindingInfo BindingInfo { get; }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public Dictionary<Key, IModelBinder> Visited { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                if (metadata == null)
                {
                    throw new ArgumentNullException(nameof(metadata));
                }

                // For non-root nodes we use the ModelMetadata as the cache token. This ensures that all non-root
                // nodes with the same metadata will have the the same binder. This is OK because for an non-root
                // node there's no opportunity to customize binding info like there is for a parameter.
                var token = metadata;

                return factory.CreateBinder(new ModelBinderFactoryContext
                {
                    BindingInfo = new BindingInfo
                    {
                        BinderModelName = metadata.BinderModelName ?? BindingInfo?.BinderModelName,
                        BinderType = metadata.BinderType ?? BindingInfo?.BinderType,
                        BindingSource = metadata.BindingSource ?? BindingInfo?.BindingSource,
                        PropertyFilterProvider =
                            metadata.PropertyFilterProvider ?? BindingInfo?.PropertyFilterProvider,
                    },
                    Metadata = metadata,
                    CacheToken = token,
                });
            }
        }


        // This key allows you to specify a ModelMetadata which represents the type/property being bound
        // and a 'token' which acts as an arbitrary discriminator.
        //
        // This is necessary because the same metadata might be bound as a top-level parameter (with BindingInfo on
        // the ParameterDescriptor) or in a call to TryUpdateModel (no BindingInfo) or as a collection element.
        //
        // We need to be able to tell the difference between these things to avoid over-caching.
        private struct Key : IEquatable<Key>
        {
            private readonly ModelMetadata _metadata;
            private readonly object _token; // Explicitly using ReferenceEquality for tokens.

            public Key(ModelMetadata metadata, object token)
            {
                _metadata = metadata;
                _token = token;
            }

            public bool Equals(Key other)
            {
                return _metadata.Equals(other._metadata) && object.ReferenceEquals(_token, other._token);
            }

            public override bool Equals(object obj)
            {
                var other = obj as Key?;
                return other.HasValue && Equals(other.Value);
            }

            public override int GetHashCode()
            {
                var hashFirst = _metadata.GetHashCode();
                return ((hashFirst << 5) + hashFirst) ^ RuntimeHelpers.GetHashCode(_token);
            }

            public override string ToString()
            {
                if (_metadata.MetadataKind == ModelMetadataKind.Type)
                {
                    return $"{_token} (Type: '{_metadata.ModelType.Name}')";
                }
                else
                {
                    return $"{_token} (Property: '{_metadata.ContainerType.Name}.{_metadata.PropertyName}' Type: '{_metadata.ModelType.Name}')";
                }
            }
        }
    }
}
