using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppContest.Data;
using AppContest.Models;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using System.Threading;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;

namespace AppContest.Pages.Contests
{
    [Authorize(Policy = "RequireContestAdministratorRole")]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);

        public enum Order
        {
            UpdateDateDesc,
            EndDateDesc,
        }

        public record Query : IRequest<Model>
        {
            public long? Id { get; init; }
            public bool? IsNotClosed { get; init; }
            public bool? IsClosed { get; init; }
            public bool? IsHidden { get; init; }
            public Order? Order { get; init; }
        }

        public record Model
        {
            public Contest PickedOutContest { get; set; }

            public bool IsNotClosed { get; init; }
            public bool IsClosed { get; init; }
            public bool IsHidden { get; init; }
            public Order Order { get; init; }

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
            private readonly IConfigurationProvider _configuration;

            public Handler(ApplicationDbContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Model> Handle(Query query, CancellationToken token)
            {
                var isHidden = query.IsHidden ?? false;
                var isNotClosed = query.IsNotClosed ?? false;
                var isClosed = query.IsClosed ?? false;
                var order = query.Order ?? Order.UpdateDateDesc;

                if (!isNotClosed && !isClosed)
                    isNotClosed = true;

                var jst = DateTime.UtcNow.AddHours(9);
                var today = new DateTime(jst.Year, jst.Month, jst.Day);

                var contests = await _db.Contests
                    .AsNoTracking()
                    .Where(x => x.IsHidden == isHidden &&
                           ((isNotClosed && x.EndDate >= today) ||
                           (isClosed && x.EndDate < today)))
                    .OrderByDescending(x => order == Order.UpdateDateDesc ? x.LastUpdatedDateTime : x.EndDate)
                    .ProjectTo<Model.Contest>(_configuration)
                    .ToListAsync(token);

                Model.Contest pickedOutContest = null;
                if (query.Id.HasValue)
                {
                    var c = contests
                        .SingleOrDefault(x => x.Id == query.Id);

                    pickedOutContest = c ?? await _db.Contests
                        .AsNoTracking()
                        .Where(x => x.Id == query.Id.Value)
                        .ProjectTo<Model.Contest>(_configuration)
                        .FirstOrDefaultAsync(token);
                }


                var viewModel = new Model
                {
                    PickedOutContest = pickedOutContest,
                    Contests = contests,
                    IsHidden = isHidden,
                    IsNotClosed = isNotClosed,
                    IsClosed = isClosed,
                    Order = order,
                };

                return viewModel;
            }
        }
    }
}
