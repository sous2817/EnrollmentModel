using System.ComponentModel;

namespace Semio.ClientService.Data.Events
{
    /// <summary>
    /// Event Arguments class for ItemPropertyChanged event
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        private object _item;

        /// <summary>
        /// The child item that changed.
        /// </summary>
        public object Item => _item;

        /// <summary>
        /// Initializes a new instance of the ItemPropertyChangedEventArgs class.
        /// </summary>
        /// <param name="item">The child item that changed.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(object item, string propertyName) : base(propertyName)
        {
            _item = item;
        }

    }

    /// <summary>
    /// Represents the method that handles the ItemPropertyChanged event
    /// </summary>
    /// <param name="sender">The object that raised the event</param>
    /// <param name="args">Information about the event</param>
    public delegate void ItemPropertyChangedEventHandler(object sender, ItemPropertyChangedEventArgs args);

}
