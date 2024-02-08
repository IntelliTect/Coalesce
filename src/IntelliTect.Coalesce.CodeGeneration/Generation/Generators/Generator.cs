using System;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{

    public abstract class Generator : IGenerator
    {
        public const string DisabledJsonPropertyName = "disabled";

        protected Generator(GeneratorServices services)
        {
            Logger = services.LoggerFactory.CreateLogger(GetType().Name);
            Configure(services.CoalesceConfiguration);
            DryRun = services.CoalesceConfiguration.DryRun;
        }

        protected ILogger Logger { get; }

        public bool DryRun { get; private set; }

        public bool IsDisabled { get; private set; }

        /// <summary>
        /// Output path that will be set in C# when instantiating generators.
        /// For the ultimate destination of generated files, use <see cref="EffectiveOutputPath"/>
        /// </summary>
        public string DefaultOutputPath { get; set; }

        /// <summary>
        /// Configues a path relative to the default output path for a generator for that generator's output to be placed in.
        /// </summary>
        [GeneratorConfig]
        public string TargetDirectory { get; set; }

        /// <summary>
        /// The desired destination path of generated files, including any user configuration through <see cref="TargetDirectory"/>
        /// </summary>
        public abstract string EffectiveOutputPath { get; }

        public abstract Task GenerateAsync();

        protected virtual void Configure(CoalesceConfiguration coalesceConfiguration)
        {
            var allConfig = coalesceConfiguration.GeneratorConfig;

            bool foundConfig =
                allConfig.TryGetValue(this.GetType().FullName, out JObject genConfig)
             || allConfig.TryGetValue(this.GetType().Name, out genConfig);

            if (foundConfig)
            {
                Configure(genConfig);
            }
        }

        public virtual void Configure(JObject obj)
        {
            IsDisabled = obj.Value<bool>(DisabledJsonPropertyName);

            var rvm = new ReflectionClassViewModel(this.GetType());
            var properties = rvm.Properties
                .Where(p => p.HasAttribute<GeneratorConfigAttribute>() && p.HasSetter);

            foreach (var configProp in properties)
            {
                var propType = configProp.Type.TypeInfo;
                if (obj.TryGetValue(configProp.Name, StringComparison.OrdinalIgnoreCase, out JToken value))
                {
                    configProp.PropertyInfo.SetValue(this, value.ToObject(propType));
                }
            }
        }
    }
}