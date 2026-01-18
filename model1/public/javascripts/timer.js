/**
 * Global Timer State
 */
let timerState = {
    minutes: 25,
    seconds: 0,
    isRunning: false,
    interval: null,
    taskId: null,
    endTime: null
};

// Default Settings
let userSettings = {
    pomodoro: 25,
    shortBreak: 5,
    longBreak: 15
};

/**
 * --- SETTINGS FUNCTIONS (NEW) ---
 */

function openSettings() {
    // 1. Fill inputs with current values
    document.getElementById('setting-pomodoro').value = userSettings.pomodoro;
    document.getElementById('setting-short').value = userSettings.shortBreak;
    document.getElementById('setting-long').value = userSettings.longBreak;

    // 2. Show Modal
    document.getElementById('settings-modal').style.display = 'flex';
}

function closeSettings() {
    document.getElementById('settings-modal').style.display = 'none';
}

function saveSettings() {
    // 1. Get values from inputs
    const pomo = parseInt(document.getElementById('setting-pomodoro').value) || 25;
    const short = parseInt(document.getElementById('setting-short').value) || 5;
    const long = parseInt(document.getElementById('setting-long').value) || 15;

    // 2. Update global settings
    userSettings = { pomodoro: pomo, shortBreak: short, longBreak: long };

    // 3. Save to LocalStorage
    localStorage.setItem('userSettings', JSON.stringify(userSettings));

    // 4. Close Modal
    closeSettings();

    // 5. If timer is NOT running, apply changes immediately
    if (!timerState.isRunning) {
        resetTimer();
    }
}

/**
 * --- EXISTING TIMER LOGIC ---
 */

function saveState() {
    const stateToSave = {
        minutes: timerState.minutes,
        seconds: timerState.seconds,
        isRunning: timerState.isRunning,
        taskId: timerState.taskId,
        endTime: timerState.endTime,
        taskNameHTML: document.getElementById('current-task-name')?.innerHTML
    };
    localStorage.setItem('pomodoroState', JSON.stringify(stateToSave));
}

function loadState() {
    // Load User Settings First
    const savedSettings = localStorage.getItem('userSettings');
    if (savedSettings) {
        userSettings = JSON.parse(savedSettings);
    }

    const saved = localStorage.getItem('pomodoroState');
    if (!saved) {
        // Apply default setting if no state exists
        timerState.minutes = userSettings.pomodoro;
        updateDisplay();
        return;
    }

    const state = JSON.parse(saved);
    timerState.taskId = state.taskId;

    if (state.taskNameHTML) {
        const nameDisplay = document.getElementById('current-task-name');
        if (nameDisplay) {
            nameDisplay.innerHTML = state.taskNameHTML;
            nameDisplay.style.opacity = "1";
        }
    }

    if (state.taskId) highlightActiveCard(state.taskId);

    if (state.isRunning && state.endTime) {
        const now = Date.now();
        const remainingMs = state.endTime - now;

        if (remainingMs > 0) {
            const totalSeconds = Math.floor(remainingMs / 1000);
            timerState.minutes = Math.floor(totalSeconds / 60);
            timerState.seconds = totalSeconds % 60;
            timerState.endTime = state.endTime;
            timerState.isRunning = true;
            startTimerInternal();
        } else {
            timerState.minutes = 0;
            timerState.seconds = 0;
            completeTimer();
            return;
        }
    } else {
        timerState.minutes = state.minutes;
        timerState.seconds = state.seconds;
        timerState.isRunning = false;
        updateButtonText("RESUME");
        const statusDisplay = document.getElementById('status-display');
        if (statusDisplay) statusDisplay.innerText = "⏸ PAUSED";
    }
    updateDisplay();
}

function updateDisplay() {
    const minStr = String(timerState.minutes).padStart(2, '0');
    const secStr = String(timerState.seconds).padStart(2, '0');
    document.title = `${minStr}:${secStr} - Focus!`;
    const display = document.getElementById('timer-display');
    if (display) display.innerText = `${minStr}:${secStr}`;
}

function handleMainButton() {
    if (timerState.isRunning) {
        pauseTimer();
    } else {
        if (!timerState.taskId) {
            alert("⚠️ Please select a task from the list first.");
            return;
        }
        startTimer();
    }
}

function startTimer(taskId = null, taskName = null) {
    if (timerState.isRunning) return;

    if (taskId) {
        timerState.taskId = taskId;
        const nameDisplay = document.getElementById('current-task-name');
        if (nameDisplay && taskName) {
            nameDisplay.innerHTML = `Working on: <strong>${taskName}</strong>`;
        }
        highlightActiveCard(taskId);
    }

    // CALCULATE END TIME
    const now = Date.now();
    const remainingMs = (timerState.minutes * 60 + timerState.seconds) * 1000;
    timerState.endTime = now + remainingMs;

    timerState.isRunning = true;
    startTimerInternal();
}

function startTimerInternal() {
    updateButtonText("PAUSE");
    const statusDisplay = document.getElementById('status-display');
    if (statusDisplay) statusDisplay.innerText = "🔥 FOCUSING...";

    saveState();

    if (timerState.interval) clearInterval(timerState.interval);

    timerState.interval = setInterval(() => {
        const now = Date.now();
        const distance = timerState.endTime - now;

        if (distance < 0) {
            completeTimer();
        } else {
            const totalSeconds = Math.floor(distance / 1000);
            timerState.minutes = Math.floor(totalSeconds / 60);
            timerState.seconds = totalSeconds % 60;
            updateDisplay();
            saveState();
        }
    }, 1000);
}

function pauseTimer() {
    clearInterval(timerState.interval);
    timerState.isRunning = false;
    timerState.endTime = null;
    updateButtonText("RESUME");
    const statusDisplay = document.getElementById('status-display');
    if (statusDisplay) statusDisplay.innerText = "⏸ PAUSED";
    saveState();
}

function resetTimer() {
    if (timerState.interval) clearInterval(timerState.interval);
    timerState.isRunning = false;

    // USE THE SAVED SETTING HERE
    timerState.minutes = userSettings.pomodoro;
    timerState.seconds = 0;
    timerState.endTime = null;

    updateDisplay();
    updateButtonText("START");

    const statusDisplay = document.getElementById('status-display');
    if (statusDisplay) statusDisplay.innerText = "READY TO FOCUS";

    localStorage.removeItem('pomodoroState');
}

function completeTimer() {
    clearInterval(timerState.interval);
    timerState.isRunning = false;
    localStorage.removeItem('pomodoroState');

    timerState.minutes = 0;
    timerState.seconds = 0;
    updateDisplay();

    alert("🎉 Pomodoro Completed!");

    if (timerState.taskId) {
        fetch(`/tasks/${timerState.taskId}/complete`, {
            method: 'POST',
            headers: { 'Accept': 'application/json', 'Csrf-Token': 'nocheck' }
        })
        .then(response => {
            if (response.ok) {
                window.location.reload();
            }
        });
    } else {
        resetTimer();
    }
}

function highlightActiveCard(id) {
    document.querySelectorAll('.task-card').forEach(card => card.classList.remove('active-task'));
    const activeCard = document.getElementById(`task-card-${id}`);
    if (activeCard) activeCard.classList.add('active-task');
}

function updateButtonText(text) {
    const btn = document.getElementById('main-btn');
    if(btn) btn.innerText = text;
}

// Close modal when clicking outside
window.onclick = function(event) {
    const modal = document.getElementById('settings-modal');
    if (event.target == modal) {
        closeSettings();
    }
}

document.addEventListener('DOMContentLoaded', loadState);