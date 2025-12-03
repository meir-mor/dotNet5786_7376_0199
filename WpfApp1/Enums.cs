using System.Collections;

namespace PL
{
    /// <summary>
    /// Item used to populate the ComboBox for shipment-type filtering.
    /// Value == null represents "All".
    /// </summary>
    internal sealed class ShipmentTypeItem
    {
        /// <summary>
        /// The ShipmentType value (nullable). Null = All.
        /// </summary>
        public BO.ShipmentType? Value { get; init; }

        /// <summary>
        /// Display text shown in the ComboBox.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <inheritdoc/>
        public override string ToString() => Name;
    }

    /// <summary>
    /// Enumerable collection that exposes ShipmentType items (first item = All).
    /// Intended to be used as a StaticResource in XAML for ComboBox ItemsSource.
    /// </summary>
    internal class ShipmentTypeItemsCollection : IEnumerable
    {
        private static readonly IEnumerable<ShipmentTypeItem> s_items = BuildItems();

        private static IEnumerable<ShipmentTypeItem> BuildItems()
        {
            var list = new List<ShipmentTypeItem>
            {
                new ShipmentTypeItem { Value = null, Name = "All" }
            };

            foreach (BO.ShipmentType st in Enum.GetValues(typeof(BO.ShipmentType)))
            {
                list.Add(new ShipmentTypeItem { Value = st, Name = st.ToString() });
            }

            return list;
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => s_items.GetEnumerator();
    }
}