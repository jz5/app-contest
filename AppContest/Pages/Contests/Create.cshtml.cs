using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppContest.Data;
using AppContest.Models;
using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AppContest.Pages.Contests
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        public CreateModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public async Task<ActionResult> OnPostAsync()
        {
            var id = await _mediator.Send(Data);

            TempData["Message"] = $@"登録しました: <a href=""/Contests/Details/{id}"" class=""alert-link"">{Data.Name}</a>";
            return this.RedirectToPageJson("Index");
        }

        public record Query : IRequest<Command>
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

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            public QueryHandler()
            {
            }

            public Task<Command> Handle(Query message, CancellationToken token)
            {
                var a = new Command
                {
                    Url = message.Url
                };
                return Task.FromResult(a);
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(ApplicationDbContext context)
            {
                RuleFor(m => m.Name).NotNull().WithMessage("名称を入力してください。");
                RuleFor(m => m.Url).NotNull().WithMessage("URL を入力してください。");
                RuleFor(m => m).Must(m => !context.Contests.Any(c => c.Url == m.Url)).WithMessage("既に登録されている URL です。");
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

        public record Command : IRequest<long>
        {
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
            public DateTime EndDate { get; init; } = DateTime.UtcNow.AddHours(9);

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

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Command, Contest>()
                .ForMember(d => d.StartDate, opt => opt.MapFrom(s =>
                    s.StartDate.HasValue ?
                    s.StartTime.HasValue ?
                        new DateTime(s.StartDate.Value.Year, s.StartDate.Value.Month, s.StartDate.Value.Day, s.StartTime.Value.Hour, s.StartTime.Value.Minute, 0) :
                        new DateTime(s.StartDate.Value.Year, s.StartDate.Value.Month, s.StartDate.Value.Day) :
                        (DateTime?)null))
                .ForMember(d => d.EndDate, opt => opt.MapFrom(s =>
                    s.EndTime.HasValue ? new DateTime(s.EndDate.Year, s.EndDate.Month, s.EndDate.Day, s.EndTime.Value.Hour, s.EndTime.Value.Minute, 0) :
                                        new DateTime(s.EndDate.Year, s.EndDate.Month, s.EndDate.Day)));

        }

        public class CommandHandler : IRequestHandler<Command, long>
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

            public async Task<long> Handle(Command command, CancellationToken token)
            {
                var contest = _mapper.Map<Command, Contest>(command);

                _mapper.Map(command, contest);

                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                contest.CreationDateTime = DateTime.UtcNow.AddHours(9);
                contest.CreatedBy = user;
                contest.LastUpdatedDateTime = DateTime.UtcNow.AddHours(9);
                contest.LastUpdatedBy = user;

                await _db.Contests.AddAsync(contest, token);
                await _db.SaveChangesAsync(token);

                return contest.Id;
            }
        }
    }
}
