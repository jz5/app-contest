using AppContest.Data;
using AppContest.Infrastructure;
using AppContest.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AppContest.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public record Query : IRequest<Model>
        {
            public string Sort { get; init; }
        }

        public record Model
        {
            public string Sort { get; init; }

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
            private readonly AutoMapper.IConfigurationProvider _configuration;

            public Handler(ApplicationDbContext db, AutoMapper.IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Model> Handle(Query query, CancellationToken token)
            {
                var jst = DateTime.UtcNow.AddHours(9);
                var year = jst.Year;
                var tommorow = new DateTime(jst.Year, jst.Month, jst.Day).AddDays(1);

                List<Model.Contest> contests;
                if (query.Sort == "date-asc")
                {
                    contests = await _db.Contests
                        .AsNoTracking()
                        .Where(x =>
                            tommorow <= x.EndDate &&
                            !x.IsHidden)
                        .OrderBy(x => x.EndDate)
                        .ProjectTo<Model.Contest>(_configuration)
                        .ToListAsync(token);
                }
                else
                {
                    contests = await _db.Contests
                        .AsNoTracking()
                        .Where(x =>
                            tommorow <= x.EndDate &&
                            !x.IsHidden)
                        .OrderByDescending(x => x.EndDate)
                        .ProjectTo<Model.Contest>(_configuration)
                        .ToListAsync(token);
                }

                var viewModel = new Model
                {
                    Sort = query.Sort,
                    Contests = contests,
                    RecentlyAddedContests = contests.OrderByDescending(x => x.CreationDateTime).Take(5).ToList()
                };


                return viewModel;
            }
        }
    }
}
