using System;
using System.Collections.Generic;
using System.Text;

namespace Hogwarts.Module
{
    public class StudentCode
    {
        public static string GetStudentCode(ulong id)
        {
            long idx = Convert.ToInt64(id);
            return Convert.ToString(idx, 16);
        }
    }
}
