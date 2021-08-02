using HtmlTags;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppContest.Infrastructure
{
    public static class HtmlHelperExtensions
    {

        public static HtmlTag ValidationDiv(this IHtmlHelper helper)
        {
            var outerDiv = new HtmlTag("div")
                .Id("validationSummary")
                .AddClass("validation-summary-valid text-danger")
                .Data("valmsg-summary", true);

            var ul = new HtmlTag("ul");
            ul.Add("li", li => li.Style("display", "none"));

            outerDiv.Children.Add(ul);

            return outerDiv;
        }

        public static HtmlTag FormBlock<T, TMember>(this IHtmlHelper<T> helper,
            Expression<Func<T, TMember>> expression,
            Action<HtmlTag> labelModifier = null,
            Action<HtmlTag> inputModifier = null,
            string note = null
        ) where T : class
        {
            labelModifier ??= _ => { };
            inputModifier ??= _ => { };

            var divTag = new HtmlTag("div");
            divTag.AddClass("mb-3 row form-group");

            var labelTag = helper.Label(expression);
            labelTag.AddClass("col-sm-2 col-form-label");
            labelModifier(labelTag);

            var inputTag = helper.Input(expression);
            inputModifier(inputTag);

            var divInputTag = new HtmlTag("div");
            divInputTag.AddClass("col-sm-10");

            divInputTag.Append(inputTag);
            if (note != null)
            {
                var div = new HtmlTag("div");
                div.AddClass("form-text small");
                div.Text(note);

                divInputTag.Append(div);
            }

            divTag.Append(labelTag);
            divTag.Append(divInputTag);

            return divTag;
        }
    }
}
