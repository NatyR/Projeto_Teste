using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.API.Common.Dto
{
    public class ResponseDto<TList> where TList : class
    {
        public IList<TList> Data { get; set; }
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public long Total { get; set; }
        public int LastPage => GetTotalPage();

        public static ResponseDto<TList> Default => new ResponseDto<TList>
        {
            CurrentPage = 1,
            PerPage = 0,
            Data = new List<TList>(),
            Total = 0
        };

        private int GetTotalPage()
        {
            return PerPage > 0 ? Convert.ToInt32(Math.Ceiling((double)Total / PerPage)) : 1;
        }

        public bool Any()
        {
            return Data.Any();
        }
    }
}
