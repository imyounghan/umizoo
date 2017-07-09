

namespace Umizoo.Seeds
{
    using System;

    internal class NoneSnapshotStore : ISnapshotStore
    {

        #region ISnapshotStore 成员

        public object GetLastest(Type sourceTypeName, object sourceId)
        {
            return null;
        }

        #endregion
    }
}
