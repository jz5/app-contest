using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppContest.Data;
using AutoMapper;
using MediatR;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AppContest.Models;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace AppContest.Pages.Contests
{
    [Authorize]
    public class StartToCreateModel : PageModel
    {
        private readonly IMediator _mediator;

        public StartToCreateModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public record Query : IRequest<Model>
        {
            public string Url { get; init; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(m => m.Url).NotNull();
            }
        }

        public record Model
        {
            public string Url { get; init; }
            public bool Duplicated { get; init; }
            public string ThumbnailUrl { get; init; }

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

                [Display(Name = "非表示")]
                public bool IsHidden { get; init; }
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
            private readonly AutoMapper.IConfigurationProvider _configurationProvider;
            private readonly IConfiguration _configuration;

            public Handler(ApplicationDbContext db, AutoMapper.IConfigurationProvider configurationProvider, IConfiguration configuration)
            {
                _db = db;
                _configurationProvider = configurationProvider;
                _configuration = configuration;
            }

            public async Task<Model> Handle(Query query, CancellationToken token)
            {
                var url = query.Url?.Trim();
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return new Model
                    {
                        Url = url,
                        Contests = new List<Model.Contest>()
                    };
                }

                var contests = await _db.Contests
                    .AsNoTracking()
                    .Where(x =>
                        x.Url.Contains(uri.Host))
                    .OrderByDescending(x => x.EndDate)
                    .ProjectTo<Model.Contest>(_configurationProvider)
                    .ToListAsync(token);

                var thumbnailUrl =
                    "https://screenshot-playwright.azurewebsites.net/api/ScreenshotHttpTrigger?code=" +
                    _configuration["Pronama:ScreenshotFunctionCode"] + "&url=" + Uri.EscapeDataString(url);

                var viewModel = new Model
                {
                    Url = url,
                    Duplicated = contests.Any(x => x.Url == url),
                    ThumbnailUrl = thumbnailUrl,
                    Contests = contests,
                };

                return viewModel;
            }
        }
    }
}
