using System;

namespace AppContest.Models
{
    public static class ContestHelperExtensions
    {
        public static string ToFriendlyDate(this (DateTime? StartDate, DateTime EndDate) contest)
        {
            var ci = new System.Globalization.CultureInfo("ja-JP");

            if (!contest.StartDate.HasValue)
            {
                // 応募期限
                if (contest.EndDate.Hour == 0 && contest.EndDate.Minute == 0)
                    // 日付のみ
                    return contest.EndDate.ToString("yyyy/M/d (ddd)", ci);
                else
                    // 時間あり
                    return contest.EndDate.ToString("yyyy/M/d (ddd) H:mm", ci);
            }
            else if (contest.StartDate.HasValue)
            {
                // 応募期間
                string val;

                // 開始
                if (contest.StartDate.Value.Hour == 0 && contest.StartDate.Value.Minute == 0)
                    // 日付のみ
                    val = contest.StartDate.Value.ToString("yyyy/M/d (ddd)", ci);
                else
                    // 時間あり
                    val = contest.StartDate.Value.ToString("yyyy/M/d (ddd) H:mm", ci);

                val += "～";

                // 終了
                if (contest.StartDate.Value.Year != contest.EndDate.Year)
                {
                    // 年をまたぐ

                    if (contest.EndDate.Hour == 0 && contest.EndDate.Minute == 0)
                        // 日付のみ
                        val += contest.EndDate.ToString("yyyy/M/d (ddd)", ci);
                    else
                        // 時間あり
                        val += contest.EndDate.ToString("yyyy/M/d (ddd) H:mm", ci);
                }
                else
                    // 年が同じ

                    if (contest.EndDate.Hour == 0 && contest.EndDate.Minute == 0)
                    // 日付のみ
                    val += contest.EndDate.ToString("M/d (ddd)", ci);
                else
                    // 時間あり
                    val += contest.EndDate.ToString("M/d (ddd) H:mm", ci);

                return val;
            }

            return null;
        }

        public static string ToFriendlyDateTitle(this (DateTime? StartDate, DateTime EndDate) contest)
        {
            if (contest.StartDate.HasValue)
                return "応募期間";
            else if (!contest.StartDate.HasValue)
                return "応募期限";

            return null;
        }

    }
}
