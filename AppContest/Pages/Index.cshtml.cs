using AppContest.Data;
using AppContest.Infrastructure;
using AppContest.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppContest.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync()
            => Data = await _mediator.Send(new Query());

        public record Query : IRequest<Model>
        {
        }

        public record Model
        {
            public IList<Contest> Contests { get; init; }
            public IList<Contest> RecentlyAddedContests { get; init; }

            public record Contest
            {
                public long Id { get; init; }

                [Display(Name = "名称")]
                public string Name { get; init; }

                [Display(Name = "説明")]
                public string Description { get; init; }

                [Display(Name = "開始")]
                [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
                public DateTime? StartDate { get; init; }

                [Display(Name = "終了")]
                [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
                public DateTime EndDate { get; init; }

                [Display(Name = "主催")]
                public string Host { get; init; }

                [Display(Name = "応募資格")]
                public string Requirements { get; init; }

                [Display(Name = "URL")]
                public string Url { get; init; }

                [Display(Name = "備考")]
                public string Note { get; init; }

                public DateTime CreationDateTime { get; init; }

                public string Thumbnail() => Url.Thumbnail();
            }

        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Contest, Model.Contest>();
            }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ApplicationDbContext _db;
            private readonly IConfigurationProvider _configuration;

            private readonly UserManager<IdentityUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;


            public Handler(ApplicationDbContext db, IConfigurationProvider configuration,
                UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
            {
                _db = db;
                _configuration = configuration;
                _userManager = userManager;
                _roleManager = roleManager;
            }

            public async Task<Model> Handle(Query query, CancellationToken token)
            {
                var jst = DateTime.UtcNow.AddHours(9);
                var year = jst.Year;
                var tommorow = new DateTime(jst.Year, jst.Month, jst.Day).AddDays(1);

                var contests = await _db.Contests
                    .AsNoTracking()
                    .Where(x =>
                        tommorow <= x.EndDate &&
                        !x.IsHidden)
                    .OrderByDescending(x => x.EndDate)
                    .ProjectTo<Model.Contest>(_configuration)
                    .ToListAsync(token);

                var viewModel = new Model
                {
                    Contests = contests,
                    RecentlyAddedContests = contests.OrderByDescending(x => x.CreationDateTime).Take(5).ToList()
                };


                return viewModel;
            }
        }
    }
}
