using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppContest.Data;
using AppContest.Models;
using AutoMapper;
using MediatR;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using static AppContest.Pages.LicensesModel.Model;

namespace AppContest.Pages
{
    public class LicensesModel : PageModel
    {
        //public void OnGet()
        //{
        //}

        private readonly IMediator _mediator;

        public LicensesModel(IMediator mediator)
            => _mediator = mediator;

        public Model Data { get; private set; }

        public async Task OnGetAsync(Query query)
            => Data = await _mediator.Send(query);


        public record Query : IRequest<Model>
        {
        }

        public record Model
        {
            public IList<LicenseFile> LicenseFiles { get; init; }

            public record LicenseFile
            {
                public string Title { get; init; }

                public string Filename { get; init; }
            }

        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly IWebHostEnvironment _environment;
            public Handler(IWebHostEnvironment environment)
            {
                _environment = environment;
            }

            public Task<Model> Handle(Query query, CancellationToken token)
            {
                var filePaths = Directory.GetFiles(Path.Combine(_environment.WebRootPath, "licenses"));

                var files = new List<LicenseFile>();
                foreach (var f in filePaths)
                {
                    files.Add(new LicenseFile
                    {
                        Title = Path.GetFileNameWithoutExtension(f),
                        Filename = Path.GetFileName(f)
                    });
                }

                var viewModel = new Model
                {
                    LicenseFiles = files
                };

                return Task.FromResult(viewModel);
            }
        }
    }
}
