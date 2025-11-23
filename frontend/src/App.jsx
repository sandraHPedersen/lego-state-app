import Dashboard from "./Components/Dashboard";
import MobileControl from "./Components/MobileController";

export default function App() {
  return (
    <div style={{ fontFamily: "Arial, sans-serif", padding: 16 }}>
      <h2>Production Pulse</h2>
      <div style={{color:'#666'}}>Live equipment states - immediate updates and history</div>
      <div style={{ display: "flex", gap: 24 }}>
        <div style={{ flex: 2 }}>
          <Dashboard />
        </div>
        <div style={{ width: 320 }}>
          <MobileControl />
        </div>
      </div>
    </div>
  );
}
