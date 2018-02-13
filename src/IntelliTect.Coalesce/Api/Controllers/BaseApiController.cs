using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public abstract class BaseApiController<T, TDto, TContext> : Controller
        where T : class, new()
        where TDto : class, IClassDto<T>, new()
        where TContext : DbContext
    {
        protected BaseApiController(TContext db)
        {
            Db = db;
            EntityClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
        }

        public TContext Db { get; }

        /// <summary>
        /// A ClassViewModel representing the entity type T that is served by this controller,
        /// independent of the DTO that will encapsulate the type in inputs and outputs.
        /// </summary>
        protected ClassViewModel EntityClassViewModel { get; }

        /// <summary>
        /// For generated controllers, the type that the controller was generated for.
        /// For custom IClassDtos, this is the DTO type. Otherwise, this is the entity type.
        /// </summary>
        protected ClassViewModel GeneratedForClassViewModel { get; set; }

        protected Task<ItemResult<TDto>> GetImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedItemAsync<TDto>(id, parameters);
        }

        protected Task<ListResult<TDto>> ListImplementation(ListParameters listParameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedListAsync<TDto>(listParameters);
        }

        protected Task<ItemResult<int>> CountImplementation(FilterParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetCountAsync(parameters);
        }

        protected Task<ItemResult<TDto>> SaveImplementation(TDto dto, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            var kind = behaviors.DetermineSaveKind(dto).Kind;

            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.FromResult<ItemResult<TDto>>($"Creation of {GeneratedForClassViewModel.Name} items not allowed.");
            }
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.FromResult<ItemResult<TDto>>($"Editing of {GeneratedForClassViewModel.Name} items not allowed.");
            }

            return behaviors.SaveAsync(dto, dataSource, parameters);
        }

        protected Task<ItemResult<TDto>> DeleteImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            return behaviors.DeleteAsync<TDto>(id, dataSource, parameters);
        }

        protected async Task<FileResult> CsvDownloadImplementation(ListParameters parameters, IDataSource<T> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvTextImplementation(parameters, dataSource));
            return File(bytes, "application/x-msdownload", this.EntityClassViewModel.Name + ".csv");
        }

        protected async Task<string> CsvTextImplementation(ListParameters parameters, IDataSource<T> dataSource)
        {
            var listResult = await ListImplementation(parameters, dataSource);
            return IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(listResult.List);
        }

        protected async Task<IEnumerable<ItemResult>> CsvUploadImplementation(
            Microsoft.AspNetCore.Http.IFormFile file,
            IDataSource<T> dataSource,
            IBehaviors<T> behaviors,
            bool hasHeader = true)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No files uploaded");

            using (var stream = file.OpenReadStream())
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var csv = await reader.ReadToEndAsync();
                    return await CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);
                }
            }
        }

        protected async Task<IEnumerable<ItemResult>> CsvSaveImplementation(
            string csv,
            IDataSource<T> dataSource,
            IBehaviors<T> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<TDto>(csv, hasHeader);
            var resultList = new List<ItemResult>();
            foreach (var dto in list)
            {
                var parameters = new DataSourceParameters() { Includes = "none" };

                // Security: SaveImplementation is responsible for checking specific save/edit attribute permissions.
                var result = await SaveImplementation(dto, parameters, dataSource, behaviors);
                resultList.Add(new ItemResult { WasSuccessful = result.WasSuccessful, Message = result.Message });
            }
            return resultList;
        }
    }
}
