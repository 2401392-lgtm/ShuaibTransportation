(function () {
    'use strict';

    const root = document.getElementById('seat-map');
    if (!root) return;

    const scheduleId = Number(root.dataset.scheduleId);
    const totalSeats = Number(root.dataset.totalSeats);
    const booked = new Set(
        (root.dataset.bookedSeats || '').split(',').filter(Boolean).map(Number)
    );
    const selected = new Set();
    const MAX_SEATS = 6;

    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
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

            if (booked.has(i)) {
                btn.classList.add('booked');
                btn.disabled = true;
            } else if (selected.has(i)) {
                btn.classList.add('selected');
            } else {
                btn.classList.add('available');
            }

            btn.addEventListener('click', () => toggle(i));
            root.appendChild(btn);
        }
        selectedCount.textContent = selected.size;
        if (confirmBtn) confirmBtn.disabled = selected.size === 0;
    }

    function toggle(seatNumber) {
        if (selected.has(seatNumber)) {
            selected.delete(seatNumber);
        } else {
            if (selected.size >= MAX_SEATS) {
                message.className = 'alert error';
                message.textContent = `You can select up to ${MAX_SEATS} seats.`;
                return;
            }
            selected.add(seatNumber);
        }
        message.textContent = '';
        message.className = 'alert';
        render();
    }

    if (confirmBtn && tokenInput) {
        confirmBtn.addEventListener('click', async () => {
            confirmBtn.disabled = true;
            message.className = 'alert';
            message.textContent = 'Reserving your seats…';

            try {
                const response = await fetch('/api/bookings', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': tokenInput.value
                    },
                    body: JSON.stringify({ scheduleId, seats: Array.from(selected) })
                });

                const data = await response.json().catch(() => ({}));

                if (response.ok) {
                    window.location.href =
                        `/Booking/MyBookings?booked=${encodeURIComponent(data.reference)}`;
                    return;
                }

                message.className = 'alert error';
                message.textContent = data.message || 'Booking failed. Please try again.';

                if (response.status === 409) {
                    setTimeout(() => window.location.reload(), 1800);
                } else {
                    confirmBtn.disabled = false;
                }
            } catch {
                message.className = 'alert error';
                message.textContent = 'Network error. Please try again.';
                confirmBtn.disabled = false;
            }
        });
    }

    render();
})();