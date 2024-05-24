using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api
{
    [JsonConverter(typeof(BulkSaveRequestItemConverter))]
    public abstract class BulkSaveRequestItem
    {
        /// <summary>
        /// The type of the item being saved.
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// The action taken agains the item - either 'save', 'delete', or 'none'
        /// </summary>
        public string Action { get; set; } = null!;

        /// <summary>
        /// If true, this is the root item that should be loaded and served as the response after the operation completes.
        /// </summary>
        public bool Root { get; set; }

        /// <summary>
        /// A map from the names of unresolvable foreign keys to the <see cref="PrimaryRef"/> 
        /// of the item that the foreign key should reference once that principal item is created.
        /// Also contains an entry keyed by the model's primary key name that defines the <see cref="PrimaryRef"/> for this item.
        /// </summary>
        public Dictionary<string, int>? Refs { get; set; }

        /// <summary>
        /// An IClassDto containing the data for the item.
        /// </summary>
        public object Data { get; set; } = null!;


        /// <summary>
        /// The ClassViewModel for the entity type.
        /// </summary>
        internal ClassViewModel ClassViewModel { get; set; } = null!;

        /// <summary>
        /// The ClassViewModel for the DTO type.
        /// </summary>
        internal ClassViewModel ParamDtoClassViewModel { get; set; } = null!;

        internal ClassViewModel DeclaredForClassViewModel => ParamDtoClassViewModel.Type.IsA(typeof(GeneratedParameterDto<>))
            ? ClassViewModel // Strategies and annotations for generated DTOs are always declared on the entity itself
            : ParamDtoClassViewModel // Strategies and annotations for non-generated DTOs (i.e. custom DTOs) will be declared for that custom DTO.
            ;

        internal PropertyViewModel InputDtoPrimaryKey => ParamDtoClassViewModel.PrimaryKey ?? throw new Exception("Bulk saves cannot operate on unkeyed models");

        internal virtual object? PrimaryKey => InputDtoPrimaryKey.PropertyInfo.GetValue(Data);

        internal string PrimaryRefName => InputDtoPrimaryKey.JsonName;
        internal int? PrimaryRef => Refs?.TryGetValue(PrimaryRefName, out int v) == true ? v : null;

        internal abstract void Initialize(IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory);

        internal abstract Task<SaveKind> DetermineSaveKindAsync(DataSourceParameters parameters);
        internal abstract Task<ItemResult> SaveAsync(DataSourceParameters parameters);
        internal abstract Task<ItemResult> DeleteAsync(DataSourceParameters parameters);
    }

    internal class BulkSaveRequestItem<T, TInputDto> : BulkSaveRequestItem
        where T : class
        where TInputDto : class, IParameterDto<T>, new()
    {
        /// <summary>
        /// An IClassDto containing the data for the item.
        /// </summary>
        internal TInputDto Dto
        {
            get => (TInputDto)Data;
            set => Data = value;
        }

        private ModelCapturingDto? ResultDto { get; set; }

        internal override object? PrimaryKey => ResultDto?.Model is { } 
            ? DeclaredForClassViewModel.PrimaryKey!.PropertyInfo.GetValue(ResultDto.Model)
            : InputDtoPrimaryKey.PropertyInfo.GetValue(Dto);

        internal IDataSource<T>? DataSource { get; set; }
        internal IBehaviors<T>? Behaviors { get; set; }

        internal override async Task<SaveKind> DetermineSaveKindAsync(DataSourceParameters parameters)
        {
            return (await Behaviors!.DetermineSaveKindAsync(Dto, DataSource!, parameters)).Kind;
        }

        internal override async Task<ItemResult> SaveAsync(DataSourceParameters parameters)
        {
            var result = await Behaviors!.SaveAsync<TInputDto, ModelCapturingDto>(Dto, DataSource!, parameters);
            if (!result.WasSuccessful) return result;

            ResultDto = result.Object;

            return result;
        }

        internal override async Task<ItemResult> DeleteAsync(DataSourceParameters parameters)
        {
            if (PrimaryKey is null) throw new InvalidOperationException("Unable to delete item - primary key was not provided");
            return await Behaviors!.DeleteAsync<ModelCapturingDto>(PrimaryKey, DataSource!, parameters);
        }

        internal override void Initialize(IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory)
        {
            // Clear non-initial state in case we're executing a retry.
            ResultDto = null;

            if (DataSource is not null) return;

            ClassViewModel declaredFor = DeclaredForClassViewModel;
            DataSource = dataSourceFactory.GetDefaultDataSource<T>(declaredFor);
            Behaviors = behaviorsFactory.GetBehaviors<T>(declaredFor);
        }

        private class ModelCapturingDto : IResponseDto<T>
        {
            public T? Model { get; set; }

            public void MapFrom(T obj, IMappingContext context, IncludeTree? tree = null)
            {
                Model = obj;
            }
        }
    }
}

