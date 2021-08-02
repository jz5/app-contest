using HtmlTags;
using HtmlTags.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppContest.Infrastructure.Tags
{
    public class TagConventions : HtmlConventionRegistry
    {
        public TagConventions()
        {
            Editors.Always.AddClass("form-control");

            //Editors.IfPropertyIs<DateOnly?>().ModifyWith(m => m.CurrentTag
            //    .Attr("type", "date")
            //    .Value(m.Value<DateOnly?>().HasValue ?
            //        m.Value<DateOnly?>().Value.ToString("yyyy-MM-dd") : string.Empty));

            //Editors.IfPropertyIs<DateOnly>().ModifyWith(m => m.CurrentTag
            //    .Attr("type", "date")
            //    .Value(m.Value<DateOnly>().ToString("yyyy-MM-dd")));

            //Editors.IfPropertyIs<TimeOnly?>().ModifyWith(m => m.CurrentTag
            //    .Attr("type", "time")
            //    .Value(m.Value<TimeOnly?>().HasValue ?
            //        m.Value<TimeOnly?>().Value.ToString("HH:mm") : string.Empty));

            //Editors.IfPropertyIs<TimeOnly>().ModifyWith(m => m.CurrentTag
            //    .Attr("type", "time")
            //    .Value(m.Value<TimeOnly>().ToString("HH:mm")));

            //Editors.IfPropertyIs<LocalDate>().Attr("type", "date");
            //Editors.IfPropertyIs<LocalDate?>().Attr("type", "date");
            //Editors.IfPropertyIs<LocalTime>().Attr("type", "time");
            //Editors.IfPropertyIs<LocalTime?>().Attr("type", "time");

            Editors.ModifyForAttribute<DataTypeAttribute>((t, a) =>
            {
                if (a.DataType == DataType.Url)
                    t.Attr("type", "url");
                else if (a.DataType == DataType.Date)
                    t.Attr("type", "date");
                else if (a.DataType == DataType.Time)
                    t.Attr("type", "time");
            });

            Editors.ModifyForAttribute<RequiredAttribute>(t => t.Attr("required", "required"));

            Editors.If(er => er.Accessor.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase)).BuildBy(a => new HiddenTag().Value(a.StringValue()));
            Editors.IfPropertyIs<byte[]>().ModifyWith(m => m.CurrentTag.Value(Convert.ToBase64String(m.Value<byte[]>())));
            Editors.IfPropertyIs<byte[]>().BuildBy(a => new HiddenTag());


            Editors.IfPropertyIs<bool>().ModifyWith(m => m.CurrentTag
                .Attr("type", "checkbox")
                .RemoveClass("form-control")
                .AddClass("form-check-input"));


            Labels.Always.AddClass("form-label");
            Labels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));

            Labels.ModifyForAttribute<RequiredAttribute>(t =>
            {
                t.AppendText("*");
            });

            //// Just assume a "Data." prefix for attributes.
            //Labels
            //    .Always
            //    .ModifyWith(er => er.CurrentTag.Text(er.CurrentTag.Text().Replace("Data ", "")));

        }
    }
}
