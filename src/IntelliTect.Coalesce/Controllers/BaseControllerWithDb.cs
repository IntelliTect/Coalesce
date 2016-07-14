using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Controllers
{
    public class BaseControllerWithDb<TContext> : Controller
        where TContext : DbContext
    {
        protected BaseControllerWithDb() : base() { }

        private TContext _Db;
        public TContext Db
        {
            get
            {
                if (_Db == null)
                {
                    _Db = HttpContext?.RequestServices.GetRequiredService<TContext>();
                }
                return _Db;
            }
            set
            {
                _Db = value;
            }
        }

        private IHostingEnvironment _AppEnv;
        protected IHostingEnvironment AppEnv
        {
            get
            {
                if (_AppEnv == null)
                {
                    _AppEnv = HttpContext?.RequestServices.GetRequiredService<IHostingEnvironment>();
                }
                return _AppEnv;
            }
        }

        protected string ControllerName
        {
            get { return this.GetType().Name.Replace("Controller", ""); }
        }
    }
}
