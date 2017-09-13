// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-13.

using System;

namespace Umizoo.Infrastructure.Database.Contexts
{
    /// <summary>
    /// <see cref="IContextManager"/> 的抽象实现类
    /// </summary>
    public abstract class ContextManager : IContextManager
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected ContextManager(string contextType)
        {
            this.Id = Guid.NewGuid();
            this.ContextType = contextType;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public Guid Id
        {
            get;
            private set;
        }

        private string _contextType;
        /// <summary>
        /// 上下文类型
        /// </summary>
        protected internal string ContextType
        {
            get { return this._contextType; }
            set { this._contextType = value; }
        }

        private ICurrentContext _currentContext;
        /// <summary>
        /// 获取当前的上下文
        /// </summary>
        public ICurrentContext CurrentContext
        {
            get
            {
                if (_currentContext != null)
                    return _currentContext;

                switch (_contextType) {
                    case "web":
                        _currentContext = new WebContext(this);
                        break;
                    case "wcf":
                        _currentContext = new OperationContext(this);
                        break;
                    case "call":
                        _currentContext = new CallContext(this);
                        break;
                    case "thread":
                        _currentContext = new ThreadContext(this);
                        break;
                    default:
                        if (!string.IsNullOrEmpty(_contextType)) {
                            _currentContext = (ICurrentContext)Activator.CreateInstance(Type.GetType(_contextType), this);
                        }
                        break;
                }

                return _currentContext;
            }
        }
    }
}
