using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppContest.Data;
using AppContest.Models;
using MediatR;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using AutoMapper;
using System.Threading;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace AppContest.Pages.Contests
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;

        public EditModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public async Task<ActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            TempData["Message"] = $@"更新しました: <a href=""/Contests/Details/{Data.Id}"" class=""alert-link"">{Data.Name}</a>";
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

            [Required]
            [Display(Name = "名称")]
            public string Name { get; init; }

            [Display(Name = "説明")]
            public string Description { get; init; }

            [Display(Name = "開始日付")]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime? StartDate { get; init; }

            [Display(Name = "開始時間")]
            [DataType(DataType.Time)]
            [DisplayFormat(DataFormatString = "{0:HH:mm:ss}", ApplyFormatInEditMode = true)]
            public DateTime? StartTime { get; init; }

            [Required]
            [Display(Name = "終了日付")]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime EndDate { get; init; }

            [Display(Name = "終了時間")]
            [DataType(DataType.Time)]
            [DisplayFormat(DataFormatString = "{0:HH:mm:ss}", ApplyFormatInEditMode = true)]
            public DateTime? EndTime { get; init; }

            [Display(Name = "主催")]
            public string Host { get; init; }

            [Display(Name = "応募資格")]
            public string Requirements { get; init; }

            [Required]
            [Display(Name = "URL")]
            [DataType(DataType.Url)]
            public string Url { get; init; }

            [Display(Name = "備考")]
            public string Note { get; init; }

            [Display(Name = "非表示")]
            public bool IsHidden { get; init; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator(ApplicationDbContext context)
            {
                RuleFor(m => m.Name).NotNull().WithMessage("名称を入力してください。");
                RuleFor(m => m.Url).NotNull().WithMessage("URL を入力してください。");
                RuleFor(m => m).Must(m => !context.Contests.Any(c => c.Url == m.Url && c.Id != m.Id)).WithMessage("既に登録されている URL です。");
                RuleFor(m => m.StartDate)
                    .NotNull()
                    .When(m => m.StartTime.HasValue)
                    .WithMessage("開始日時を入力した場合は開始日付を入力してください。");
                RuleFor(m => m.StartDate)
                    .LessThan(m => m.EndDate)
                    .When(m => m.StartDate.HasValue)
                    .WithMessage("開始日付は終了日付より前の日付を入力してください。");
                RuleFor(m => m.StartDate)
                    .GreaterThanOrEqualTo(new DateTime(2015, 1, 1))
                    .WithMessage("開始日付は2015年より新しい日付を入力してください。");
                RuleFor(m => m.EndDate)
                    .GreaterThanOrEqualTo(new DateTime(2015, 1, 1))
                    .WithMessage("終了日付は2015年より新しい日付を入力してください。");
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Command, Contest>()
                    .ForMember(d => d.StartDate, opt => opt.MapFrom(s =>
                        s.StartDate.HasValue ?
                        s.StartTime.HasValue ?
                            new DateTime(s.StartDate.Value.Year, s.StartDate.Value.Month, s.StartDate.Value.Day, s.StartTime.Value.Hour, s.StartTime.Value.Minute, 0) :
                            new DateTime(s.StartDate.Value.Year, s.StartDate.Value.Month, s.StartDate.Value.Day) :
                            (DateTime?)null))
                    .ForMember(d => d.EndDate, opt => opt.MapFrom(s =>
                        s.EndTime.HasValue ? new DateTime(s.EndDate.Year, s.EndDate.Month, s.EndDate.Day, s.EndTime.Value.Hour, s.EndTime.Value.Minute, 0) :
                                            new DateTime(s.EndDate.Year, s.EndDate.Month, s.EndDate.Day)));

                CreateMap<Contest, Command>()
                    .ForMember(d => d.StartDate, opt => opt.MapFrom(s =>
                        s.StartDate.HasValue ?
                            new DateTime(s.StartDate.Value.Year, s.StartDate.Value.Month, s.StartDate.Value.Day) :
                            (DateTime?)null))
                    .ForMember(d => d.StartTime, opt => opt.MapFrom(s =>
                        s.StartDate.HasValue ?
                            new DateTime(1, 1, 1, s.StartDate.Value.Hour, s.StartDate.Value.Minute, 0) :
                            (DateTime?)null))
                    .ForMember(d => d.EndDate, opt => opt.MapFrom(s =>
                        new DateTime(s.EndDate.Year, s.EndDate.Month, s.EndDate.Day)))
                    .ForMember(d => d.EndTime, opt => opt.MapFrom(s =>
                        (DateTime?)(new DateTime(1, 1, 1, s.EndDate.Hour, s.EndDate.Minute, 0))));
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
            private readonly IMapper _mapper;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly UserManager<IdentityUser> _userManager;

            public CommandHandler(ApplicationDbContext context, IMapper mapper,
                IHttpContextAccessor httpContextAccessor,
                UserManager<IdentityUser> userManager)
            {
                _db = context;
                _mapper = mapper;
                _httpContextAccessor = httpContextAccessor;
                _userManager = userManager;
            }

            public async Task<Unit> Handle(Command command, CancellationToken token)
            {
                var contest = await _db.Contests.SingleOrDefaultAsync(x => x.Id == command.Id, token);

                _mapper.Map(command, contest);

                _db.Entry(contest).Property("RowVersion").OriginalValue = command.RowVersion;

                contest.LastUpdatedDateTime = DateTime.UtcNow.AddHours(9);
                contest.LastUpdatedBy = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

                return default;
            }
        }
    }
}
