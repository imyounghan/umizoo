

namespace UserRegistration.Application
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Registration;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Umizoo;
    using Umizoo.Messaging;

    using UserRegistration.Commands;
    using UserRegistration.ReadModel;

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new RegistrationBuilder();
            builder.ForType<WcfClient>().Export<ICommandService>().SetCreationPolicy(CreationPolicy.Shared);
            builder.ForType<WcfClient>().Export<IQueryService>().SetCreationPolicy(CreationPolicy.Shared);

            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.Load("Umizoo"), builder));
            //catelog.Catalogs.Add(new DirectoryCatalog(Directory.GetCurrentDirectory()));//查找部件，当前应用程序
           var container = new CompositionContainer(catalog);
            container.ComposeParts();

            Console.WriteLine("输入任意键开始演示...");
            Console.ReadKey();

            Console.WriteLine("开始创建用户...");
            var commandService = container.GetExportedValue<ICommandService>();
            var commandResult = commandService.ExecuteAsync(new RegisterUser {
                UserName = "hanyang",
                Password = "123456",
                LoginId = "young.han",
                Email = "19126332@qq.com"
            }, CommandReturnMode.EventHandled).Result;
            Console.WriteLine("{0}:{1}", commandResult.Status, commandResult.ErrorMessage.IfEmpty("null"));
            Thread.Sleep(2000);

            var queryService = container.GetExportedValue<IQueryService>();
            var queryResult = queryService.Fetch<ICollection<UserModel>>(new FindAllUser());
            Console.WriteLine("共有 {0} 个用户。", queryResult.Count());
            Thread.Sleep(2000);

            var authoResult =
                queryService.Fetch<bool>(
                    new UserAuthentication() { LoginId = "young.han", Password = "123456", IpAddress = "127.0.0.1" });
            if(authoResult) {
                Console.WriteLine("登录成功。");
            }
            else {
                Console.WriteLine("用户名或密码错误。");
            }

            Console.ReadKey();
        }
    }
}
