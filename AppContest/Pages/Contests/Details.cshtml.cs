using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppContest.Data;
using AppContest.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Xml.Linq;

namespace AppContest.Pages.Contests
{
    [Authorize(Policy = "RequireContestAdministratorRole")]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;

        public DetailsModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Model Data { get; set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public record Query : IRequest<Model>
        {
            public long? Id { get; init; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(m => m.Id).NotNull();
            }
        }

        public record Model : IRequest
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

            [Display(Name = "登録日時")]
            public DateTime CreationDateTime { get; init; }
            [Display(Name = "登録者")]
            public IdentityUser CreatedBy { get; init; }
            [Display(Name = "更新日時")]
            public DateTime LastUpdatedDateTime { get; init; }
            [Display(Name = "更新者")]
            public IdentityUser LastUpdatedBy { get; init; }

            public string Markdown()
            {
                var started = StartDate.HasValue && StartDate.Value <= DateTime.UtcNow.AddHours(9) || !StartDate.HasValue;
                return @$"{Name}が{(started ? "開催中" : "開催予定")}（{EndDate:M/d}〆切）

{Host}主催の、**{Name}**が{(started ? "開催中" : "開催予定")}です。


[{Name}]({Url})

## 募集内容

{Description}

## 応募資格

* {Requirements}

## 応募期限

* {((EndDate.Hour == 0 && EndDate.Minute == 0) ? EndDate.ToString("yyyy/MM/dd") : EndDate.ToString("yyyy/MM/dd HH:mm"))}

## 表彰


## 主催

* {Host}

---

※ 詳細は、[{Name}]({Url}) を確認してください。

アプリコンテストを調べるときは、プロ生提供の [アプリコンテストまとめ](https://contest.pronama.jp/) も活用してみてください。
";
            }
        }



        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Contest, Model>();
            }
        }

        public class QueryHandler : IRequestHandler<Query, Model>
        {
            private readonly ApplicationDbContext _db;
            private readonly AutoMapper.IConfigurationProvider _configuration;

            public QueryHandler(ApplicationDbContext db, AutoMapper.IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Model> Handle(Query message, CancellationToken token) => await _db.Contests
                .AsNoTracking()
                .Where(x => x.Id == message.Id)
                .ProjectTo<Model>(_configuration)
                .SingleOrDefaultAsync(token);

        }


    }
}
