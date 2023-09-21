using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Api
{
    public class BulkSaveRequest
    {
        public List<BulkSaveRequestItem> Items { get; set; } = new();

        internal IEnumerable<BulkSaveRequestItem> Save => Items.Where(i => i.Action == "save");
        internal IEnumerable<BulkSaveRequestItem> Delete => Items.Where(i => i.Action == "delete");
        internal IEnumerable<BulkSaveRequestItem> None => Items.Where(i => i.Action == "none");

        private Dictionary<int, BulkSaveRequestItem>? _RefsLookup;
        internal Dictionary<int, BulkSaveRequestItem> RefsLookup
        {
            get
            {
                if (_RefsLookup is not null) return _RefsLookup;

                _RefsLookup = new();
                foreach (var item in Items)
                {
                    if (item.PrimaryRef is int val)
                    {
                        _RefsLookup[val] = item;
                    }
                }
                return _RefsLookup;
            }
        }
    }
}
