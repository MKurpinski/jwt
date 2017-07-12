using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationSample.DataAccess
{
    public class TrelloContext: IdentityDbContext<IdentityUser>
    {
        public TrelloContext(DbContextOptions<TrelloContext> options) : base(options)
        {
        }
    }
}
