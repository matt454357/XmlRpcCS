using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Nwc.XmlRpc
{
    /// <summary>This class provides support for creating local proxies of XML-RPC remote objects</summary>
    /// <remarks>
    ///     To create a local proxy you need to create a local C# interface and then, via <i>createProxy</i>
    ///     associate that interface with a remote object at a given URL.
    /// </remarks>
    public class XmlRpcClientProxy : RealProxy
    {
        private readonly XmlRpcRequest _client = new XmlRpcRequest();
        private readonly string _remoteObjectName;
        private readonly string _url;

        private XmlRpcClientProxy(string remoteObjectName, string url, Type t) : base(t)
        {
            _remoteObjectName = remoteObjectName;
            _url = url;
        }

        /// <summary>Factory method to create proxies.</summary>
        /// <remarks>
        ///     To create a local proxy you need to create a local C# interface with methods that mirror those of the server
        ///     object.
        ///     Next, pass that interface into <c>createProxy</c> along with the object name and URL of the remote object and
        ///     cast the resulting object to the specifice interface.
        /// </remarks>
        /// <param name="remoteObjectName"><c>String</c> The name of the remote object.</param>
        /// <param name="url"><c>String</c> The URL of the remote object.</param>
        /// <param name="anInterface"><c>Type</c> The typeof() of a C# interface.</param>
        /// <returns><c>Object</c> A proxy for your specified interface. Cast to appropriate type.</returns>
        public static object createProxy(string remoteObjectName, string url, Type anInterface)
        {
            return new XmlRpcClientProxy(remoteObjectName, url, anInterface).GetTransparentProxy();
        }

        /// <summary>The local method dispatcher - do not invoke.</summary>
        public override IMessage Invoke(IMessage msg)
        {
            var methodMessage = (IMethodCallMessage) msg;

            _client.MethodName = _remoteObjectName + "." + methodMessage.MethodName;
            _client.Params.Clear();
            foreach (var o in methodMessage.Args)
                _client.Params.Add(o);

            try
            {
                var ret = _client.Invoke(_url);
                return new ReturnMessage(ret, null, 0,
                    methodMessage.LogicalCallContext, methodMessage);
            }
            catch (Exception e)
            {
                return new ReturnMessage(e, methodMessage);
            }
        }
    }
}