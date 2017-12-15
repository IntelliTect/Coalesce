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

        protected Type GetBehaviorsType(ClassViewModel servedType)
        {
            var behaviorsClassViewModel = reflectionRepository.Behaviors
                .SingleOrDefault(usage => usage.SourceFor == servedType)
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

        public object GetBehaviors(ClassViewModel servedType)
        {
            var behaviorsType = GetBehaviorsType(servedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, behaviorsType);
        }


        protected Type GetDefaultBehaviorsType(ClassViewModel servedType)
        {
            var tContext = reflectionRepository.DbContexts.FirstOrDefault(c => c.Entities.Any(e => e.ClassViewModel.Equals(servedType)));
            var BehaviorsType = typeof(StandardBehaviors<,>).MakeGenericType(
                servedType.Type.TypeInfo,
                tContext.ClassViewModel.Type.TypeInfo
            );
            return BehaviorsType;
        }

        public object GetDefaultBehaviors(ClassViewModel servedType)
        {
            var behaviorsType = GetDefaultBehaviorsType(servedType);
            return ActivatorUtilities.CreateInstance(serviceProvider, behaviorsType);
        }
    }

}
