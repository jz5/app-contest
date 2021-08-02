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
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IMediator _mediator;

        public DeleteModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public async Task<ActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            TempData["Message"] = "削除しました";
            return this.RedirectToPageJson("Index");
        }

        public record Query : IRequest<Command>
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

        public record Command : IRequest
        {
            public long Id { get; init; }
            public byte[] RowVersion { get; init; }

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
            [DataType(DataType.Url)]
            public string Url { get; init; }

            [Display(Name = "備考")]
            public string Note { get; init; }

            [Display(Name = "非表示")]
            public bool IsHidden { get; init; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Contest, Command>();

            }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly ApplicationDbContext _db;
            private readonly IConfigurationProvider _configuration;

            public QueryHandler(ApplicationDbContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Command> Handle(Query message, CancellationToken token) => await _db.Contests
                .Where(x => x.Id == message.Id)
                .ProjectTo<Command>(_configuration)
                .SingleOrDefaultAsync(token);

        }

        public class CommandHandler : IRequestHandler<Command>
        {
            private readonly ApplicationDbContext _db;

            public CommandHandler(ApplicationDbContext context)
            {
                _db = context;
            }

            public async Task<Unit> Handle(Command command, CancellationToken token)
            {
                var contest = await _db.Contests.SingleOrDefaultAsync(x => x.Id == command.Id, token);
                _db.Contests.Remove(contest);

                return default;
            }
        }
    }
}
