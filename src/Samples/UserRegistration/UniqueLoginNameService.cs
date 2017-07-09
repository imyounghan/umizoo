using System;
using System.Collections.Concurrent;
using Umizoo;

namespace UserRegistration
{

    [Register(typeof(IUniqueLoginNameService))]
    public class UniqueLoginNameService : IUniqueLoginNameService
    {
        private readonly ConcurrentDictionary<string, string> dict = new ConcurrentDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);


        public bool Validate(string loginName, string correlationId)
        {
            string commandId;
            if(!dict.TryGetValue(loginName, out commandId)) {
                return dict.TryAdd(loginName, correlationId);
            }
            
            return correlationId == commandId;
        }
    }
}
