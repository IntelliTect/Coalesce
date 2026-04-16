
global using TUnit.Core;
global using TUnit.Assertions;
global using TUnit.Assertions.Extensions;
global using Moq;
global using IntelliTect.Coalesce;
global using IntelliTect.Coalesce.Api;
global using IntelliTect.Coalesce.Models;
global using Microsoft.EntityFrameworkCore;
global using Coalesce.Starter.Vue.Data;
global using Coalesce.Starter.Vue.Data.Test;
global using System.Security.Claims;
#if (Identity || ExampleModel || TrackingBase || AuditLogs)
global using Coalesce.Starter.Vue.Data.Models;
#endif