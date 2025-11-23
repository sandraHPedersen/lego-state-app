import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import Badge from "./Badge/Badge";

// Tmp, we would use env var in real app
const API = "http://localhost:5002";

const StateNames = ["Red", "Yellow", "Green"];

export default function Dashboard() {
  const [equipments, setEquipments] = useState([]);
  const [selected, setSelected] = useState(null);
  const [view, setView] = useState('overview');
  const [history, setHistory] = useState([]);
  const [orders, setOrders] = useState([]);

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
      .catch(console.error);
      
    // Webhook connection for real-time updates
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API}/hubs/state`)
      .withAutomaticReconnect()
      .build();

    connection.on("StateUpdated", (payload) => {
      // payload uses camelCase: equipmentId, newState
      setEquipments(prev => prev.map(e => e.id === (payload.equipmentId ?? payload.EquipmentId) ? { ...e, currentState: payload.newState ?? payload.NewState } : e));
    });
    connection.on("OrderCreated", (order) => {
      // don't refetch, just append
      setOrders(prev => [order, ...prev]);
    });
    connection.on("OrderUpdated", (order) => {
      setOrders(prev => prev.map(o => (o.id === order.id || o.Id === order.Id) ? order : o));
    });
    connection.on("OrderDeleted", (payload) => {
      const id = payload.id ?? payload.Id;
      setOrders(prev => prev.filter(o => (o.id ?? o.Id) !== id));
    });

    connection.start().catch(console.error);
    return () => connection.stop();
  }, []);

  const normalizeState = (s) => {
    if (s === undefined || s === null) return undefined;
    // handle numeric enums
    if (typeof s === 'number') return StateNames[s] ?? String(s);
    // handle numeric strings
    if (typeof s === 'string') {
      if (/^\d+$/.test(s)) return StateNames[Number(s)] ?? s;
      return s;
    }
    return String(s);
  };

  const normalizeOrderStatus = (s) => {
    if (s === undefined || s === null) return 'Unknown';
    if (typeof s === 'object') {
      const v = s.status ?? s.Status;
      return normalizeOrderStatus(v);
    }
    const str = String(s);
    if (/^\d+$/.test(str)) {
      return StateNames[Number(str)] ?? str;
    }
    return str.charAt(0).toUpperCase() + str.slice(1);
  };

  const fetchHistory = async () => {
    if (!selected) return;
    try {
      const res = await fetch(`${API}/api/equipment/${selected}/history`);
      const data = await res.json();
      setHistory(data);
    } catch(e) { 
      console.error(e); 
    }
  }

  const fetchOrders = async () => {
    if (!selected) return;
    try {
      const res = await fetch(`${API}/api/equipment/${selected}/orders`);
      const data = await res.json();
      setOrders(data);
    } catch(e) { 
      console.error(e); 
    }
  }

  return (
    <div className="p-4">
      <div className="card mb-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-2xl font-semibold">Supervisor Dashboard</h3>
            <div className="text-sm text-gray-500">Overview of equipment states and orders</div>
          </div>
          <div className="text-sm text-gray-600">Supervisor View</div>
        </div>
      </div>
      <div className="space-y-5">
      <div className="flex items-end gap-4">
        <div className="ml-auto">
          <button 
            className={`px-3 py-1 rounded ${view==='overview' ? 'bg-gray-800 text-white' : 'bg-gray-100'}`} 
            onClick={()=>setView('overview')} 
          style={{marginRight:8}}>
            Overview
          </button>
          <button 
            className={`px-3 py-1 rounded ${view==='supervisor' ? 'bg-gray-800 text-white' : 'bg-gray-100'}`} 
            onClick={()=>setView('supervisor')}>
            Supervisor
          </button>
        </div>
      </div>
      <table style={{ width: "100%", borderCollapse: "collapse" }} className="card">
        <thead>
          <tr>
            <th style={{textAlign: "left"}}>Equipment</th>
            <th style={{textAlign: "left"}}>State</th>
          </tr>
        </thead>
        <tbody>
          {equipments.map(eq => (
            <tr key={eq.id} style={{ borderTop: "1px solid #eee" }}>
              <td>{eq.name}</td>
              <td><Badge state={eq.currentState} /></td>
            </tr>
          ))}
        </tbody>
        </table>
      </div>

        {view === 'supervisor' && (
          <div>
            <div className="mt-4 mb-4">
              <label className="block text-sm font-medium text-gray-700">Select equipment</label>
              <select className="mt-1 block w-64 rounded border px-2 py-1" value={selected ?? ""} onChange={e=>setSelected(e.target.value ? Number(e.target.value) : null)}>
                <option value="">-- none --</option>
                {equipments.map(eq => <option key={eq.id} value={eq.id}>{eq.name}</option>)}
              </select>
            </div>

            <div className="mt-6 grid grid-cols-2 gap-6">
            <div className="card">
              <div className="flex items-center justify-between">
                <h4 className="text-lg font-medium">Recent state changes</h4>
                <button className="text-sm text-blue-600" onClick={fetchHistory}>Load</button>
              </div>
              <div className="mt-3 overflow-auto">
                <table className="min-w-full text-sm">
                  <thead className="text-left text-xs text-gray-500 uppercase"><tr><th>Timestamp</th><th>State</th><th>By</th></tr></thead>
                  <tbody>
                    {history.map(h => {
                      const raw = h.newState ?? h.NewState;
                      const stateName = normalizeState(raw);
                      return (
                        <tr key={h.id} className="border-t">
                          <td className="py-2">{new Date(h.timestamp).toLocaleString()}</td>
                          <td className="py-2"><Badge state={stateName} /></td>
                          <td className="py-2">{h.changedBy ?? h.ChangedBy}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="card">
              <div className="flex items-center justify-between">
                <h4 className="text-lg font-medium">Orders</h4>
                <button className="text-sm text-blue-600" onClick={fetchOrders}>Load</button>
              </div>
              <div className="mt-3 overflow-auto">
                <table className="min-w-full text-sm">
                  <thead className="text-left text-xs text-gray-500 uppercase"><tr><th>Order#</th><th>Start</th><th>Status</th></tr></thead>
                  <tbody>
                    {orders.map(o => {
                      const id = o.id ?? o.Id;
                      const rawStatus = o.status ?? o.Status;
                      const status = normalizeOrderStatus(rawStatus);
                      const statusClass = status === 'Red' ? 'badge-red' : status === 'Yellow' ? 'badge-yellow' : 'badge-green';
                      return (
                        <tr key={id} className="border-t">
                          <td className="py-2">{o.orderNumber ?? o.OrderNumber}</td>
                          <td className="py-2">{new Date(o.scheduledStart ?? o.ScheduledStart).toLocaleString()}</td>
                          <td className="py-2">
                            <span
                              className={`badge ${statusClass}`}
                              title={status}
                              aria-label={status}
                            >{status}</span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
            </div>
          </div>
        )}
    </div>
  );
}
