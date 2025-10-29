using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class ChatResponseParameter : SparseDto, IGeneratedParameterDto<Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse>
    {
        public ChatResponseParameter() { }

        private string _response;
        private string _history;

        public string response
        {
            get => _response;
            set { _response = value; Changed(nameof(response)); }
        }
        public string history
        {
            get => _history;
            set { _history = value; Changed(nameof(history)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse entity, IMappingContext context)
        {
            var includes = context.Includes;

        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse MapToNew(IMappingContext context)
        {
            var includes = context.Includes;

            var entity = new Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse(
                response,
                history
            )
            {
                response = response,
                history = history,
            };

            return entity;
        }

        public Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse MapToModelOrNew(Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class ChatResponseResponse : IGeneratedResponseDto<Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse>
    {
        public ChatResponseResponse() { }

        public string response { get; set; }
        public string history { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Starter.Vue.Data.Services.AIAgentService.ChatResponse obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.response = obj.response;
            this.history = obj.history;
        }
    }
}
