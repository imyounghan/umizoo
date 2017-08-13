using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using log4net.Config;
using Microsoft.Practices.ServiceLocation;
using Umizoo.Configurations;
using Umizoo.Infrastructure.Composition;
using Umizoo.Messaging;
using UserRegistration.Commands;
using UserRegistration.ReadModel;

namespace UserRegistration.QuickStart
{
    internal class Program
    {
        class DefaultServiceLocator : ServiceLocatorImplBase
        {
            private readonly IObjectContainer _container;

            public DefaultServiceLocator(IObjectContainer container)
            {
                _container = container;
            }

            protected override object DoGetInstance(Type serviceType, string key)
            {
                return _container.Resolve(serviceType, key);
            }

            protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
            {
                return _container.ResolveAll(serviceType);
            }
        }

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            Configuration.Create().Accept(container =>
            {
                ServiceLocator.SetLocatorProvider(() => new DefaultServiceLocator(container));
            }).EnableService().Done();


            Console.WriteLine("输入任意键演示...");
            Console.ReadKey();


            Console.WriteLine("开始创建用户...");
            var commandService = ServiceLocator.Current.GetInstance<ICommandService>();
            var commandResult = commandService.Execute(new RegisterUser
            {
                UserName = "hanyang",
                Password = "123456",
                LoginId = "young.han",
                Email = "19126332@qq.com"
            }, CommandReturnMode.EventHandled);
            Console.WriteLine("命令处理完成(结果：{0})...", commandResult.Status);

            //var commandService = ServiceGateway.Current.GetService<ICommandService>();
            //commandService.Execute(new RegisterUser {
            //    UserName = "hanyang",
            //    Password = "123456",
            //    LoginId = "young.han",
            //    Email = "19126332@qq.com"
            //});
            //int counter = 0;
            //var tasks = new System.Threading.Tasks.Task[5000];
            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //while(counter < 5000) {
            //    var userRegister = new RegisterUser {
            //        UserName = "hanyang",
            //        Password = "123456",
            //        LoginId = "young.han," + counter.ToString(),
            //        Email = "19126332@qq.com"
            //    };

            //    tasks[counter++] = commandService.ExecuteAsync(userRegister, CommandReturnType.DomainEventHandled);
            //}
            //System.Threading.Tasks.Task.WaitAll(tasks);
            //sw.Stop();
            //Console.WriteLine("用时:{0}ms", sw.ElapsedMilliseconds);
            //Console.WriteLine("成功完成的命令数量：{0}", tasks.Where(p => p.IsCompleted).Count());
            Thread.Sleep(2000);

            var queryService = ServiceLocator.Current.GetInstance<IQueryService>();

            var queryResult = queryService.Fetch<ICollection<UserModel>>(new FindAllUser());
            Console.WriteLine("共有 {0} 个用户。", queryResult.Count);

            Thread.Sleep(2000);

            var authoResult =
                queryService.Fetch<bool>(
                    new UserAuthentication {LoginId = "young.han", Password = "123456", IpAddress = "127.0.0.1"});
            if (authoResult) Console.WriteLine("登录成功。");
            else Console.WriteLine("用户名或密码错误。");

            Console.ReadKey();
        }
    }
}