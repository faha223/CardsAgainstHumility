using System;

namespace CardsAgainstHumility.Interfaces
{
    public interface ISocket
    {
        void Connect();

        void On(string eventName, Action<object[]> listener);

        void Emit(string message);

        void Emit(string message, string messageBody);

        void Close();
    }
}