using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IrcClient
{
    public abstract class IrcEventHandler
    {
        /// <summary>
        /// Triggers the specified event, passing the provided message to all handlers.
        /// </summary>
        /// <param name="eventToTrigger">The event that is being triggered</param>
        /// <param name="message">The message that was received</param>
        /// <returns></returns>
        protected static async Task TriggerEventAsync<T>(Func<T, Task> eventToTrigger, T message)
        {
            if (eventToTrigger == null)
                return;

            var exceptionList = new List<Exception>();
            foreach (var action in eventToTrigger.GetInvocationList().Cast<Func<T, Task>>())
            {
                try
                {
                    await action(message);
                }
                catch (Exception ex)
                {
                    exceptionList.Add(ex);
                }
            }

            if (exceptionList.Any())
            {
                throw new AggregateException(exceptionList);
            }
        }

        /// <summary>
        /// Processes input and triggers appropriate events.
        /// </summary>
        /// <param name="input">The input string</param>
        public abstract Task ProcessInputAsync(string input);
    }
}
