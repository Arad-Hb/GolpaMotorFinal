using Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Services
{
    public interface IBaseRepository<TModel, TKey, TListItem>
    {
       Task<OperationResult> Add(TModel model);               
       Task<OperationResult> Delete(TKey ID);                 
       Task<OperationResult> Update(TModel model);            
       Task<TModel> Get(TKey ID);                             
       //Task<List<TListItem>> GetAll();                        
    }
}
