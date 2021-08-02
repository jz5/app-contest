using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace AppContest.Models
{
    public class Contest
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "説明")]
        public string Description { get; set; }

        [Display(Name = "開始日時")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [Required]
        [Display(Name = "終了日時")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "主催")]
        public string Host { get; set; }

        [Display(Name = "応募資格")]
        public string Requirements { get; set; }

        [Required]
        [Display(Name = "URL")]
        [DataType(DataType.Url)]
        public string Url { get; set; }

        [Display(Name = "備考")]
        public string Note { get; set; }

        [Display(Name = "非表示")]
        public bool IsHidden { get; set; }

        public DateTime LastUpdatedDateTime { get; set; }
        public virtual IdentityUser LastUpdatedBy { get; set; }
        public DateTime CreationDateTime { get; set; }
        public virtual IdentityUser CreatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
