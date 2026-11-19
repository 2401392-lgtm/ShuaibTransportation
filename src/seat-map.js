(function () {
  const root = document.getElementById('seat-map');
  if (!root) return;

  const scheduleId = parseInt(root.dataset.scheduleId, 10);
  const totalSeats = parseInt(root.dataset.totalSeats, 10);
  const booked = new Set((root.dataset.bookedSeats || '').split(',').filter(Boolean).map(Number));
  const selected = new Set();
  const maxSeats = 6;

  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
  const confirmBtn = document.getElementById('confirm-booking');
  const selectedCount = document.getElementById('selected-count');
  const message = document.getElementById('booking-message');

  function render() {
    root.innerHTML = '';
    for (let i = 1; i <= totalSeats; i++) {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'seat';
      btn.textContent = i;
      if (booked.has(i)) { btn.classList.add('booked'); btn.disabled = true; }
      else if (selected.has(i)) btn.classList.add('selected');
      else btn.classList.add('available');
      btn.addEventListener('click', () => toggle(i));
      root.appendChild(btn);
    }
    selectedCount.textContent = selected.size;
    confirmBtn.disabled = selected.size === 0;
  }

  function toggle(n) {
    if (selected.has(n)) selected.delete(n);
    else {
      if (selected.size >= maxSeats) { message.textContent = `You can pick up to ${maxSeats} seats.`; return; }
      selected.add(n);
    }
    message.textContent = '';
    render();
  }

  confirmBtn.addEventListener('click', async () => {
    confirmBtn.disabled = true;
    message.className = 'alert';
    message.textContent = 'Reserving your seats…';
    try {
      const res = await fetch('/api/bookings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ scheduleId, seats: [...selected] })
      });
      const data = await res.json().catch(() => ({}));
      if (res.ok) {
        window.location.href = `/Booking/MyBookings?booked=${encodeURIComponent(data.reference)}`;
        return;
      }
      message.className = 'alert error';
      message.textContent = data.message || 'Booking failed.';
      if (res.status === 409) {
        // seats taken — refresh from server
        setTimeout(() => window.location.reload(), 1800);
      } else confirmBtn.disabled = false;
    } catch {
      message.className = 'alert error';
      message.textContent = 'Network error. Please try again.';
      confirmBtn.disabled = false;
    }
  });

  render();
})();