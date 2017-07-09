using Umizoo.Infrastructure;

namespace UserRegistration
{
    public class UserRegisterService
    {
        private readonly IUniqueLoginNameService _uniqueService;
        private readonly string _commandId;
        public UserRegisterService(IUniqueLoginNameService uniqueService, string commandId)
        {
            this._uniqueService = uniqueService;
            this._commandId = commandId;
        }


        public User Register(string loginId, string password, string userName, string email)
        {
            if (!_uniqueService.Validate(loginId, _commandId)) {
                throw new UnrecoverableException("用户名已存在！");
            }

            return new User(loginId, password, userName, email);
        }
    }
}
