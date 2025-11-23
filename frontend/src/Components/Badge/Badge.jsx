function Badge({ state }) {
  const colors = { Red: "#ff4d4f", Yellow: "#fadb14", Green: "#52c41a" };
  return (
    <div
      title={state}
      role="img"
      aria-label={state}
      style={{
        width: 20,
        height: 20,
        borderRadius: 4,
        background: colors[state] || "#ccc",
        display: "inline-block",
        marginRight: 8
      }}
    />
  );
}

export default Badge;