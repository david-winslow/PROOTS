




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OTS.DAL.Core;

namespace OTS.DAL
{
    public class HttpUnitOfWork : UnitOfWork
    {
        public HttpUnitOfWork(ApplicationDbContext context, IHttpContextAccessor httpAccessor) : base(context)
        {
            context.CurrentUserId = httpAccessor.HttpContext?.User.FindFirst(ClaimConstants.Subject)?.Value?.Trim();
        }
    }
}
