// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Linq;

using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Caching;
using Umizoo.Infrastructure.Database;
using Umizoo.Infrastructure.Logging;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    public sealed class Repository : IRepository
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IMessageBus<IEvent> _eventBus;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IAggregateStorage _storage;

        public Repository(IMessageBus<IEvent> eventBus,
            IAggregateStorage storage,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            ICacheProvider cacheProvider)
        {
            _eventBus = eventBus;
            _storage = storage;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _cacheProvider = cacheProvider;
        }

        //public void Delete(IAggregateRoot aggregateRoot)
        //{
        //    Assertions.NotNull(aggregateRoot, "aggregateRoot");

        //    _storage.Delete(aggregateRoot);

        //    //var sourceInfo = new SourceInfo(aggregateRoot.Id, aggregateRoot.GetType());
        //    //_eventStore.
        //}



        public IAggregateRoot Find(Type aggregateRootType, object aggregateRootId)
        {
            Assertions.NotNull(aggregateRootType, "aggregateRootType");
            Assertions.NotNull(aggregateRootId, "aggregateRootId");

            if (!aggregateRootType.IsClass || aggregateRootType.IsAbstract || typeof(IAggregateRoot).IsAssignableFrom(aggregateRootType))
                throw new ArgumentException("the type is not a AggregateRoot.", "aggregateRootType");
            
            var cacheRegion = GetCacheRegion(aggregateRootType);
            var cacheKey = GetCacheKey(aggregateRootType, aggregateRootId.ToString());

            var aggregateRoot =_cacheProvider.GetCache(cacheRegion).Get(cacheKey) as IAggregateRoot;
            if (!aggregateRoot.IsNull() && aggregateRoot.GetType() != aggregateRootType)
            {
                aggregateRoot = null;
            }

            if (!aggregateRoot.IsNull())
            {
                if (LogManager.Default.IsDebugEnabled)
                    LogManager.Default.DebugFormat("Find the aggregate root {0} of id {1} from cache.",
                        aggregateRootType.FullName, aggregateRootId);

                return aggregateRoot;
            }

            if (typeof(IEventSourced).IsAssignableFrom(aggregateRootType))
                try
                {
                    aggregateRoot = Restore(aggregateRootType, aggregateRootId.ToString());
                }
                catch (Exception ex)
                {
                    LogManager.Default.Warn(ex,
                        "Failed to restore aggregate root from events. aggregateRootType:{0},aggregateRootId:{1}.",
                        aggregateRootType.FullName, aggregateRoot.Id);
                    aggregateRoot = null;
                }

            if (aggregateRoot.IsNull())
                aggregateRoot = _storage.Find(aggregateRootType, aggregateRootId);

            if (!aggregateRoot.IsNull())
                try
                {
                    _cacheProvider.GetCache(cacheRegion).Put(cacheKey, aggregateRoot);
                }
                catch (Exception ex)
                {
                    LogManager.Default.Warn(ex,
                        "Failed to refresh aggregate root to memory cache. aggregateRootType:{0},aggregateRootId:{1}.",
                        aggregateRootType.FullName, aggregateRoot.Id);
                }

            return aggregateRoot;
        }

        public void Save(IAggregateRoot aggregateRoot)
        {
            Assertions.NotNull(aggregateRoot, "aggregateRoot");

            _storage.Save(aggregateRoot);

            var aggregateRootType = aggregateRoot.GetType();
            try
            {
                _cacheProvider.GetCache(GetCacheRegion(aggregateRootType)).Put(GetCacheKey(aggregateRootType, aggregateRoot.Id), aggregateRoot);
            }
            catch (Exception ex)
            {
                LogManager.Default.Warn(ex,
                    "Failed to refresh aggregate root to memory cache. aggregateRootType:{0},aggregateRootId:{1}.",
                    aggregateRootType.FullName, aggregateRoot.Id);
            }

            var eventPublisher = aggregateRoot as IEventPublisher;
            if (eventPublisher == null) return;

            var sourceInfo = new SourceInfo(aggregateRoot.Id, aggregateRootType);
            var envelopes = eventPublisher.GetEvents().Select(@event =>
            {
                var envelope = new Envelope<IEvent>(@event, ObjectId.GenerateNewId().ToString());
                envelope.Items[StandardMetadata.SourceInfo] = sourceInfo;
                return envelope;
            }).ToArray();


            _eventBus.Send(envelopes);
        }

        public void Save(IEventSourced eventSourced, Envelope<ICommand> command)
        {
            Assertions.NotNull(eventSourced, "eventSourced");
            Assertions.NotNull(command, "command");

            var eventSourcedType = eventSourced.GetType();
            var sourceInfo = new SourceInfo(eventSourced.Id, eventSourcedType);
            var changedEvents = eventSourced.GetChanges();

            var saved = false;

            try
            {
                if (_eventStore.Save(sourceInfo, changedEvents, command.MessageId))
                {
                    if (LogManager.Default.IsDebugEnabled)
                        LogManager.Default.DebugFormat(
                            "Persistent domain events successfully. aggregateRootType:{0},aggregateRootId:{1},version:{2}~{3}.",
                            eventSourcedType.FullName, eventSourced.Id, changedEvents.Min(p=>p.Version), changedEvents.Max(p => p.Version));
                    saved = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "Persistent domain events failed. aggregateRootType:{0},aggregateRootId:{1},version:{2}~{3}.",
                    eventSourcedType.FullName, eventSourced.Id, changedEvents.Min(p => p.Version), changedEvents.Max(p => p.Version));
                throw ex;
            }

            if (!saved)
            {
                //由该命令产生的事件如果已保存，则取出之前的事件重新发送
                changedEvents = _eventStore.Find(sourceInfo, command.MessageId);

                _eventBus.Publish(sourceInfo, changedEvents, command);
                return;
            }

            try
            {
                _cacheProvider.GetCache(GetCacheRegion(eventSourcedType)).Put(GetCacheKey(eventSourcedType, eventSourced.Id), eventSourced);
            }
            catch (Exception ex)
            {
                LogManager.Default.Warn(ex,
                    "Failed to refresh aggregate root to memory cache. aggregateRootType:{0},aggregateRootId:{1}.",
                    eventSourcedType.FullName, eventSourced.Id);
            }

            _eventBus.Publish(sourceInfo, changedEvents, command);
        }

        private IEventSourced Create(Type sourceType, object sourceId)
        {
            var idType = sourceId.GetType();
            var constructor = sourceType.GetConstructor(new[] {idType});

            if (constructor == null)
            {
                var errorMessage =
                    string.Format("Type '{0}' must have a constructor with the following signature: .ctor({1} id)",
                        sourceType.FullName, idType.FullName);
                throw new ApplicationException(errorMessage);
                //return FormatterServices.GetUninitializedObject(aggregateRootType) as IEventSourced;
            }

            return constructor.Invoke(new[] {sourceId}) as IEventSourced;
        }

        private IEventSourced Restore(Type sourceType, string sourceId)
        {
            IEventSourced eventSourced;
            try
            {
                eventSourced = _snapshotStore.GetLatest(sourceType, sourceId) as IEventSourced;
                if (!eventSourced.IsNull())
                    if (LogManager.Default.IsDebugEnabled)
                        LogManager.Default.DebugFormat(
                            "Find the aggregate root '{0}' of id '{1}' from snapshot. current version:{2}.",
                            sourceType.FullName, sourceId, eventSourced.Version);
            }
            catch (Exception ex)
            {
                LogManager.Default.Warn(ex,
                    "Get the latest snapshot failed. aggregateRootId:{0},aggregateRootType:{1}.",
                    sourceId, sourceType.FullName);
                eventSourced = null;
            }

            var events = _eventStore.FindAll(new SourceInfo(sourceId, sourceType), eventSourced.Version);
            if (!events.IsEmpty())
            {
                if (eventSourced.IsNull()) eventSourced = Create(sourceType, sourceId);

                eventSourced.LoadFrom(events);

                if (LogManager.Default.IsDebugEnabled)
                    LogManager.Default.DebugFormat(
                        "Restore the aggregate root '{0}' of id '{1}' from event stream. version:{2} ~ {3}",
                        sourceType.FullName,
                        sourceId,
                        events.Min(p => p.Version),
                        events.Max(p => p.Version));
            }

            return eventSourced;
        }

        private static string GetCacheRegion(Type type)
        {
            var attr = type.GetSingleAttribute<CacheRegionAttribute>(false);
            if (attr.IsNull())
                return CacheRegionAttribute.DefaultRegion;

            return attr.RegionName;
        }

        private static string GetCacheKey(Type type, string id)
        {
            return string.Format("Model:{0}:{1}", type.FullName, id);
        }
    }

    /// <summary>
    /// 仓储接口实现
    /// </summary>
    /// <typeparam name="TAggregateRoot">聚合类型</typeparam>
    public class Repository<TAggregateRoot> : IRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ICache _cache;

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public Repository(IDbContextFactory dbContextFactory, ICacheProvider cacheProvider)
        {
            this._dbContextFactory = dbContextFactory;
            this._cache = cacheProvider.GetCache(GetCacheRegion(typeof(TAggregateRoot)));
        }

        /// <summary>
        /// 数据上下文
        /// </summary>
        protected IDbContext DbContext { get { return _dbContextFactory.GetCurrent(); } }
        /// <summary>
        /// 缓存程序
        /// </summary>
        protected ICache Cache { get { return this._cache; } }

        private static string GetCacheRegion(Type type)
        {
            var attr = type.GetSingleAttribute<CacheRegionAttribute>(false);
            if (attr.IsNull())
                return CacheRegionAttribute.DefaultRegion;

            return attr.RegionName;
        }

        private static string GetCacheKey(Type type, object id)
        {
            return string.Format("Model:{0}:{1}", type.FullName, id);
        }

        /// <summary>
        /// 添加聚合根到仓储
        /// </summary>
        public virtual void Add(TAggregateRoot aggregateRoot)
        {
            DbContext.Insert(aggregateRoot);

        }
        void IRepository<TAggregateRoot>.Add(TAggregateRoot aggregateRoot)
        {
            this.Add(aggregateRoot);

            DbContext.DataCommitted += (sender, args) => {
                _cache.Put(GetCacheKey(typeof(TAggregateRoot), aggregateRoot.Id), aggregateRoot);
            };

            LogManager.Default.DebugFormat("the aggregate root {0} of id {1} is added the dbcontext.",
                    typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }
        /// <summary>
        /// 更新聚合到仓储
        /// </summary>
        public virtual void Update(TAggregateRoot aggregateRoot)
        {
            DbContext.Update(aggregateRoot);

        }
        void IRepository<TAggregateRoot>.Update(TAggregateRoot aggregateRoot)
        {
            this.Update(aggregateRoot);

            DbContext.DataCommitted += (sender, args) => {
                _cache.Put(GetCacheKey(typeof(TAggregateRoot), aggregateRoot.Id), aggregateRoot);
            };

            LogManager.Default.DebugFormat("the aggregate root {0} of id {1} is updated the dbcontext.",
                    typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }
        /// <summary>
        /// 从仓储中移除聚合
        /// </summary>
        public virtual void Remove(TAggregateRoot aggregateRoot)
        {
            DbContext.Delete(aggregateRoot);
        }
        void IRepository<TAggregateRoot>.Remove(TAggregateRoot aggregateRoot)
        {
            this.Remove(aggregateRoot);

            DbContext.DataCommitted += (sender, args) => {
                _cache.Remove(GetCacheKey(typeof(TAggregateRoot), aggregateRoot.Id));
            };

            LogManager.Default.DebugFormat("updated the aggregate root {0} of id {1} in dbcontext.",
                   typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }


        ///// <summary>
        ///// 根据标识id获得实体
        ///// </summary>
        //public TAggregateRoot Get<TIdentify>(TIdentify id)
        //{
        //    var aggregate = (this as IRepository<TAggregateRoot>).Find(id);
        //    if (aggregate == null)
        //        throw new EntityNotFoundException(id, typeof(TAggregateRoot));

        //    return aggregate;
        //}

        /// <summary>
        /// 根据标识id获取聚合实例，如未找到则返回null
        /// </summary>
        public virtual TAggregateRoot Find<TIdentify>(TIdentify id)
        {
            return DbContext.Get<TAggregateRoot>(id);
        }
        TAggregateRoot IRepository<TAggregateRoot>.Find<TIdentify>(TIdentify id)
        {
            var cacheKey = GetCacheKey(typeof(TAggregateRoot), id);
            var aggregate = (TAggregateRoot)_cache.Get(cacheKey);
            if (aggregate == null) {
                aggregate = this.Find(id);
                LogManager.Default.DebugFormat("find the aggregate root {0} of id {1} from storage.",
                    typeof(TAggregateRoot).FullName, id.ToString());
                if (aggregate != null) {
                    _cache.Put(cacheKey, aggregate);
                }
            }
            else {
                LogManager.Default.DebugFormat("find the aggregate root {0} of id {1} from cache.",
                    typeof(TAggregateRoot).FullName, id.ToString());
            }

            return aggregate;
        }
    }
}