using System.Collections.Generic;
using System.Linq;
using Umizoo.Messaging;
using Umizoo.Messaging.Handling;
using UserRegistration.Events;
namespace UserRegistration.ReadModel
{


    public class UserQueryHandler :
        IQueryHandler<FindAllUser, IEnumerable<UserModel>>,
        //IQueryHandler<FindAllUser, PageResult<UserModel>>,
        IQueryHandler<UserAuthentication, bool>
    {
        private readonly IUserDao dao;
        private readonly IMessageBus<IEvent> bus;

        public UserQueryHandler(IUserDao userDao, IMessageBus<IEvent> messageBus)
        {
            this.dao = userDao;
            this.bus = messageBus;
        }


        #region IQueryMultipleFetcher<FindAllData,UserModel> 成员

        public IEnumerable<UserModel> Handle(FindAllUser parameter)
        {
            return dao.GetAll().ToList();
        }
        //public PageResult<UserModel> Handle(FindAllUser parameter)
        //{
        //    return new PageResult<UserModel>(dao.GetAll());
        //}

        #endregion

        #region IQueryFetcher<UserAuthentication,bool> 成员

        public bool Handle(UserAuthentication parameter)
        {
            var user = dao.Find(parameter.LoginId);
            if(user == null)
                return false;

            if(user.Password != parameter.Password)
                return false;

            var userSigned = new UserSigned(parameter.LoginId, parameter.IpAddress);
            bus.Publish(userSigned);

            return true;
        }

        #endregion
    }
}
