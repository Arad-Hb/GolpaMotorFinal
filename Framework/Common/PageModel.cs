using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common
{
    public class PageModel
    {
        public int PageIndex { get; set; }
        private int pageSize;
        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
            set
            {
                if (value <= 0)
                {
                    value = 10;
                }
                this.pageSize = value;
            }

        }

        private int recordCount;
        public int RecordCount
        {
            get { return this.recordCount; }

            set { this.recordCount = value; }
        }
        public int PageCount
        {
            get
            {
                if (PageSize == 0) this.pageSize = 10;

                if (RecordCount % pageSize == 0)
                {
                    return RecordCount / pageSize;
                }
                else
                {
                    return RecordCount / pageSize + 1;
                }
            }
        }
    }
}
