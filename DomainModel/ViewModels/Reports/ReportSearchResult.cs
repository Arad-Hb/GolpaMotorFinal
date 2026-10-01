using Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.Reports
{
    public class ReportSearchResult<T>
    {
        public OperationResult Operation { get; set; }

        public T? Data { get; set; }

        public ReportSearchResult(string operationName)
        {
            Operation = new OperationResult(operationName);
        }
    }
}
