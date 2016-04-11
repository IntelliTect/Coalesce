using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Controllers
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
                    _Db = Resolver.GetRequiredService<TContext>();
                }
                return _Db;
            }
            set
            {
                _Db = value;
            }
        }

        private IApplicationEnvironment _AppEnv;
        protected IApplicationEnvironment AppEnv
        {
            get
            {
                if (_AppEnv == null)
                {
                    _AppEnv = Resolver.GetRequiredService<IApplicationEnvironment>();
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
