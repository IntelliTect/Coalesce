using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.Api.Behaviors
{
    public class BehaviorsFactory : IBehaviorsFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ReflectionRepository reflectionRepository;

        public BehaviorsFactory(IServiceProvider serviceProvider, ReflectionRepository reflectionRepository)
        {
            this.serviceProvider = serviceProvider;
            this.reflectionRepository = reflectionRepository;
        }

        /// <summary>
        /// Defines all marker interfaces for defaults, along with their default concrete implementation.
        /// </summary>
        internal static readonly Dictionary<Type, Type> DefaultTypes = new Dictionary<Type, Type>
        {
            { typeof(IEntityFrameworkBehaviors<,>), typeof(StandardBehaviors<,>) }
            // Future: may be other kinds of defaults (non-EF)
        };

        protected Type GetDefaultBehaviorsType(ClassViewModel servedType)
        {
            // If other kinds of default are handled here in the future, add them to the collection above.

            var tContext = reflectionRepository.DbContexts.FirstOrDefault(c => c.Entities.Any(e => e.ClassViewModel.Equals(servedType)));
            var BehaviorsType = typeof(IEntityFrameworkBehaviors<,>).MakeGenericType(
                servedType.Type.TypeInfo,
                tContext.ClassViewModel.Type.TypeInfo
            );
            return BehaviorsType;
        }

        public object GetDefaultBehaviors(ClassViewModel servedType)
        {
            var behaviorsType = GetDefaultBehaviorsType(servedType);
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, behaviorsType);
        }


        protected Type GetBehaviorsType(ClassViewModel servedType, ClassViewModel declaredFor)
        {
            var behaviorsClassViewModel = reflectionRepository.Behaviors
                .SingleOrDefault(usage => usage.DeclaredFor == declaredFor)
                ?.StrategyClass;

            if (behaviorsClassViewModel == null)
            {
                return GetDefaultBehaviorsType(servedType);
            }
            else
            {
                return behaviorsClassViewModel.Type.TypeInfo;
            }
        }

        public object GetBehaviors(ClassViewModel servedType, ClassViewModel declaredFor)
        {
            var behaviorsType = GetBehaviorsType(servedType, declaredFor);
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, behaviorsType);
        }
    }

}
