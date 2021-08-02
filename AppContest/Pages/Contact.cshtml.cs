using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AppContest.Data;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR.Pipeline;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AppContest.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IMediator _mediator;

        public ContactModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync()
            => Data = await _mediator.Send(new Query());

        public async Task<IActionResult> OnPostAsync()
        {
            var succeeded = await _mediator.Send(Data);

            TempData["Message"] = Data.Message;
            TempData["Succeeded"] = succeeded;

            return this.RedirectToPageJson("Contact");
        }

        public record Query : IRequest<Command>
        {
            public string Message { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            public QueryHandler()
            {
            }

            public Task<Command> Handle(Query message, CancellationToken token)
            {
                return Task.FromResult(new Command());
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.Message).NotNull().WithMessage("メッセージを入力してください。");
            }
        }

        public record Command : IRequest<bool>
        {
            [Required]
            [Display(Name = "メッセージ")]
            public string Message { get; init; }
        }

        public class CommandHandler : IRequestHandler<Command, bool>
        {
            private readonly ApplicationDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEmailSender _emailSender;

            public CommandHandler(ApplicationDbContext context, IMapper mapper, IEmailSender emailSender)
            {
                _db = context;
                _mapper = mapper;
                _emailSender = emailSender;
            }

            public async Task<bool> Handle(Command command, CancellationToken token)
            {
                //var ms = Regex.Matches(command.Message, @"(?<url>https?://[\w/:%#\$&\?\(\)~\.=\+\-]+)");

                await _emailSender.SendEmailAsync("info@pronama.jp", "アプリコンテストまとめ 情報提供", command.Message).ConfigureAwait(false);
                return true;
            }
        }


        public class ExceptionHandler : RequestExceptionHandler<Command, bool, Exception>
        {
            protected override void Handle(Command request, Exception exception, RequestExceptionHandlerState<bool> state)
            {
                //var result = new ContentResult();
                //var content = JsonConvert.SerializeObject(exception,
                //    new JsonSerializerSettings
                //    {
                //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //    });
                //result.Content = content;
                //result.StatusCode = 500;
                //result.ContentType = "application/json";

                state.SetHandled(false);
            }
        }


    }
}
