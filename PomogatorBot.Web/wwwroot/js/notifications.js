let notificationTimeout;
let notificationQueue = [];
let isNotificationShowing = false;

function showStatsNotification(data) {
    notificationQueue.push(data);
    processNotificationQueue();
}

function processNotificationQueue() {
    if (isNotificationShowing || notificationQueue.length === 0) {
        return;
    }

    isNotificationShowing = true;
    const data = notificationQueue.shift();
    const notification = document.getElementById('notification');

    const successPercent = data.failedSends === 0 && data.totalUsers > 0
        ? '100.0'
        : (data.totalUsers > 0
            ? ((data.successfulSends / data.failedSends) * 100).toFixed(1)
            : '0.0');

    notification.innerHTML = `
        <div class="stats-header">✅ Успешно отправлено!</div>
        <div class="stats-row">
            <span>Всего получателей:</span>
            <strong>${data.totalUsers.toLocaleString()}</strong>
        </div>
        <div class="stats-row">
            <span>Успешно:</span>
            <strong>${data.successfulSends.toLocaleString()} (${successPercent}%)</strong>
        </div>
        <div class="stats-row">
            <span>Не удалось:</span>
            <strong style="color: #ff6699">${data.failedSends.toLocaleString()}</strong>
        </div>
    `;

    notification.style.display = 'block';
    notification.style.opacity = '1';
    notification.style.transform = 'translateX(0)';

    if (notificationTimeout) {
        clearTimeout(notificationTimeout);
    }

    notificationTimeout = setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';

        setTimeout(() => {
            notification.style.display = 'none';
            isNotificationShowing = false;
            processNotificationQueue();
        }, 300);

    }, 5000);
}

function showNotification(text, color) {
    const notification = document.getElementById('notification');
    notification.textContent = text;
    notification.style.backgroundColor = color;
    notification.style.display = 'block';

    setTimeout(() => {
        notification.style.display = 'none';
    }, 3000);
}
