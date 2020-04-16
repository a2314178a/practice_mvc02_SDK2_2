using System;
using System.Collections.Generic;

namespace practice_mvc02.Models
{
    public class cusResult<T> where T : class
    {
        //public IList<errorMsg> Error { set; get; } = new List<errorMsg>();
        public errorMsg Error {set; get;}
        public int Code { set; get; }
        public T Result { set; get; }
        public string StatusText {set; get;}
    }

    public class errorMsg
    {
        public string Message { set; get; }
        public string Field { set; get; }
    }





}