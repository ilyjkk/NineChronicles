// <auto-generated />
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace MagicOnion
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::MagicOnion;
    using global::MagicOnion.Client;

    public static partial class MagicOnionInitializer
    {
        static bool isRegistered = false;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            if(isRegistered) return;
            isRegistered = true;

            MagicOnionClientRegistry<Nekoyume.Shared.Services.IBlockChainService>.Register((x, y, z) => new Nekoyume.Shared.Services.BlockChainServiceClient(x, y, z));

            StreamingHubClientRegistry<Nekoyume.Shared.Hubs.IActionEvaluationHub, Nekoyume.Shared.Hubs.IActionEvaluationHubReceiver>.Register((a, _, b, c, d, e) => new Nekoyume.Shared.Hubs.ActionEvaluationHubClient(a, b, c, d, e));
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace MagicOnion.Resolvers
{
    using System;
    using MessagePack;

    public class MagicOnionResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new MagicOnionResolver();

        MagicOnionResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = MagicOnionResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class MagicOnionResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static MagicOnionResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(8)
            {
                {typeof(global::MagicOnion.DynamicArgumentTuple<byte[], byte[], byte[]>), 0 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<byte[], byte[]>), 1 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<byte[], global::System.Collections.Generic.IEnumerable<byte[]>>), 2 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>), 3 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<int, string>), 4 },
                {typeof(global::MagicOnion.DynamicArgumentTuple<string, string>), 5 },
                {typeof(global::System.Collections.Generic.Dictionary<byte[], byte[]>), 6 },
                {typeof(global::System.Collections.Generic.IEnumerable<byte[]>), 7 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MagicOnion.DynamicArgumentTupleFormatter<byte[], byte[], byte[]>(default(byte[]), default(byte[]), default(byte[]));
                case 1: return new global::MagicOnion.DynamicArgumentTupleFormatter<byte[], byte[]>(default(byte[]), default(byte[]));
                case 2: return new global::MagicOnion.DynamicArgumentTupleFormatter<byte[], global::System.Collections.Generic.IEnumerable<byte[]>>(default(byte[]), default(global::System.Collections.Generic.IEnumerable<byte[]>));
                case 3: return new global::MagicOnion.DynamicArgumentTupleFormatter<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>(default(global::System.Collections.Generic.IEnumerable<byte[]>), default(byte[]));
                case 4: return new global::MagicOnion.DynamicArgumentTupleFormatter<int, string>(default(int), default(string));
                case 5: return new global::MagicOnion.DynamicArgumentTupleFormatter<string, string>(default(string), default(string));
                case 6: return new global::MessagePack.Formatters.DictionaryFormatter<byte[], byte[]>();
                case 7: return new global::MessagePack.Formatters.InterfaceEnumerableFormatter<byte[]>();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Nekoyume.Shared.Services {
    using System;
    using MagicOnion;
    using MagicOnion.Client;
    using Grpc.Core;
    using MessagePack;

    [Ignore]
    public class BlockChainServiceClient : MagicOnionClientBase<global::Nekoyume.Shared.Services.IBlockChainService>, global::Nekoyume.Shared.Services.IBlockChainService
    {
        static readonly Method<byte[], byte[]> PutTransactionMethod;
        static readonly Func<RequestContext, ResponseContext> PutTransactionDelegate;
        static readonly Method<byte[], byte[]> GetNextTxNonceMethod;
        static readonly Func<RequestContext, ResponseContext> GetNextTxNonceDelegate;
        static readonly Method<byte[], byte[]> GetStateMethod;
        static readonly Func<RequestContext, ResponseContext> GetStateDelegate;
        static readonly Method<byte[], byte[]> GetBalanceMethod;
        static readonly Func<RequestContext, ResponseContext> GetBalanceDelegate;
        static readonly Method<byte[], byte[]> GetTipMethod;
        static readonly Func<RequestContext, ResponseContext> GetTipDelegate;
        static readonly Method<byte[], byte[]> SetAddressesToSubscribeMethod;
        static readonly Func<RequestContext, ResponseContext> SetAddressesToSubscribeDelegate;
        static readonly Method<byte[], byte[]> IsTransactionStagedMethod;
        static readonly Func<RequestContext, ResponseContext> IsTransactionStagedDelegate;
        static readonly Method<byte[], byte[]> ReportExceptionMethod;
        static readonly Func<RequestContext, ResponseContext> ReportExceptionDelegate;
        static readonly Method<byte[], byte[]> AddClientMethod;
        static readonly Func<RequestContext, ResponseContext> AddClientDelegate;
        static readonly Method<byte[], byte[]> RemoveClientMethod;
        static readonly Func<RequestContext, ResponseContext> RemoveClientDelegate;
        static readonly Method<byte[], byte[]> GetAvatarStatesMethod;
        static readonly Func<RequestContext, ResponseContext> GetAvatarStatesDelegate;
        static readonly Method<byte[], byte[]> GetStateBulkMethod;
        static readonly Func<RequestContext, ResponseContext> GetStateBulkDelegate;

        static BlockChainServiceClient()
        {
            PutTransactionMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "PutTransaction", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            PutTransactionDelegate = _PutTransaction;
            GetNextTxNonceMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetNextTxNonce", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetNextTxNonceDelegate = _GetNextTxNonce;
            GetStateMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetState", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetStateDelegate = _GetState;
            GetBalanceMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetBalance", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetBalanceDelegate = _GetBalance;
            GetTipMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetTip", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetTipDelegate = _GetTip;
            SetAddressesToSubscribeMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "SetAddressesToSubscribe", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            SetAddressesToSubscribeDelegate = _SetAddressesToSubscribe;
            IsTransactionStagedMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "IsTransactionStaged", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            IsTransactionStagedDelegate = _IsTransactionStaged;
            ReportExceptionMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "ReportException", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            ReportExceptionDelegate = _ReportException;
            AddClientMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "AddClient", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            AddClientDelegate = _AddClient;
            RemoveClientMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "RemoveClient", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            RemoveClientDelegate = _RemoveClient;
            GetAvatarStatesMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetAvatarStates", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetAvatarStatesDelegate = _GetAvatarStates;
            GetStateBulkMethod = new Method<byte[], byte[]>(MethodType.Unary, "IBlockChainService", "GetStateBulk", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);
            GetStateBulkDelegate = _GetStateBulk;
        }

        BlockChainServiceClient()
        {
        }

        public BlockChainServiceClient(CallInvoker callInvoker, MessagePackSerializerOptions serializerOptions, IClientFilter[] filters)
            : base(callInvoker, serializerOptions, filters)
        {
        }

        protected override MagicOnionClientBase<IBlockChainService> Clone()
        {
            var clone = new BlockChainServiceClient();
            clone.host = this.host;
            clone.option = this.option;
            clone.callInvoker = this.callInvoker;
            clone.serializerOptions = this.serializerOptions;
            clone.filters = filters;
            return clone;
        }

        public new IBlockChainService WithHeaders(Metadata headers)
        {
            return base.WithHeaders(headers);
        }

        public new IBlockChainService WithCancellationToken(System.Threading.CancellationToken cancellationToken)
        {
            return base.WithCancellationToken(cancellationToken);
        }

        public new IBlockChainService WithDeadline(System.DateTime deadline)
        {
            return base.WithDeadline(deadline);
        }

        public new IBlockChainService WithHost(string host)
        {
            return base.WithHost(host);
        }

        public new IBlockChainService WithOptions(CallOptions option)
        {
            return base.WithOptions(option);
        }
   
        static ResponseContext _PutTransaction(RequestContext __context)
        {
            return CreateResponseContext<byte[], bool>(__context, PutTransactionMethod);
        }

        public global::MagicOnion.UnaryResult<bool> PutTransaction(byte[] txBytes)
        {
            return InvokeAsync<byte[], bool>("IBlockChainService/PutTransaction", txBytes, PutTransactionDelegate);
        }
        static ResponseContext _GetNextTxNonce(RequestContext __context)
        {
            return CreateResponseContext<byte[], long>(__context, GetNextTxNonceMethod);
        }

        public global::MagicOnion.UnaryResult<long> GetNextTxNonce(byte[] addressBytes)
        {
            return InvokeAsync<byte[], long>("IBlockChainService/GetNextTxNonce", addressBytes, GetNextTxNonceDelegate);
        }
        static ResponseContext _GetState(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<byte[], byte[]>, byte[]>(__context, GetStateMethod);
        }

        public global::MagicOnion.UnaryResult<byte[]> GetState(byte[] addressBytes, byte[] blockHashBytes)
        {
            return InvokeAsync<DynamicArgumentTuple<byte[], byte[]>, byte[]>("IBlockChainService/GetState", new DynamicArgumentTuple<byte[], byte[]>(addressBytes, blockHashBytes), GetStateDelegate);
        }
        static ResponseContext _GetBalance(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<byte[], byte[], byte[]>, byte[]>(__context, GetBalanceMethod);
        }

        public global::MagicOnion.UnaryResult<byte[]> GetBalance(byte[] addressBytes, byte[] currencyBytes, byte[] blockHashBytes)
        {
            return InvokeAsync<DynamicArgumentTuple<byte[], byte[], byte[]>, byte[]>("IBlockChainService/GetBalance", new DynamicArgumentTuple<byte[], byte[], byte[]>(addressBytes, currencyBytes, blockHashBytes), GetBalanceDelegate);
        }
        static ResponseContext _GetTip(RequestContext __context)
        {
            return CreateResponseContext<byte[]>(__context, GetTipMethod);
        }

        public global::MagicOnion.UnaryResult<byte[]> GetTip()
        {
            return InvokeAsync<Nil, byte[]>("IBlockChainService/GetTip", Nil.Default, GetTipDelegate);
        }
        static ResponseContext _SetAddressesToSubscribe(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<byte[], global::System.Collections.Generic.IEnumerable<byte[]>>, bool>(__context, SetAddressesToSubscribeMethod);
        }

        public global::MagicOnion.UnaryResult<bool> SetAddressesToSubscribe(byte[] toByteArray, global::System.Collections.Generic.IEnumerable<byte[]> addressesBytes)
        {
            return InvokeAsync<DynamicArgumentTuple<byte[], global::System.Collections.Generic.IEnumerable<byte[]>>, bool>("IBlockChainService/SetAddressesToSubscribe", new DynamicArgumentTuple<byte[], global::System.Collections.Generic.IEnumerable<byte[]>>(toByteArray, addressesBytes), SetAddressesToSubscribeDelegate);
        }
        static ResponseContext _IsTransactionStaged(RequestContext __context)
        {
            return CreateResponseContext<byte[], bool>(__context, IsTransactionStagedMethod);
        }

        public global::MagicOnion.UnaryResult<bool> IsTransactionStaged(byte[] txidBytes)
        {
            return InvokeAsync<byte[], bool>("IBlockChainService/IsTransactionStaged", txidBytes, IsTransactionStagedDelegate);
        }
        static ResponseContext _ReportException(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<string, string>, bool>(__context, ReportExceptionMethod);
        }

        public global::MagicOnion.UnaryResult<bool> ReportException(string code, string message)
        {
            return InvokeAsync<DynamicArgumentTuple<string, string>, bool>("IBlockChainService/ReportException", new DynamicArgumentTuple<string, string>(code, message), ReportExceptionDelegate);
        }
        static ResponseContext _AddClient(RequestContext __context)
        {
            return CreateResponseContext<byte[], bool>(__context, AddClientMethod);
        }

        public global::MagicOnion.UnaryResult<bool> AddClient(byte[] addressByte)
        {
            return InvokeAsync<byte[], bool>("IBlockChainService/AddClient", addressByte, AddClientDelegate);
        }
        static ResponseContext _RemoveClient(RequestContext __context)
        {
            return CreateResponseContext<byte[], bool>(__context, RemoveClientMethod);
        }

        public global::MagicOnion.UnaryResult<bool> RemoveClient(byte[] addressByte)
        {
            return InvokeAsync<byte[], bool>("IBlockChainService/RemoveClient", addressByte, RemoveClientDelegate);
        }
        static ResponseContext _GetAvatarStates(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>, global::System.Collections.Generic.Dictionary<byte[], byte[]>>(__context, GetAvatarStatesMethod);
        }

        public global::MagicOnion.UnaryResult<global::System.Collections.Generic.Dictionary<byte[], byte[]>> GetAvatarStates(global::System.Collections.Generic.IEnumerable<byte[]> addressBytesList, byte[] blockHashBytes)
        {
            return InvokeAsync<DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>, global::System.Collections.Generic.Dictionary<byte[], byte[]>>("IBlockChainService/GetAvatarStates", new DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>(addressBytesList, blockHashBytes), GetAvatarStatesDelegate);
        }
        static ResponseContext _GetStateBulk(RequestContext __context)
        {
            return CreateResponseContext<DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>, global::System.Collections.Generic.Dictionary<byte[], byte[]>>(__context, GetStateBulkMethod);
        }

        public global::MagicOnion.UnaryResult<global::System.Collections.Generic.Dictionary<byte[], byte[]>> GetStateBulk(global::System.Collections.Generic.IEnumerable<byte[]> addressBytesList, byte[] blockHashBytes)
        {
            return InvokeAsync<DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>, global::System.Collections.Generic.Dictionary<byte[], byte[]>>("IBlockChainService/GetStateBulk", new DynamicArgumentTuple<global::System.Collections.Generic.IEnumerable<byte[]>, byte[]>(addressBytesList, blockHashBytes), GetStateBulkDelegate);
        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Nekoyume.Shared.Hubs {
    using Grpc.Core;
    using MagicOnion;
    using MagicOnion.Client;
    using MessagePack;
    using System;
    using System.Threading.Tasks;

    [Ignore]
    public class ActionEvaluationHubClient : StreamingHubClientBase<global::Nekoyume.Shared.Hubs.IActionEvaluationHub, global::Nekoyume.Shared.Hubs.IActionEvaluationHubReceiver>, global::Nekoyume.Shared.Hubs.IActionEvaluationHub
    {
        static readonly Method<byte[], byte[]> method = new Method<byte[], byte[]>(MethodType.DuplexStreaming, "IActionEvaluationHub", "Connect", MagicOnionMarshallers.ThroughMarshaller, MagicOnionMarshallers.ThroughMarshaller);

        protected override Method<byte[], byte[]> DuplexStreamingAsyncMethod { get { return method; } }

        readonly global::Nekoyume.Shared.Hubs.IActionEvaluationHub __fireAndForgetClient;

        public ActionEvaluationHubClient(CallInvoker callInvoker, string host, CallOptions option, MessagePackSerializerOptions serializerOptions, IMagicOnionClientLogger logger)
            : base(callInvoker, host, option, serializerOptions, logger)
        {
            this.__fireAndForgetClient = new FireAndForgetClient(this);
        }
        
        public global::Nekoyume.Shared.Hubs.IActionEvaluationHub FireAndForget()
        {
            return __fireAndForgetClient;
        }

        protected override void OnBroadcastEvent(int methodId, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case 1092973952: // OnRender
                {
                    UnityEngine.Debug.Log("MagicOnion OnRender");
                    var result = MessagePackSerializer.Deserialize<byte[]>(data, serializerOptions);
                    receiver.OnRender(result); break;
                }
                case -1668462809: // OnUnrender
                {
                    var result = MessagePackSerializer.Deserialize<byte[]>(data, serializerOptions);
                    receiver.OnUnrender(result); break;
                }
                case -1243684591: // OnRenderBlock
                {
                    var result = MessagePackSerializer.Deserialize<DynamicArgumentTuple<byte[], byte[]>>(data, serializerOptions);
                    receiver.OnRenderBlock(result.Item1, result.Item2); break;
                }
                case 1940953564: // OnReorged
                {
                    var result = MessagePackSerializer.Deserialize<DynamicArgumentTuple<byte[], byte[], byte[]>>(data, serializerOptions);
                    receiver.OnReorged(result.Item1, result.Item2, result.Item3); break;
                }
                case 1034302074: // OnReorgEnd
                {
                    var result = MessagePackSerializer.Deserialize<DynamicArgumentTuple<byte[], byte[], byte[]>>(data, serializerOptions);
                    receiver.OnReorgEnd(result.Item1, result.Item2, result.Item3); break;
                }
                case 1163840065: // OnException
                {
                    var result = MessagePackSerializer.Deserialize<DynamicArgumentTuple<int, string>>(data, serializerOptions);
                    receiver.OnException(result.Item1, result.Item2); break;
                }
                case -1340987425: // OnPreloadStart
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    receiver.OnPreloadStart(); break;
                }
                case 1916412074: // OnPreloadEnd
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    receiver.OnPreloadEnd(); break;
                }
                default:
                    break;
            }
        }

        protected override void OnResponseEvent(int methodId, object taskCompletionSource, ArraySegment<byte> data)
        {
            switch (methodId)
            {
                case -733403293: // JoinAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1368362116: // LeaveAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -291080696: // BroadcastRenderAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -1141315011: // BroadcastUnrenderAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -493480153: // BroadcastRenderBlockAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1187694116: // ReportReorgAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 697389103: // ReportReorgEndAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case 1773826856: // ReportExceptionAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -662856294: // PreloadStartAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                case -486331643: // PreloadEndAsync
                {
                    var result = MessagePackSerializer.Deserialize<Nil>(data, serializerOptions);
                    ((TaskCompletionSource<Nil>)taskCompletionSource).TrySetResult(result);
                    break;
                }
                default:
                    break;
            }
        }
   
        public global::System.Threading.Tasks.Task JoinAsync(string addressHex)
        {
            return WriteMessageWithResponseAsync<string, Nil>(-733403293, addressHex);
        }

        public global::System.Threading.Tasks.Task LeaveAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(1368362116, Nil.Default);
        }

        public global::System.Threading.Tasks.Task BroadcastRenderAsync(byte[] encoded)
        {
            return WriteMessageWithResponseAsync<byte[], Nil>(-291080696, encoded);
        }

        public global::System.Threading.Tasks.Task BroadcastUnrenderAsync(byte[] encoded)
        {
            return WriteMessageWithResponseAsync<byte[], Nil>(-1141315011, encoded);
        }

        public global::System.Threading.Tasks.Task BroadcastRenderBlockAsync(byte[] oldTip, byte[] newTip)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<byte[], byte[]>, Nil>(-493480153, new DynamicArgumentTuple<byte[], byte[]>(oldTip, newTip));
        }

        public global::System.Threading.Tasks.Task ReportReorgAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<byte[], byte[], byte[]>, Nil>(1187694116, new DynamicArgumentTuple<byte[], byte[], byte[]>(oldTip, newTip, branchpoint));
        }

        public global::System.Threading.Tasks.Task ReportReorgEndAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<byte[], byte[], byte[]>, Nil>(697389103, new DynamicArgumentTuple<byte[], byte[], byte[]>(oldTip, newTip, branchpoint));
        }

        public global::System.Threading.Tasks.Task ReportExceptionAsync(int code, string message)
        {
            return WriteMessageWithResponseAsync<DynamicArgumentTuple<int, string>, Nil>(1773826856, new DynamicArgumentTuple<int, string>(code, message));
        }

        public global::System.Threading.Tasks.Task PreloadStartAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(-662856294, Nil.Default);
        }

        public global::System.Threading.Tasks.Task PreloadEndAsync()
        {
            return WriteMessageWithResponseAsync<Nil, Nil>(-486331643, Nil.Default);
        }

        [Ignore]
        class FireAndForgetClient : global::Nekoyume.Shared.Hubs.IActionEvaluationHub
        {
            readonly ActionEvaluationHubClient __parent;

            public FireAndForgetClient(ActionEvaluationHubClient parentClient)
            {
                this.__parent = parentClient;
            }

            public global::Nekoyume.Shared.Hubs.IActionEvaluationHub FireAndForget()
            {
                throw new NotSupportedException();
            }

            public Task DisposeAsync()
            {
                throw new NotSupportedException();
            }

            public Task WaitForDisconnect()
            {
                throw new NotSupportedException();
            }

            public global::System.Threading.Tasks.Task JoinAsync(string addressHex)
            {
                return __parent.WriteMessageAsync<string>(-733403293, addressHex);
            }

            public global::System.Threading.Tasks.Task LeaveAsync()
            {
                return __parent.WriteMessageAsync<Nil>(1368362116, Nil.Default);
            }

            public global::System.Threading.Tasks.Task BroadcastRenderAsync(byte[] encoded)
            {
                return __parent.WriteMessageAsync<byte[]>(-291080696, encoded);
            }

            public global::System.Threading.Tasks.Task BroadcastUnrenderAsync(byte[] encoded)
            {
                return __parent.WriteMessageAsync<byte[]>(-1141315011, encoded);
            }

            public global::System.Threading.Tasks.Task BroadcastRenderBlockAsync(byte[] oldTip, byte[] newTip)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<byte[], byte[]>>(-493480153, new DynamicArgumentTuple<byte[], byte[]>(oldTip, newTip));
            }

            public global::System.Threading.Tasks.Task ReportReorgAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<byte[], byte[], byte[]>>(1187694116, new DynamicArgumentTuple<byte[], byte[], byte[]>(oldTip, newTip, branchpoint));
            }

            public global::System.Threading.Tasks.Task ReportReorgEndAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<byte[], byte[], byte[]>>(697389103, new DynamicArgumentTuple<byte[], byte[], byte[]>(oldTip, newTip, branchpoint));
            }

            public global::System.Threading.Tasks.Task ReportExceptionAsync(int code, string message)
            {
                return __parent.WriteMessageAsync<DynamicArgumentTuple<int, string>>(1773826856, new DynamicArgumentTuple<int, string>(code, message));
            }

            public global::System.Threading.Tasks.Task PreloadStartAsync()
            {
                return __parent.WriteMessageAsync<Nil>(-662856294, Nil.Default);
            }

            public global::System.Threading.Tasks.Task PreloadEndAsync()
            {
                return __parent.WriteMessageAsync<Nil>(-486331643, Nil.Default);
            }

        }
    }
}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
