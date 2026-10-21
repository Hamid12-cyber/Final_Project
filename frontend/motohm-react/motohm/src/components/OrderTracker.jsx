import { TRACK_STEPS, STATUS_LABELS } from '../utils/orderStatus.js';

// props: status ('Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled')
export default function OrderTracker({ status }) {
  if (status === 'Cancelled') {
    return <div className="tracker-cancelled">Bu sifariş ləğv edilib.</div>;
  }

  const current = TRACK_STEPS.indexOf(status);

  return (
    <ol className="tracker">
      {TRACK_STEPS.map((step, i) => {
        const state = i < current ? 'done' : i === current ? 'active' : 'todo';
        // Çatdırılma son addımdır: aktiv olanda da tamamlanmış kimi göstər
        const finished = step === 'Delivered' && i === current;
        return (
          <li key={step} className={`tracker-step ${finished ? 'done' : state}`}>
            <span className="tracker-dot">{state === 'done' || finished ? '✓' : i + 1}</span>
            <span className="tracker-label">{STATUS_LABELS[step]}</span>
          </li>
        );
      })}
    </ol>
  );
}