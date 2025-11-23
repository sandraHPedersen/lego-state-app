import { useEffect, useState } from "react";
import { HubConnectionBuilder, HttpTransportType } from "@microsoft/signalr";

// Tmp, we would use env var in real app
const API = "http://localhost:5002";

export default function MobileControl() {
  const [equipments, setEquipments] = useState([]);
  const [selected, setSelected] = useState(null);
  const [user, setUser] = useState("worker-1");
  const [currentOrder, setCurrentOrder] = useState(null);
  const [scheduledOrders, setScheduledOrders] = useState([]);
  const [connState, setConnState] = useState('disconnected');

  useEffect(() => {
    fetch(`${API}/api/equipment`)
      .then(r => r.json())
      .then(data => {
        const names = ["Red", "Yellow", "Green"];
        const normalized = (data || []).map(eq => ({
          id: eq.id ?? eq.Id,
          name: eq.name ?? eq.Name,
          currentState: typeof eq.currentState !== 'undefined'
            ? (typeof eq.currentState === 'number' ? names[eq.currentState] : eq.currentState)
            : (typeof eq.CurrentState !== 'undefined'
                ? (typeof eq.CurrentState === 'number' ? names[eq.CurrentState] : eq.CurrentState)
                : undefined)
        }));
        setEquipments(normalized);
      })
      .catch(err => console.error(err));
      
    // Listen for order broadcasts to update current/scheduled orders in real-time
    const conn = new HubConnectionBuilder()
      .withUrl(`${API}/hubs/state`)
      .withAutomaticReconnect()
      .build();

    conn.on('OrderCreated', (order) => {
      if (order.equipmentId === selected || order.EquipmentId === selected) {
        setScheduledOrders(prev => [order, ...prev]);
      }
    });
    conn.on('OrderUpdated', (order) => {
      if ((order.equipmentId ?? order.EquipmentId) === selected) {
        setScheduledOrders(prev => prev.map(o => (o.id === order.id || o.Id === order.Id) ? order : o));
        if ((currentOrder?.id ?? currentOrder?.Id) === (order.id ?? order.Id)) setCurrentOrder(order);
      }
    });
    conn.on('OrderDeleted', (payload) => {
      const id = payload.id ?? payload.Id;
      setScheduledOrders(prev => prev.filter(o => (o.id ?? o.Id) !== id));
      if ((currentOrder?.id ?? currentOrder?.Id) === id) setCurrentOrder(null);
    });
    conn.onreconnecting((err) => { console.warn('SignalR reconnecting', err); setConnState('reconnecting'); });
    conn.onreconnected(() => { console.info('SignalR reconnected'); setConnState('connected'); });
    conn.onclose((err) => { console.warn('SignalR connection closed', err); setConnState('disconnected'); });
    setConnState('connecting');
    conn.start().then(()=> setConnState('connected')).catch(err => {
      console.error('SignalR start failed (first attempt):', err);
      setConnState('disconnected');
      // try fallback to long polling
      conn.start({ transport: HttpTransportType.LongPolling }).then(()=> setConnState('connected')).catch(err2 => { console.error('SignalR long-polling fallback failed:', err2); setConnState('disconnected'); });
    });
  }, []);

  useEffect(()=>{
    if (!selected) return;
    (async ()=>{
      try {
        const curRes = await fetch(`${API}/api/equipment/${selected}/current-order`);
        if (curRes.status === 200) setCurrentOrder(await curRes.json());
        else setCurrentOrder(null);
        const ordRes = await fetch(`${API}/api/equipment/${selected}/orders`);
        if (ordRes.status === 200) setScheduledOrders(await ordRes.json());
      } catch(e){ console.error(e); }
    })();
    // Subscribe to hub group for this equipment so we receive targeted updates
    if (window.__rhub && window.__rhub.invoke) {
      try { window.__rhub.invoke('Subscribe', selected); } catch(e){ console.warn('subscribe failed', e); }
    }

    return () => {
      if (window.__rhub && window.__rhub.invoke) {
        try { window.__rhub.invoke('Unsubscribe', selected); } catch(e){ console.warn('unsubscribe failed', e);}
      }
    };
  }, [selected]);

  const updateState = async (equipmentId, state) => {
    if (!equipmentId) {
      alert('Please select equipment first');
      return;
    }
    const stateMap = { Red: 0, Yellow: 1, Green: 2 };
    try {
      await fetch(`${API}/api/equipment/${equipmentId}/state`, {
        method: "PATCH",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newState: stateMap[state], changedBy: user })
      });
    } catch (err) {
      console.error(err);
      alert('Failed to update state');
    }
  };

  return (
    <div className="card max-w-xl mx-auto">
      <div className="flex items-center justify-between">
        <h3 className="text-xl font-semibold">Mobile Control</h3>
        <div>
          <span className={`px-2 py-1 rounded text-sm font-medium ${connState === 'connected' ? 'bg-green-100 text-green-800' : connState === 'reconnecting' ? 'bg-yellow-100 text-yellow-800' : 'bg-red-100 text-red-800'}`}>
            {connState}
          </span>
        </div>
      </div>

      <div className="mt-4 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Worker name</label>
          <input className="mt-1 block w-full rounded border px-2 py-1" value={user} onChange={(e)=>setUser(e.target.value)} />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Select equipment</label>
          <select className="mt-1 block w-full rounded border px-2 py-1" value={selected ?? ""} onChange={e=>setSelected(e.target.value ? Number(e.target.value) : null)}>
            <option value="">-- choose --</option>
            {equipments.map(eq => <option key={eq.id} value={eq.id}>{eq.name}</option>)}
          </select>
        </div>

        {selected && (
          <div className="bg-gray-50 p-3 rounded">
            <h4 className="font-medium">Assigned orders</h4>
            <div className="mt-2">
              <div className="text-sm"><strong>Current:</strong> {currentOrder ? `${currentOrder.orderNumber ?? currentOrder.OrderNumber} (${currentOrder.status ?? currentOrder.Status})` : 'None'}</div>
            </div>
            <div className="mt-3">
              <div className="text-sm font-medium">Scheduled</div>
              <div className="mt-2">
                <table className="min-w-full text-sm">
                  <thead className="text-left text-xs text-gray-500 uppercase"><tr><th>Order</th><th>Start</th><th>Status</th></tr></thead>
                  <tbody>
                    {scheduledOrders.map(o => (
                      <tr key={o.id} className="border-t"><td className="py-2">{o.orderNumber ?? o.OrderNumber}</td><td className="py-2">{new Date(o.scheduledStart ?? o.ScheduledStart).toLocaleString()}</td><td className="py-2"><span className={`badge ${((o.status ?? o.Status) === 'InProgress') ? 'badge-green' : ((o.status ?? o.Status) === 'Scheduled') ? 'badge-yellow' : 'badge-red'}`}>{o.status ?? o.Status}</span></td></tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}

        <div className="flex gap-3">
          <button disabled={!selected} onClick={()=>updateState(selected, "Red")} className="flex-1 px-4 py-2 rounded bg-red-600 text-white">Red</button>
          <button disabled={!selected} onClick={()=>updateState(selected, "Yellow")} className="flex-1 px-4 py-2 rounded bg-yellow-400">Yellow</button>
          <button disabled={!selected} onClick={()=>updateState(selected, "Green")} className="flex-1 px-4 py-2 rounded bg-green-600 text-white">Green</button>
        </div>
      </div>
    </div>
  );
}
