using Abc.IdentityModel.Protocols.EidasLight;

namespace Abc.IdentityServer.EidasLight.Stores
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.ValidationResult" />
    public class IgniteProxyGetResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IgniteProxyGetResult"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="errorDescription">The error description.</param>
        public IgniteProxyGetResult(string error, string errorDescription = null)
        {
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgniteProxyGetResult"/> class.
        /// </summary>
        /// <param name="request">The eIDAS light request.</param>
        /// <exception cref="System.ArgumentNullException">request is <c>null</c>.</exception>
        public IgniteProxyGetResult(EidasLightRequest request)
        {
            IsError = false;
            Request = request ?? throw new System.ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Gets the eIDAS light request.
        /// </summary>
        /// <value>
        /// The eIDAS light request.
        /// </value>
        public EidasLightRequest Request { get; }
    }
}