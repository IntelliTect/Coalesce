
using Intellitect.ComponentModel.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Intellitect.ComponentModel.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Intellitect.ComponentModel.Data;
using System.Linq.Dynamic;
using Intellitect.ComponentModel.Mapping;

// Model Namespaces 
using Coalesce.Domain;
using Microsoft.EntityFrameworkCore;
using Intellitect.ComponentModel.DataAnnotations;

namespace Coalesce.Web.Api
{
    public partial class PersonController
         : LocalBaseApiController<Person>
    {
        protected override bool BeforeSave(Person dto, Person obj)
        {
            if (dto.FirstName.Contains("[user]"))
            {
                dto.FirstName = dto.FirstName.Replace("[user]", User.Identity.Name);
            }
            return true;
        }

        protected override bool AfterSave(Person dto, Person obj, Person orig, AppDbContext context)
        {
            // Add the company name to the last name if it changed.
            if (obj.CompanyId != orig.CompanyId && !obj.LastName.Contains(obj.Company.Name))
            {
                obj.LastName = obj.LastName + "-" + obj.Company.Name;
                Db.SaveChanges();
            }
            return true;
        }
    }

}