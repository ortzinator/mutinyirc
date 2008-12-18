using System.AddIn.Contract;
using System.Collections.Generic;

namespace OrtzIRC.Contracts
{
    [System.AddIn.Pipeline.AddInContract()]
    public interface ICommandContract : IContract
    {
        bool Excecute(IListContract<string> parameters);
    }
}
