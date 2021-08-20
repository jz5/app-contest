using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppContest.Data;
using AppContest.Models;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using System.Threading;
using AutoMapper.QueryableExtensions;
using AppContest.Infrastructure;
using FluentValidation;

namespace AppContest.Pages.Contests
{
    public class ClosedModel : PageModel
    {
        private readonly IMediator _mediator;

        public ClosedModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public record Query : IRequest<Model>
        {
            public int? Year { get; init; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(m => m.Year).InclusiveBetween(2015, DateTime.UtcNow.AddHours(9).Year);
            }
        }

        public record Model
        {
            public int Year { get; init; }

            public IList<Contest> Contests { get; init; }

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
                var year = query.Year ?? jst.Year;
                var tommorow = new DateTime(jst.Year, jst.Month, jst.Day).AddDays(1);

                DateTime sd, ed;
                if (year == jst.Year)
                {
                    sd = new DateTime(year, 1, 1);
                    ed = new DateTime(year, jst.Month, jst.Day).AddDays(1);
                }
                else
                {
                    sd = new DateTime(year, 1, 1);
                    ed = new DateTime(year, 12, 31).AddDays(1);
                }

                var contests = await _db.Contests
                    .AsNoTracking()
                    .Where(x =>
                        x.EndDate < tommorow &&
                        //sd <= x.StartDate && x.StartDate < ed ||
                        sd <= x.EndDate && x.EndDate < ed &&
                        !x.IsHidden)
                    .OrderByDescending(x => x.EndDate)
                    .ProjectTo<Model.Contest>(_configuration)
                    .ToListAsync(token);

                var viewModel = new Model
                {
                    Year = year,
                    Contests = contests,
                };

                return viewModel;
            }
        }
    }
}
