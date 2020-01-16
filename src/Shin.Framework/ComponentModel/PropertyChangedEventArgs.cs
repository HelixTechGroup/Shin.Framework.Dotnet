namespace Shin.Framework.ComponentModel
{
    public class PropertyChangedEventArgs<T> : PropertyChangingEventArgs<T>
    {
        #region Members
        private readonly T m_previousValue;
        #endregion

        #region Properties
        public T PreviousValue
        {
            get { return m_previousValue; }
        }
        #endregion

        public PropertyChangedEventArgs(T currentValue, T requestedValue, T previousValue) : base(currentValue, requestedValue)
        {
            m_previousValue = previousValue;
        }
    }
}