using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using JetBrains.Annotations;

namespace Semio.ClientService.Data
{
	/// <summary>
	/// Provides a base implementation of INotifyPropertyChanged including a debug helper
	/// to identify invalid change notifications which are usually caused by changes to 
	/// property names
	/// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public abstract class Bindable : INotifyPropertyChanged, IDisposable
	{
		/// <summary>
		/// The property name parameter to be used when firing an INotifyPropertyChanged 
		/// change notification for a "this[]" property.
		/// </summary>
		protected const string IndexerNotificationName = "Item[]";
        /// <summary>
        /// The property name of the "this[]" property.
        /// </summary>
		protected const string IndexerPropertyName = "Item";

        private readonly int _showDebugLevel;
        /// <summary>
        /// Available to all classes derived from Bindable
        /// 
        /// Gets the ShowDebugLevel where the following is assumed:
        ///   0 = None, 
        ///   5 = low / no priority,
        ///   10 = Medium,
        ///   15 = High
        /// </summary>
        /// <value>The show debug level.</value>
        protected int DebugLevel => _showDebugLevel;


        /// <summary>
        /// Initializes a new instance of the <see cref="Bindable"/> class.
        /// </summary>
        protected Bindable()
        {
            // If developer has a ShowDebugLevel show only messages
            // above or equal to that level
            try
            {
                object level = Environment.GetEnvironmentVariable("ShowDebugLevel");
                if (level != null)
                {
                    if (!Int32.TryParse(level.ToString(), out _showDebugLevel))
                        Debug.Print(String.Format("Failed to parse ShowDebugLevel from {0}", level));
                } 
                else
                {
                    _showDebugLevel = 10;
                }
            }
            catch (ArgumentNullException) { }//Do nothing... 
            catch (SecurityException) { }//Do nothing... 
        }

	    /// <summary>
	    /// Fires change notification for the specified property.
	    /// Prefer NotifyPropertyChanged for direct property notifications.
	    /// </summary>
	    /// <param name="propertyName">Name of the property.</param>
	    /// <remarks>
	    /// Override <see cref="PropertyChangeNotifying"/> for additional processing during property changes.
	    /// </remarks>
	    [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged(string propertyName)
	    {
	        NotifyPropertyChanged(propertyName, false);
	    }

        /// <summary>
        /// Fires change notification for the specified property.
        /// Prefer NotifyPropertyChanged for direct property notifications.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="stringRequired">Flag indicating debug checks for expression override preference should be skipped.</param>
        /// <remarks>
        /// Override <see cref="PropertyChangeNotifying"/> for additional processing during property changes.
        /// </remarks>
        protected void NotifyPropertyChanged(string propertyName, bool stringRequired)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("propertyName is null or empty.", nameof(propertyName));

            CheckCallerForExpressionSuitability(propertyName, stringRequired);
            ValidatePropertyExists(propertyName);

            NotifyPropertyChangedInternal(propertyName);
        }

        /// <summary>
        /// Fires a change notification for the specified property.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        /// <remarks>
        /// Override <see cref="PropertyChangeNotifying"/> for additional processing during property changes.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Cannot change the signature")]
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            
            NotifyPropertyChangedInternal(propertyName);
        }

        private void NotifyPropertyChangedInternal(string propertyName)
        {
            if (!PropertyChangeNotifying(propertyName))
                return;

            var handler = _propertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));

            PropertyChangeNotified(propertyName);
        }

        protected virtual bool PropertyChangeNotifying(string propertyName) => true;

        protected virtual void PropertyChangeNotified(string propertyName)
        {
        }

        /// <summary>
        /// Fires a change notification for the entire object.
        /// </summary>
        protected virtual void NotifyObjectChanged()
        {
            var handler = _propertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(String.Empty));
        }

        [Conditional("DEBUG")]
        protected void ValidatePropertyExists(string propertyName)
        {
            Debug.Assert(PropertyExists(propertyName), String.Format(CultureInfo.CurrentCulture, "Invalid property change notification for {0} on {1}", propertyName, GetType().FullName));
        }

	    protected bool PropertyExists(string propertyName)
	    {
	        var type = GetType();

	        if (propertyName != IndexerNotificationName) 
                return type.GetProperty(propertyName) != null;

	        var property = type.GetProperty(IndexerPropertyName);
	        return property != null && property.GetIndexParameters().Length > 0;
	    }

	    [Conditional("DEBUG")]
        private void CheckCallerForExpressionSuitability(string propertyName, bool skip)
        {
            if (skip)
                return;

            Debug.Fail("Use NotifyPropertyChanged with Expression parameter for compiler checked notifications. If a string parameter is required, use the (string, bool) overload instead to skip this check.");
        }

		/// <summary>
		/// Unsubscribes all change notifications from this object. This should only be called when the object will not be used or changed later.
		/// </summary>
		public void UnsubscribeAllNotifications()
		{
			if (_propertyChanged == null)
				return;

			Delegate[] invocationList = _propertyChanged.GetInvocationList();
			if (invocationList.Length == 0)
				return;

			foreach (Delegate attached in invocationList)
			{
                if (_showDebugLevel < 5)
                    Debug.Print(String.Format("Detaching: {0};{1}", attached.Target, attached.Method.Name));
			}

			_propertyChanged = null;
		}

        #region Change Notification Relaying

	    private readonly Dictionary<INotifyPropertyChanged, Dictionary<string, List<string>>> _registeredWrappedRelays = new Dictionary<INotifyPropertyChanged, Dictionary<string, List<string>>>();

        /// <summary>
        /// Set up an automatic change notification to trigger based on a property change from another object.
        /// </summary>
        /// <param name="property">The name of the property for which to trigger change notification.</param>
        /// <param name="sourceObject">The object to monitor for originating changes.</param>
        /// <param name="sourceProperties">A set of property names to use as triggers from the source object.</param>
        protected void RegisterWrappedPropertyNotificationRelay(string property, object sourceObject, IEnumerable<string> sourceProperties)
	    {
	        foreach (string source in sourceProperties)
	        {
	            RegisterWrappedPropertyNotificationRelay(property, sourceObject, source);
	        }
	    }

        /// <summary>
        /// Set up an automatic change notification to trigger based on a property change from another object.
        /// </summary>
        /// <param name="property">The name of the property for which to trigger change notification.</param>
        /// <param name="source">The object to monitor for originating changes.</param>
        /// <param name="sourceProperty">An optional alternate property name to use as triggers from the source object.</param>
	    protected void RegisterWrappedPropertyNotificationRelay(string property, object source, string sourceProperty = null)
	    {
	        INotifyPropertyChanged sourceObject = source as INotifyPropertyChanged;
	        if (sourceObject == null)
	            return;

	        string propertyFromSource = sourceProperty ?? property;

	        if (!_registeredWrappedRelays.ContainsKey(sourceObject))
	        {
	            _registeredWrappedRelays.Add(sourceObject, new Dictionary<string, List<string>>());
	            sourceObject.PropertyChanged += RelayObjectPropertyChanged;
	        }

	        if (!_registeredWrappedRelays[sourceObject].ContainsKey(propertyFromSource))
	        {
	            _registeredWrappedRelays[sourceObject].Add(propertyFromSource, new List<string>());
	        }

	        if (!_registeredWrappedRelays[sourceObject][propertyFromSource].Contains(property))
	        {
	            _registeredWrappedRelays[sourceObject][propertyFromSource].Add(property);
	        }
	    }

        /// <summary>
        /// Remove an automatic change notification relay for a specific object instance.
        /// </summary>
        /// <param name="source">The object being monitored for originating changes to disconnect.</param>
        /// <param name="property">Optionally the name of a specific property for which to disconnect triggering.</param>
        /// <param name="sourceProperty">An optional alternate property name to remove as a trigger from the source object.</param>
        /// <returns></returns>
	    protected Dictionary<string, List<string>> UnregisterWrappedPropertyNotificationRelay(object source, string property = null, string sourceProperty = null)
	    {
	        Dictionary<string, List<string>> removed = null;
	        INotifyPropertyChanged sourceObject = source as INotifyPropertyChanged;
	        if (sourceObject == null || !_registeredWrappedRelays.ContainsKey(sourceObject))
	            return removed;

	        string propertyFromSource = sourceProperty ?? property;

	        if (property == null && propertyFromSource == null)
	        {
	            removed = _registeredWrappedRelays[sourceObject].ToDictionary(k => k.Key, k => k.Value);
	            _registeredWrappedRelays[sourceObject].Clear();
	        }

	        if (property != null && _registeredWrappedRelays[sourceObject].ContainsKey(propertyFromSource))
	        {
	            _registeredWrappedRelays[sourceObject][propertyFromSource].Remove(property);
	            removed = new Dictionary<string, List<string>> { { propertyFromSource, new List<string> { property } } };
	        }

	        if (propertyFromSource != null && (property == null || !_registeredWrappedRelays[sourceObject][propertyFromSource].Any()))
	        {
	            _registeredWrappedRelays[sourceObject].Remove(propertyFromSource);
	        }

	        if (!_registeredWrappedRelays[sourceObject].Any())
	        {
	            _registeredWrappedRelays.Remove(sourceObject);
	            sourceObject.PropertyChanged -= RelayObjectPropertyChanged;
	        }

	        return removed;
	    }

        /// <summary>
        /// Add a set of relaying notifications onto a source object instance.
        /// </summary>
        /// <param name="source">The object to monitor for originating changes.</param>
        /// <param name="relays">A set of relaying notifications previously removed from an object.</param>
	    protected void RestoreWrappedPropertyRegistrations(object source, Dictionary<string, List<string>> relays)
        {
            if (relays == null || !relays.Any())
                return;

            foreach (var kvp in relays)
            {
                foreach (string prop in kvp.Value)
                {
                    RegisterWrappedPropertyNotificationRelay(prop, source, kvp.Key);
                }
            }

            // send all notifications from new object
            foreach (string prop in relays.Values.SelectMany(_ => _).Distinct())
            {
                NotifyPropertyChanged(prop, true);
            }
        }

	    private void RelayObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var source = sender as INotifyPropertyChanged;
            if (source == null || !_registeredWrappedRelays.ContainsKey(source))
                return;

            if (!_registeredWrappedRelays[source].ContainsKey(e.PropertyName))
                return;

            foreach (string target in _registeredWrappedRelays[source][e.PropertyName])
            {
                NotifyPropertyChanged(target, true);
            }
        }
        #endregion

		#region INotifyPropertyChanged Members
		private PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
		public event PropertyChangedEventHandler PropertyChanged
		{
			add { _propertyChanged += value; }
			remove { _propertyChanged -= value; }
		}

		#endregion

		#region IDisposable Dispose pattern
		private bool _disposed;

		/// <summary>
		/// Releases unmanaged and managed resources
		/// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
                // If Disposed then return
				return;
			}

			ReleaseUnmanagedResources();
			if (disposing)
			{
				ReleaseManagedResources();
			}

			_disposed = true;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Bindable"/> is reclaimed by garbage collection.
		/// </summary>
		~Bindable()
		{
			Dispose(false);
		}

		/// <summary>
		/// Releases unmanaged resources during Dispose or finalization.
		/// </summary>
		/// <remarks>
		/// This method should be overriden in any derived class that creates its 
		/// own unmanaged resources. A call to the base method should always be included.
		/// </remarks>
		protected virtual void ReleaseUnmanagedResources()
		{
			UnsubscribeAllNotifications();
		}

		/// <summary>
		/// Releases managed resources during Dispose.
		/// </summary>
		/// <remarks>
		/// This method should be overriden in any derived class that creates its 
		/// own managed resources. A call to the base method should always be included.
		/// </remarks>
		protected virtual void ReleaseManagedResources()
		{
            foreach (var kvp in _registeredWrappedRelays)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.PropertyChanged -= RelayObjectPropertyChanged;
                }
            }
		}
		#endregion
	}
}
