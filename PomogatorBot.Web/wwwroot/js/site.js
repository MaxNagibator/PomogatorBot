let messageHistory = JSON.parse(localStorage.getItem('messageHistory')) || [];

messageHistory = messageHistory.map(msg => ({ ...msg }));

async function sendMessage() {
    const button = document.querySelector('.send-button');
    const textarea = document.querySelector('.message-field');

    const subscribes = Array.from(document.querySelectorAll('input[name="subscribes"]:checked'))
        .reduce((acc, checkbox) => acc | parseInt(checkbox.value), 0);

    textarea.classList.remove('error');

    if (!textarea.value.trim()) {
        textarea.classList.add('error');
        return;
    }

    button.classList.add('loading');
    button.innerHTML = '⏳ Отправляем...';

    try {
        const response = await fetch('/notify', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                message: textarea.value,
                subscribes
            })
        });

        const result = await response.json();

        if (response.ok) {
            const newMessage = {
                text: textarea.value,
                date: new Date().toLocaleString(),
                status: 'success',
                stats: {
                    total: result.totalUsers,
                    success: result.successfulSends,
                    failed: result.failedSends
                }
            };

            addToHistory(newMessage);
            textarea.classList.remove('error');
            textarea.value = '';
            showStatsNotification(result);
        } else {
            throw new Error(result.message || 'Ошибка отправки');
        }
    } catch (error) {
        console.error('Error:', error);

        const errorMessage = {
            text: textarea.value,
            date: new Date().toLocaleString(),
            status: 'error',
            error: error.message
        };

        addToHistory(errorMessage);
        showNotification('❌ Ошибка отправки', '#ff6699');
    } finally {
        button.classList.remove('loading');
        button.innerHTML = 'Возвестить пастве';
    }
}

document.querySelector('.message-field').addEventListener('input', function () {
    this.classList.remove('error');
    this.style.borderColor = '';
});

function addToHistory(message) {
    messageHistory.unshift(message);

    if (messageHistory.length > 100) {
        messageHistory.pop();
    }

    localStorage.setItem('messageHistory', JSON.stringify(messageHistory));
    updateHistory();
}

function updateHistory() {
    const historyList = document.getElementById('history-list');
    historyList.innerHTML = '';

    const regular = messageHistory.map((msg, index) => ({ msg, index }));

    if (regular.length > 0) {
        regular.forEach(msg => {
            const item = createMessageElement(msg.msg, msg.index);
            historyList.appendChild(item);
        });
    } else {
        historyList.innerHTML = '<div class="empty-history">📭 Нет обычных сообщений</div>';
    }
}

function createMessageElement(msg, index) {
    const item = document.createElement('div');
    item.className = `message-item ${msg.status === 'success' ? 'message-success' : 'message-error'}`;
    item.dataset.index = index;

    const safeDate = escapeHtml(msg.date);
    const safeText = escapeHtml(msg.text);

    const statsHTML = msg.stats ? `
        <div class="message-stats">
            <span class="stat-success">▲ ${msg.stats.success}</span>
            <span class="stat-failed">▼ ${msg.stats.failed}</span>
            <span class="stat-total">◼ ${msg.stats.total}</span>
        </div>
    ` : '';

    item.innerHTML = `
            <div class="message-date">${safeDate}</div>
            <div class="message-preview">${safeText}</div>
            ${statsHTML}
            <div class="message-actions">
                <div class="copy-notice">📋 Скопировано!</div>
                <div class="delete-btn">
                    <button class="action-btn delete-action" data-index="${index}">🗑️</button>
                </div>
                <div class="delete-confirm">
                    <button class="action-btn confirm-delete-btn" data-index="${index}">✅</button>
                    <button class="action-btn cancel-delete-btn">❌</button>
                </div>
            </div>
        `;

    const deleteBtn = item.querySelector('.delete-action');
    const confirmBtn = item.querySelector('.confirm-delete-btn');
    const cancelBtn = item.querySelector('.cancel-delete-btn');

    deleteBtn.addEventListener('click', () => item.classList.add('confirm-delete'));
    cancelBtn.addEventListener('click', () => item.classList.remove('confirm-delete'));
    confirmBtn.addEventListener('click', () => deleteMessage(index));

    item.addEventListener('click', e => {
        if (!e.target.closest('.message-actions')) {
            const textarea = document.querySelector('.message-field');
            textarea.value = msg.text;
            textarea.focus();
            item.classList.add('copied');
            setTimeout(() => item.classList.remove('copied'), 1000);
        }
    });

    return item;
}

function deleteMessage(index) {
    messageHistory.splice(index, 1);
    localStorage.setItem('messageHistory', JSON.stringify(messageHistory));
    updateHistory();
    showNotification('🗑️ Сообщение удалено', '#666');
}

function clearHistory() {
    if (confirm('Очистить всю историю?')) {
        messageHistory = [];
        localStorage.removeItem('messageHistory');
        updateHistory();
        showNotification('🧹 История очищена, избранные сохранены', '#00bcd4');
    }
}

function escapeHtml(unsafe) {
    return unsafe.replace(/[&<>"']/g, function (match) {
        return {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        }[match];
    });
}

document.addEventListener('DOMContentLoaded', updateHistory);
document.addEventListener('DOMContentLoaded', loadSubscriptionMeta);
