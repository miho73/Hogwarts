using System;
using System.Collections.Generic;
using System.Text;

namespace Hogwarts.Module
{
    public class TimeFormat
    {
        public static string SpanToProper(TimeSpan span)
        {
            if(span.Days == 0)
            {
                if(span.Hours == 0)
                {
                    if(span.Minutes == 0)
                    {
                        return span.Seconds + "초";
                    }
                    else
                    {
                        return span.Minutes + "분 " + span.Seconds + "초";
                    }
                }
                else
                {
                    return span.Hours + "시간 " + span.Minutes + "분 " + span.Seconds + "초";
                }
            }
            else
            {
                return span.Days + "일 " + span.Hours + "시간 " + span.Minutes + "분 " + span.Seconds + "초";
            }
        }
    }
}
