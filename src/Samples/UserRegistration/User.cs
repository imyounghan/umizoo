using System;
using Umizoo.Seeds;
using UserRegistration.Events;

namespace UserRegistration
{

    [Serializable]
    public class User : EventSourced<Guid>
    {
        public User(Guid id)
            : base(id)
        { }

        public User(string loginId, string password, string userName, string email)
            : this(Guid.NewGuid())
        {
            var userCreated = new UserCreated(loginId, password, userName, email);

            RaiseEvent(userCreated);
        }

        public string LoginId { get; private set; }

        public string Password { get; private set; }

        public string UserName { get; private set; }

        public string Email { get; private set; }

        public bool VertifyPassword(string password)
        {
            return this.Password == password;
        }

        //private void OnUserCreated(UserCreated @event)
        //{
        //    this.LoginId = @event.LoginId;
        //    this.Password = @event.Password;
        //    this.UserName = @event.UserName;
        //    this.Email = @event.Email;
        //}
        private void Handle(UserCreated @event)
        {
            this.LoginId = @event.LoginId;
            this.Password = @event.Password;
            this.UserName = @event.UserName;
            this.Email = @event.Email;
        }
    }
}
