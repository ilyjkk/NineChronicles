using System.Collections.Immutable;
using Libplanet;
using Libplanet.Action;

namespace Nekoyume.Action
{
    [ActionType("sleep")]
    public class Sleep : ActionBase
    {
        public override void LoadPlainValue(IImmutableDictionary<string, object> plainValue)
        {
            throw new System.NotImplementedException();
        }

        public override AddressStateMap Execute(Address @from, Address to, AddressStateMap states)
        {
            throw new System.NotImplementedException();
        }

        public override IImmutableDictionary<string, object> PlainValue { get; }
    }
}
