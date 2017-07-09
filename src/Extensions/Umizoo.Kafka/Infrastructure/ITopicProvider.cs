
namespace Umizoo.Infrastructure
{
    using System;

    public interface ITopicProvider
    {
        string GetTopic(Type type);
    }
}
