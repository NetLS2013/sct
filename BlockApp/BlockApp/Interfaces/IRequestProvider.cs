using System.Threading.Tasks;

namespace BlockApp.Interfaces
{
    public interface IRequestProvider
    {
        /// <summary>
        /// Requests data from a specified resource.
        /// </summary>
        /// <param name="uri">Link to resource.</param>
        /// <typeparam name="TResult">TResult deserialized from json.</typeparam>
        /// <returns>Returns deserialize object.</returns>
        Task<TResult> GetAsync<TResult>(string uri);
        
        /// <summary>
        /// Submits data to be processed to a specified resource.
        /// </summary>
        /// <param name="uri">Link to resource.</param>
        /// <param name="data">name/value pairs sent in the HTTP message body.</param>
        /// <typeparam name="TData">TData object that will be serialize.</typeparam>
        /// <typeparam name="TResult">TResult deserialized from json.</typeparam>
        /// <returns>Returns deserialize object.</returns>
        Task<TResult> PostAsync<TData, TResult>(string uri, TData data);
        
        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        /// <param name="uri">Link to resource.</param>
        Task DeleteAsync(string uri);
    }
}