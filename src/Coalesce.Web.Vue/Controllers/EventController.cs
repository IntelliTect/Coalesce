using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace Coalesce.Web.Vue.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Get(int eventId)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true })) 
            {
                var result = client.GetAsync($"https://myriad.corp.com/api/Event/Get/{eventId}?includes=&dataSource=EventForDetails")
                .Result.Content.ReadAsStringAsync().Result;

                return Content(result, "application/json");
            }

            // var app1 = new
            // {
            //     ApplicationId = 1,
            //     Name = "CCB",
            // };
            // var app2 = new
            // {
            //     ApplicationId = 2,
            //     Name = "MDM",
            // };
            // var app3 = new
            // {
            //     ApplicationId = 3,
            //     Name = "SOA",
            // };
            // var list = new[]
            // {
            //     new
            //     {
            //         EventId = 1,
            //         Title = "First Event",
            //         Notes = new[]
            //         {
            //             new
            //             {
            //                 EventNoteId = 1,
            //                 Text = "Note one",
            //                 EventId = 1,
            //             },
            //             new
            //             {
            //                 EventNoteId = 2,
            //                 Text = "Note 2!!!!",
            //                 EventId = 1,
            //             },
            //         },
            //         EventApplications = new [] {
            //             new
            //             {
            //                 EventApplicationId = 1,
            //                 EventId = 1,
            //                 ApplicationId = 1,
            //                 Application = app1
            //             },
            //             new
            //             {
            //                 EventApplicationId = 2,
            //                 EventId = 1,
            //                 ApplicationId = 2,
            //                 Application = app2
            //             },
            //         }
            //     },
            //     new
            //     {
            //         EventId = 2,
            //         Title = "Go-Live Event",
            //         Notes = new[]
            //         {
            //             new
            //             {
            //                 EventNoteId = 3,
            //                 Text = "Note one",
            //                 EventId = 2,
            //             },
            //             new
            //             {
            //                 EventNoteId = 4,
            //                 Text = "Note 2!!!!",
            //                 EventId = 2,
            //             },
            //         },
            //         EventApplications = new [] {
            //             new
            //             {
            //                 EventApplicationId = 3,
            //                 EventId = 2,
            //                 ApplicationId = 3,
            //                 Application = app3
            //             },
            //             new
            //             {
            //                 EventApplicationId = 4,
            //                 EventId = 2,
            //                 ApplicationId = 2,
            //                 Application = app2
            //             },
            //         }
            //     }
            // };


            // return list.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        }
    }
}
